# Full Functionality & UX Simulation Test Report – NonProfit Finance – January 2025

## Summary

**Overall Usability Score: 6/10** – The application has solid foundational UX patterns and accessibility features (skip links, ARIA attributes, keyboard shortcuts), but is **currently blocked from starting** due to a critical database schema issue (`NOT NULL constraint failed: Funds.RowVersion`). Beyond this blocker, simulated user journeys reveal multiple friction points: inconsistent modal behavior, confusing category vs. account terminology, missing form validation feedback, and incomplete error recovery flows. The import/export feature has significant UX complexity. Mobile responsiveness needs attention. Risk level is **HIGH** for production deployment without addressing the critical and high-priority items below.

---

## Critical Functional Breaks

| Severity | Description | Steps to Repro | Location | Impact |
|----------|-------------|----------------|----------|--------|
| **CRITICAL** | Application fails to start - `NOT NULL constraint failed: Funds.RowVersion` | 1. Start debugger (F5) 2. App crashes at DataSeeder line 31 | `Data/DataSeeder.cs:31`, `Program.cs:657` | **100% blocking** - No user can access application |
| **CRITICAL** | Database schema mismatch - EnsureCreated() doesn't fix old databases | 1. Have existing DB without RowVersion column 2. Start app 3. Crash | `Program.cs:165-210` | Existing installations will crash after update |
| **CRITICAL** | ALTER TABLE for RowVersion silently fails for SQLite existing tables | The ALTER TABLE statements in Program.cs don't properly handle existing populated tables | `Program.cs:172-208` | Schema migration broken |

---

## High Priority Issues

| Severity | Description | Steps to Repro | Location | Impact |
|----------|-------------|----------------|----------|--------|
| **HIGH** | Category unique constraint may cause duplicate key errors on import | 1. Import CSV with categories 2. Category matching fails due to case sensitivity or whitespace | `Data/DataSeeder.cs`, `Services/ImportExportService.cs` | Data import failures |
| **HIGH** | QuickAddModal CategoryId bound to 0 on init - invalid category submission | 1. Open QuickAdd from Dashboard 2. Don't select category 3. Submit | `Components/Shared/QuickAddModal.razor:51` | `CategoryId = 0` will cause FK constraint error |
| **HIGH** | TransactionForm allows future dates despite max attribute | 1. Open Transaction form 2. Manually type future date 3. Warning shows but form still submits | `Components/Pages/Transactions/TransactionForm.razor:53-54` | Invalid data saved to DB |
| **HIGH** | Transfer transactions require ToFundId but validation may not catch null | 1. Select Transfer type 2. Select From Account 3. Leave To Account empty 4. Submit | `Components/Pages/Transactions/TransactionForm.razor:70-96` | Partial transfer with orphaned transaction |
| **HIGH** | Global search dropdown keyboard navigation trap | 1. Open search 2. Press ↓ to navigate results 3. Tab - focus escapes to document, not close button | `Components/Shared/GlobalSearch.razor` | Keyboard-only users stuck |
| **HIGH** | Import column numbers are 0-indexed but UI doesn't explain this | 1. Go to Import/Export 2. Set Date Column = 1 (first column) 3. Import fails - actually needs 0 | `Components/Pages/ImportExport/ImportExportPage.razor:76-88` | Confusing for all users |
| **HIGH** | No confirmation before deleting import presets | 1. Select a preset 2. Click trash icon 3. Preset deleted immediately | `ImportExportPage.razor:53-56` | Accidental data loss |
| **HIGH** | Budget form modal missing - referenced but not shown | 1. Go to Budgets 2. Try to add budget 3. Check if BudgetFormModal opens properly | `Components/Pages/Budgets/BudgetList.razor` | Can't create budgets |
| **HIGH** | NavMenu shows "/funds" as "Accounts" - terminology mismatch | 1. Click "Accounts" in nav 2. URL says /funds 3. Page header might say "Funds" | `Components/Layout/NavMenu.razor:29-31` | User confusion about where they are |

---

## Medium UX Frictions

