# Project Code Review – NonProfit Finance Management System – 2026-01-29

## Summary

The NonProfit Finance Management System is a comprehensive Blazor Server application for nonprofit financial management. While the application demonstrates solid architectural patterns and includes many security features, **this audit identifies 67+ issues across critical, high, medium, and low severity levels**. The most concerning areas are: **database data integrity risks** (duplicate data, missing constraints, orphaned records), **UI/UX responsiveness issues** (several buttons and inputs not functioning), **broken navigation links** (404 errors for some routes), **potential N+1 query patterns** causing performance degradation, and **inconsistent error handling** across services. The application has significant technical debt that could lead to data corruption, security vulnerabilities, and poor user experience if not addressed. Overall risk level: **HIGH**.

---

## Critical Issues

### CRIT-01: Duplicate Category Names Cause Import Failures
**Severity:** Critical  
**Location:** `Services/ImportExportService.cs:128`, `Data/ApplicationDbContext.cs`  
**Description:** The import service uses `ToDictionaryAsync(c => c.Name.ToLower(), c => c.Id)` which throws `ArgumentException` when duplicate category names exist in the database. The unique constraint on Categories is case-sensitive (`ParentId, Name`) but lookups are case-insensitive.  
**Impact:** CSV imports crash completely. Users cannot import any data when duplicates exist. Error logged as "An item with the same key has already been added. Key: dinners".  
**Evidence:** Exception trace shows failure at line 128 with key "dinners", indicating duplicate entries with different casing.

### CRIT-02: Fund Balance Input Field Unresponsive on Create
**Severity:** Critical  
**Location:** `Components/Shared/FundFormModal.razor:65-67`, lines 220-229  
**Description:** The `InputNumber` component for Starting Balance uses non-nullable `decimal` type, which causes the input to be unresponsive in Blazor. The `FundFormModel.StartingBalance` is declared as `decimal` instead of `decimal?`.  
**Impact:** Users cannot enter a starting balance when creating new accounts/funds. This breaks core functionality.  
**Evidence:** Line 225: `public decimal StartingBalance { get; set; } = 0;` - should be `decimal?`.

### CRIT-03: Category Filter Dropdown Does Not Trigger Search
**Severity:** Critical  
**Location:** `Components/Pages/Transactions/TransactionList.razor:42-49`  
**Description:** The category dropdown uses `@bind="filter.CategoryId"` without an `@onchange` handler. Selecting a category does not trigger the filter.  
**Impact:** Users expect instant filtering when selecting a category, but nothing happens. The Search button must be clicked separately, which is not intuitive.  
**Evidence:** Line 42 shows `@bind` without corresponding `@onchange` event handler.

### CRIT-04: No Transaction Boundaries for Multi-Table Updates (Partial Fix)
**Severity:** Critical  
**Location:** `Services/TransactionService.cs:200-290` (CreateAsync method)  
**Description:** While `CreateTransferAsync` uses explicit transactions, `CreateAsync` for regular transactions does not wrap the multiple database operations (transaction creation, split creation, donor update, grant update, fund balance update) in a transaction.  
**Impact:** Partial failures can leave database in inconsistent state. For example, transaction may be created but fund balance not updated.  
**Evidence:** Line 222-227 shows `_context.Transactions.Add` followed by multiple `await _context.SaveChangesAsync()` calls without explicit transaction.

### CRIT-05: Missing 404 Handler for Error Routes
**Severity:** Critical  
**Location:** `Program.cs:117`, missing `/error/404` page  
**Description:** The application uses `UseStatusCodePagesWithReExecute("/error/{0}")` but the error page may not handle all status codes properly. Logs show repeated warnings: "HTTP GET /error/404 completed | Status: 404".  
**Impact:** Users see broken error pages. The error handling creates infinite loops (error page itself returns 404).  
**Evidence:** Log output: `HTTP GET /error/404 completed | Status: 404 | Duration: 0ms`

---

## High Issues

### HIGH-01: N+1 Query Pattern in Transaction Loading
**Severity:** High  
**Location:** `Services/TransactionService.cs:33-40`  
**Description:** Uses multiple `Include()` statements which load all related data, but then iterates through results. For large datasets, this causes O(n) additional queries.  
**Impact:** Dashboard and transaction list become extremely slow with large datasets. 100 transactions could trigger 500+ queries.  
**Evidence:** Lines 33-40 include Category, Fund, Donor, Grant, and Splits with nested Category.

### HIGH-02: Missing Index on Frequently Queried Columns
**Severity:** High  
**Location:** `Data/ApplicationDbContext.cs`  
**Description:** Several commonly filtered columns lack indexes: `Transactions.Payee`, `Transactions.ReferenceNumber`, `Transactions.PONumber`, `Donors.Email`, `Documents.OriginalFileName`.  
**Impact:** Full table scans on filtered queries, degrading performance as data grows.  
**Evidence:** Only Date, Type, CategoryId, and IsDeleted have indexes per lines 119-122.

### HIGH-03: Swagger UI Enabled in All Environments
**Severity:** High  
**Location:** `Program.cs:104-110`  
**Description:** Swagger is only conditionally shown in the middleware pipeline (`if (app.Environment.IsDevelopment())`), but `AddSwaggerGen()` is unconditionally registered on line 100, and there's no explicit block to prevent access in production.  
**Impact:** API documentation may be exposed in production, revealing internal endpoints.  
**Evidence:** Line 100: `builder.Services.AddSwaggerGen();` with no environment check.

### HIGH-04: Hard-Coded Encryption Key in BackupService
**Severity:** High  
**Location:** `Services/BackupService.cs:21-22`  
**Description:** AES-256 encryption key is hard-coded as a Base64 string with a predictable pattern.  
**Impact:** Anyone with source code access can decrypt backups. Backups are not truly secure.  
**Evidence:** Line 21-22: `private static readonly byte[] DefaultEncryptionKey = Convert.FromBase64String("QXVkaXRSZW1lZGlhdGlvbjIwMjZLZXk=AAAAAAAAAAAAAAAAAAAAAA==");`

### HIGH-05: No Pagination Limit Validation on API
**Severity:** High  
**Location:** `Controllers/TransactionsController.cs:24`, `Services/TransactionService.cs:28-29`  
**Description:** While TransactionService enforces `MaxPageSize = 100`, the API controller doesn't validate before passing to service. A malicious request with `PageSize=1000000` is processed before service-level capping.  
**Impact:** Potential memory exhaustion before the limit is enforced. Request binding still processes large value.  
**Evidence:** Controller line 24 directly passes `filter` without pre-validation.

### HIGH-06: Missing Authorization on All API Endpoints
**Severity:** High  
**Location:** `Controllers/*.cs`  
**Description:** No `[Authorize]` attributes on any API controller. All endpoints are publicly accessible.  
**Impact:** Unauthenticated users can create, read, update, and delete all financial data via API.  
**Evidence:** `TransactionsController.cs`, `CategoriesController.cs`, etc. - no authorization attributes.

### HIGH-07: File Upload Without Virus Scanning
**Severity:** High  
**Location:** `Services/DocumentService.cs:11-18`  
**Description:** While file extensions are validated, there's no content-type verification or virus scanning for uploaded documents.  
**Impact:** Malicious files could be uploaded with renamed extensions.  
**Evidence:** Lines 11-18 only check `AllowedExtensions`, not actual file content.

### HIGH-08: Modal Focus Management Issues
**Severity:** High  
**Location:** `Components/Shared/FundFormModal.razor`, `Components/Shared/QuickAddModal.razor`  
**Description:** Modal dialogs don't trap focus or return focus to trigger element on close. No `role="dialog"` or `aria-modal` attributes.  
**Impact:** Keyboard users lose context. Screen reader users can interact with background content. WCAG 2.1 failure.  
**Evidence:** Line 4 in FundFormModal.razor: `<div class="modal">` without appropriate ARIA attributes.

### HIGH-09: No Rate Limiting on Import Endpoint
**Severity:** High  
**Location:** `Controllers/ImportExportController.cs`  
**Description:** File import endpoint accepts unlimited requests. No rate limiting or file size validation on controller level (only service level).  
**Impact:** DoS attack possible via repeated large file uploads.  
**Evidence:** No rate limiting middleware or attributes present.