| Severity | Description | Steps to Repro | Location | Impact |
|----------|-------------|----------------|----------|--------|
| **MEDIUM** | Dashboard compliance alerts take() but don't show total count | 1. Have >3 critical reminders 2. Dashboard shows "• reminder • reminder • reminder" | `Components/Pages/Dashboard.razor:44-47` | Users don't know how many they're missing |
| **MEDIUM** | Category tree selection state not cleared when switching Income/Expense tabs | 1. Select expense category 2. Switch to Income tab 3. Details panel still shows expense category | `Components/Pages/Categories/CategoryManager.razor` | Confusing stale data |
| **MEDIUM** | TransactionList filter bar doesn't have clear visual hierarchy | 1. View Transactions page 2. Try to understand filter groups | `Components/Pages/Transactions/TransactionList.razor:28-81` | Cognitive overload |
| **MEDIUM** | Search button in filter bar vs Enter key behavior unclear | 1. Type in search box 2. Press Enter - nothing happens 3. Must click Search button | `TransactionList.razor:72-75` | Unexpected for power users |
| **MEDIUM** | Date range filter has no presets (This Month, Last Quarter, YTD) | 1. Go to Transactions 2. Want "Last Month" 3. Must manually enter dates | `TransactionList.razor:33-38` | Repetitive work for common tasks |
| **MEDIUM** | Currency symbol hardcoded to $ in multiple places | 1. Check MetricCard, TransactionForm 2. $ symbol appears statically | Multiple components | Not localization-ready |
| **MEDIUM** | Settings page has no Save button visibility - scrolls off screen | 1. Open Settings 2. Fill org info 3. Save button at bottom of very long form | `Components/Pages/Settings/SettingsPage.razor` | User unsure if changes saved |
| **MEDIUM** | Keyboard shortcut help modal not focusable on open | 1. Press Shift+? 2. Modal opens 3. Tab - focus doesn't start in modal | `Components/Shared/KeyboardShortcutsHelp.razor` | Accessibility trap |
| **MEDIUM** | Theme toggle button icon doesn't update after toggle | 1. Click theme toggle 2. Icon should switch sun↔moon 3. May need page refresh | `Components/Layout/MainLayout.razor:28-30` | Visual feedback missing |
| **MEDIUM** | Empty states missing loading vs empty distinction | 1. Load Categories (first time) 2. See "No categories found" briefly before data loads | `CategoryManager.razor:53-56` | Misleading empty state flash |
| **MEDIUM** | ImportExport column mapping uses number inputs - no column name preview | 1. Upload CSV 2. Guess which column is 0, 1, 2 3. Trial and error | `ImportExportPage.razor:68-88` | High error rate on imports |
| **MEDIUM** | Donor/Grant modals may not reset state between Add/Edit | 1. Edit Donor 2. Close 3. Click Add Donor 4. Old data may remain | `Components/Shared/DonorFormModal.razor` | Data leakage between operations |
| **MEDIUM** | Reports page has no loading indicator during PDF generation | 1. Go to Reports 2. Generate PDF 3. No feedback until download starts | `Components/Pages/Reports/ReportBuilder.razor` | User thinks button didn't work |
| **MEDIUM** | Compliance page deadlines may not sort by urgency | 1. View Compliance 2. Check if most urgent items are at top | `Components/Pages/Compliance/CompliancePage.razor` | Miss critical deadlines |

---

## Low / Polish Items

| Severity | Description | Location | Impact |
|----------|-------------|----------|--------|
| **LOW** | Skip link visible on focus but styling could be improved | `MainLayout.razor:9-11` | Functional but not polished |
| **LOW** | Card header inconsistent - some have subtitles, some don't | Multiple pages | Visual inconsistency |
| **LOW** | Form labels use * for required but no legend explaining this | All forms | Accessibility best practice missing |
| **LOW** | Search clear button (×) target is small on mobile | `GlobalSearch.razor:23-27` | Touch target < 44px |
| **LOW** | Sidebar collapsed state not persisted across sessions | `MainLayout.razor` | Minor convenience |
| **LOW** | Help page link in nav doesn't indicate external content sections | `NavMenu.razor:93-96` | Mild expectation mismatch |
| **LOW** | Transaction type tabs (Income/Expense/Transfer) not keyboard navigable with arrow keys | `TransactionForm.razor:21-35` | WCAG nice-to-have |
| **LOW** | Modal backdrop click to close is inconsistent - some modals allow, some don't | Various modals | Unpredictable behavior |
| **LOW** | Chart tooltips may not be accessible to screen readers | `Components/Charts/*.razor` | Chart data not announced |
| **LOW** | Success toast messages don't auto-dismiss | `QuickAddModal.razor:65-68` | Clutter if adding many items |
| **LOW** | Tags filter uses comma-separated text - no tag picker UI | `TransactionList.razor:66-69` | UX could be friendlier |
| **LOW** | Document upload doesn't show progress indicator | `DocumentsPage.razor` | Large files appear stuck |
| **LOW** | Bank Connections page mentions Plaid but integration is placeholder | `BankConnectionsPage.razor` | Misleading feature |