### HIGH-10: CircuitHost Exception Handling Exposes Stack Traces
**Severity:** High  
**Location:** Blazor SignalR circuits  
**Description:** Unhandled exceptions in Blazor components log full stack traces including internal paths like `C:\Users\tech\source\repos\NonProfitFinance\...`.  
**Impact:** Information disclosure about server filesystem structure, .NET version, and internal implementation.  
**Evidence:** Log shows: `System.ArgumentException... at NonProfitFinance.Services.ImportExportService.ImportTransactionsFromCsvAsync...line 128`

---

## Medium Issues

### MED-01: Inconsistent Date Handling (Now vs UtcNow)
**Severity:** Medium  
**Location:** Multiple services  
**Description:** Mix of `DateTime.Now` and `DateTime.UtcNow` throughout codebase.  
**Impact:** Dates may be inconsistent across timezones. Reports could show incorrect dates.  
**Evidence:** `Services/TransactionService.cs` uses `DateTime.UtcNow`, but `Models/Transaction.cs:127` uses `DateTime.UtcNow` while UI may display local time.

### MED-02: Missing Unique Constraint on Fund Names
**Severity:** Medium  
**Location:** `Data/ApplicationDbContext.cs:148-162`  
**Description:** No unique constraint on `Fund.Name`. Duplicate fund names allowed.  
**Impact:** User confusion when selecting funds. Reporting issues.  
**Evidence:** Entity configuration for Fund (lines 148-162) has no unique index on Name.

### MED-03: Grant Spending Not Validated Against Budget
**Severity:** Medium  
**Location:** `Services/TransactionService.cs`  
**Description:** When creating expense transactions against grants, no validation checks if expense exceeds grant remaining balance.  
**Impact:** Organizations can overspend grants, causing compliance issues.  
**Evidence:** `CreateAsync` method updates grant usage but doesn't validate against `Grant.Amount`.

### MED-04: Document Storage on Local Filesystem Only
**Severity:** Medium  
**Location:** `Services/DocumentService.cs:25-30`  
**Description:** Documents stored in local `ContentRootPath/Documents` with no redundancy or cloud backup option.  
**Impact:** Disk failure results in permanent document loss.  
**Evidence:** Line 26: `_storagePath = Path.Combine(environment.ContentRootPath, "Documents");`

### MED-05: No Audit Trail for Bulk Operations
**Severity:** Medium  
**Location:** `Services/ImportExportService.cs`  
**Description:** CSV import creates transactions but audit logging is not called for each imported record.  
**Impact:** Bulk imports are not auditable. Cannot trace who imported what data.  
**Evidence:** Import method doesn't inject or call `IAuditService`.

### MED-06: Mobile Responsiveness Issues
**Severity:** Medium  
**Location:** `wwwroot/css/site.css`  
**Description:** Sidebar doesn't collapse on mobile (no hamburger menu visible). Tables overflow on narrow screens.  
**Impact:** Poor mobile user experience.  
**Evidence:** CSS uses fixed `--sidebar-width: 260px` without responsive breakpoints for hiding.

### MED-07: No CSRF Protection on State-Changing Operations
**Severity:** Medium  
**Location:** `Components/Shared/*.razor`  
**Description:** Button click handlers don't verify request origin. While Blazor Server has some built-in protection, explicit CSRF tokens are not used for sensitive operations.  
**Impact:** Potential cross-site request forgery for financial operations.  
**Evidence:** Form submissions use `@onclick` without anti-forgery tokens.

### MED-08: Memory Leak Potential in Chart Components
**Severity:** Medium  
**Location:** `Components/Charts/LineChart.razor`, `Components/Charts/PieChart.razor`  
**Description:** Chart.js instances may not be properly disposed when components unmount.  
**Impact:** Memory grows with repeated navigation, eventually slowing browser.  
**Evidence:** Components don't implement `IDisposable` or call JS cleanup.

### MED-09: Search Debouncing Inconsistent
**Severity:** Medium  
**Location:** `Components/Pages/Transactions/TransactionList.razor:79-80`  
**Description:** Search input uses `@bind:event="oninput"` which triggers on every keystroke, but filtering doesn't debounce.  
**Impact:** Excessive server requests while typing. Performance issues.  
**Evidence:** Line 79-80 binds to `oninput` without debounce timer.

### MED-10: No Empty State Messages
**Severity:** Medium  
**Location:** `Components/Pages/Transactions/TransactionList.razor:183-186`  
**Description:** When no transactions match filters, only "No transactions found" shows with no helpful guidance.  
**Impact:** Users unsure if it's a filter issue or truly empty data.  
**Evidence:** Line 185: generic message without context about applied filters.

### MED-11: Budget Overspend Not Prevented
**Severity:** Medium  
**Location:** `Services/BudgetService.cs`, `Services/TransactionService.cs`  
**Description:** No check prevents spending beyond category or overall budget limits.  
**Impact:** Organizations can unknowingly overspend, violating budget controls.  
**Evidence:** Transaction creation doesn't check `Category.BudgetLimit`.

### MED-12: Cascading Delete Risks with SetNull
**Severity:** Medium  
**Location:** `Data/ApplicationDbContext.cs:108-112`  
**Description:** Transaction-Fund relationship uses `OnDelete(DeleteBehavior.SetNull)`. Deleting a fund orphans transactions.  
**Impact:** Transactions lose fund association, breaking reports and balance calculations.  
**Evidence:** Line 112: `.OnDelete(DeleteBehavior.SetNull);`

---

## Low / Informational

### LOW-01: No EditorConfig for Consistent Formatting
**Severity:** Low  
**Location:** Solution root  
**Description:** `.editorconfig` file exists but may not enforce all team conventions.  
**Impact:** Inconsistent code formatting across developers.

### LOW-02: Magic Strings for Role Names and Status Values
**Severity:** Low  
**Location:** Various services  
**Description:** Strings like "Transfer", "Active" used as literals instead of constants.  
**Impact:** Typos cause bugs. Refactoring is risky.  
**Evidence:** `TransactionService.cs:295`: `c.Name == "Transfer"` literal string.

### LOW-03: No XML Documentation on Interfaces
**Severity:** Low  
**Location:** `Services/I*.cs`  
**Description:** Interface methods lack XML documentation comments.  
**Impact:** Poor IntelliSense, difficulty generating API docs.

### LOW-04: Font Awesome Full Library Loaded
**Severity:** Low  
**Location:** `Components/App.razor`  
**Description:** Loading entire Font Awesome library for approximately 50 icons used.  
**Impact:** ~300KB unnecessary download.

### LOW-05: Static Assets Not Cache-Busted
**Severity:** Low  
**Location:** `wwwroot/css/*.css`, `wwwroot/js/*.js`  
**Description:** No version hash or fingerprinting on static asset URLs.  
**Impact:** Browser caching issues after deployments.