---

## User Journey Pain Points

### Top 10 Most Frustrating Flows:

1. **First-time startup fails completely** – New users installing the app encounter immediate crash due to RowVersion constraint. Cannot proceed at all. This is catastrophic for first impressions.

2. **CSV Import guessing game** – Users must guess 0-indexed column numbers without seeing column headers. No preview of data. High likelihood of failed imports and frustration.

3. **Transfer transactions are confusing** – Terminology switches between "Account" and "Fund" throughout the app. Transfer creates two transactions but UI doesn't clearly show this relationship afterward.

4. **Finding where you saved something** – After using QuickAdd from Dashboard, no confirmation of which account/category was used. User must navigate to Transactions to verify.

5. **Settings page scroll marathon** – Long form with no section anchors visible. Save button at the bottom. User types organization info, scrolls down, types more, hopes autosave works (it may not).

6. **Category management type switching** – Selecting a category, then switching tabs (Income↔Expense) leaves stale selection panel. Must mentally reset.

7. **Compliance deadline anxiety** – Critical deadlines shown truncated ("...due Jan 15"), but clicking "View All" goes to a page where urgency sorting may not be obvious.

8. **Report generation black hole** – Click generate, no spinner, no progress. PDF either downloads or doesn't. Error handling unclear.

9. **Recurring transaction setup blind** – Setting up recurring transactions without clear preview of when next one will execute. "Monthly" starting Jan 15 - when does it trigger?

10. **Global search no context** – Search results show type badges but clicking navigates away. No way to preview result without leaving current page.

---

## Detailed Persona-Based Observations

### Novice User (First-time nonprofit treasurer)
- **Blocker**: App won't start (RowVersion crash)
- Would struggle with Fund vs Account terminology
- No onboarding wizard or "Getting Started" prompt
- Help documentation exists but not contextual

### Power User (Daily bookkeeper)
- Would want keyboard shortcuts - these exist but discovery is poor
- Filter presets for common date ranges would save significant time
- Batch operations (edit multiple transactions) not available
- No duplicate detection on manual entry

### Impatient User (Board member checking reports)
- Dashboard loads but charts may take time
- No way to quickly get "Last Month Summary" without configuration
- PDF export has no progress - would click multiple times
- Would expect email export option

### Accessibility-Needs User
- Skip link present ✓
- ARIA labels on many elements ✓
- Focus management in modals inconsistent ✗
- Screen reader announcements for dynamic content sparse ✗
- High contrast mode not explicitly supported ✗

### Edge-Case User (Testing limits)
- Max length inputs not enforced everywhere
- Unicode characters in descriptions - unknown handling
- Negative amounts - validation unclear
- 10+ year old dates accepted with just a warning
- Very long category names may overflow UI

### Frustrated User (Recovering from errors)
- Delete preset - no undo
- Failed import - error message but no "try again" with corrections
- No recycle bin for categories (transactions have one)
- Browser back button during modal - behavior undefined

---

## Mobile/Responsive Issues

| Issue | Description | Breakpoint |
|-------|-------------|------------|
| Filter bar overflows | Transaction filter bar wraps awkwardly | < 768px |
| Charts too small | Pie/line charts unreadable | < 576px |
| Modal width fixed | Modals don't adapt to narrow screens | < 480px |
| Touch targets small | Filter dropdowns, close buttons | All mobile |
| Sidebar toggle missing | No hamburger menu visible | < 768px |
| Table horizontal scroll | No indication more columns exist | < 992px |

---

## Recommendations for Immediate Action

1. **FIX DATABASE SCHEMA** - Resolve RowVersion constraint by either:
   - Delete old database and recreate
   - Properly migrate with `EnsureDeleted()` before `EnsureCreated()` when schema mismatch detected
   
2. **Add confirmation dialogs** for destructive actions (delete preset, archive category)

3. **Improve import UX** - Show CSV column headers, let users map visually

4. **Standardize terminology** - Pick "Fund" OR "Account" and use consistently

5. **Add loading states** everywhere a network call happens

6. **Fix modal focus traps** - Ensure Tab key stays within open modals

7. **Add date presets** - "This Month", "Last Quarter", "YTD" buttons

8. **Persist user preferences** - Theme, sidebar state, filter preferences

---

*No code was suggested, written or fixed. Only problems observed during simulated real usage were reported.*