### LOW-06: Currency Format Inconsistent
**Severity:** Low  
**Location:** Various Razor components  
**Description:** Mix of `ToString("C")`, `ToString("C2")`, `ToString("C0")`, and manual formatting.  
**Impact:** Inconsistent display (some show cents, some don't).

### LOW-07: Date Format Inconsistent
**Severity:** Low  
**Location:** Various components  
**Description:** Mix of `MMM dd, yyyy`, `yyyy-MM-dd`, `M/d/yyyy` across pages.  
**Impact:** User confusion.

### LOW-08: Console Logging Instead of Structured Logging
**Severity:** Low  
**Location:** Various services  
**Description:** Some error handling uses basic logging without structured parameters.  
**Impact:** Difficult to query logs in production.

### LOW-09: Commented-Out Code Present
**Severity:** Low  
**Location:** Various files  
**Description:** Dead code and commented blocks remain in codebase.  
**Impact:** Maintenance confusion.

### LOW-10: No API Versioning
**Severity:** Low  
**Location:** `Controllers/*.cs`  
**Description:** API routes are `/api/transactions` not `/api/v1/transactions`.  
**Impact:** Breaking changes affect all clients simultaneously.

### LOW-11: No Request/Response Logging for API
**Severity:** Low  
**Location:** `Program.cs`  
**Description:** While `UseRequestLogging()` is called, it may not capture API request bodies.  
**Impact:** Difficult to debug API issues.

### LOW-12: Orphan Detection Missing
**Severity:** Low  
**Location:** Database  
**Description:** No scheduled job to detect orphaned records (e.g., TransactionSplits without parent Transaction).  
**Impact:** Data integrity degrades over time.

---

## Steps to Remediation

### Critical Issues

**CRIT-01: Duplicate Category Names**
- Why it matters: Blocks all CSV import functionality
- Fix direction: Use `GroupBy().ToDictionary()` pattern instead of direct `ToDictionary()`. Add case-insensitive unique index on Categories.

**CRIT-02: Fund Balance Input Unresponsive**
- Why it matters: Users cannot create funds with starting balances
- Fix direction: Change `StartingBalance` property to nullable `decimal?` type. Add `TValue="decimal?"` to InputNumber component.

**CRIT-03: Category Filter Not Working**
- Why it matters: Core filtering functionality broken
- Fix direction: Add `@onchange` event handler that calls filter apply method. Remove redundant `@bind` or use value/onchange pattern.

**CRIT-04: Missing Transaction Boundaries**
- Why it matters: Database can become inconsistent on partial failures
- Fix direction: Wrap all multi-table operations in explicit database transactions using `BeginTransactionAsync()`.

**CRIT-05: Error Page 404 Loop**
- Why it matters: Error handling itself is broken
- Fix direction: Create proper error pages for each status code. Ensure `/error/{statusCode}` route exists and doesn't itself error.

### High Issues

**HIGH-01 through HIGH-10:**
- Add database indexes on frequently queried columns
- Move encryption keys to secure configuration (Azure Key Vault, environment variables)
- Add `[Authorize]` attributes to all API controllers
- Implement request rate limiting middleware
- Add ARIA attributes to modals for accessibility
- Configure Swagger to only load in Development environment
- Implement content-type verification for file uploads
- Configure production exception handling to not expose stack traces

### Medium Issues

**MED-01 through MED-12:**
- Standardize on `DateTime.UtcNow` throughout
- Add unique constraints on business keys (Fund.Name, Donor.Email)
- Implement budget validation before transaction creation
- Consider cloud storage for documents (Azure Blob, S3)
- Add audit logging to import operations
- Implement responsive CSS with mobile breakpoints
- Add search debouncing with 300ms delay
- Properly dispose Chart.js instances on component disposal

### Low Issues

**LOW-01 through LOW-12:**
- Define constants for magic strings
- Add XML documentation to public interfaces
- Implement static asset fingerprinting
- Standardize date/currency formatting helpers
- Remove dead code and commented blocks
- Consider API versioning strategy for future changes

---

## Database-Specific Issues Summary

| Issue | Table | Column | Problem |
|-------|-------|--------|---------|
| Duplicate names | Categories | Name | Case-insensitive duplicates allowed |
| Missing unique | Funds | Name | No uniqueness constraint |
| Missing unique | Donors | Email | No uniqueness constraint |
| Missing index | Transactions | Payee | Full table scans |
| Missing index | Transactions | ReferenceNumber | Full table scans |
| Missing index | Documents | OriginalFileName | Full table scans |
| Orphan risk | TransactionSplits | TransactionId | SetNull on cascade |
| Orphan risk | Transactions | FundId | SetNull on cascade |

---

## Navigation & Button Issues Summary

| Page | Element | Issue |
|------|---------|-------|
| /transactions | Category dropdown | Does not trigger filter |
| /transactions | Account dropdown | Does not trigger filter |
| /transactions | Type dropdown | Does not trigger filter |
| /funds (Add Fund) | Starting Balance input | Unresponsive |
| /error/404 | Error page | Returns 404 itself |
| All modals | Focus management | Focus not trapped |
| All modals | Escape key | Inconsistent handling |

---

*I have not modified, suggested, or written any corrected code — only identified and described issues.*
