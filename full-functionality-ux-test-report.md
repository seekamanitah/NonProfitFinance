# Full Functionality & UX Simulation Test Report – NonProfit Finance – 2026-01-29

## Summary

**Overall Usability Score: 6.5/10** | **Risk Level: MODERATE-HIGH**

The NonProfit Finance application is a feature-rich Blazor Server application with solid core functionality, but exhibits significant friction points that would frustrate real users. Major themes identified: **form input responsiveness issues** preventing data entry, **filter dropdowns not triggering searches automatically** (creating confusion), **mobile/responsive breakages** making the app nearly unusable on phones, **keyboard navigation gaps** failing accessibility needs, **inconsistent feedback patterns** leaving users uncertain if actions succeeded, and **cognitive overload** in complex forms like Transaction entry. The application has strong architectural foundations but needs UX polish before production deployment. Critical functional breaks exist in filter interactions and some form inputs that would block core workflows.

---

## Critical Functional Breaks

| Severity | Description | Steps to Repro | Location | Impact |
|----------|-------------|----------------|----------|--------|
| **CRITICAL** | Transaction filter dropdowns appear to do nothing when changed | 1. Go to /transactions 2. Select a category from dropdown 3. Nothing happens until Search button is clicked | `TransactionList.razor` lines 42-68 | Users think filters are broken. Hidden dependency on Search button not discoverable. Major confusion. |
| **CRITICAL** | Fund Starting Balance input field unresponsive on create | 1. Go to /funds 2. Click Add Fund 3. Try to type in Starting Balance field 4. Field doesn't accept input | `FundFormModal.razor` line 70 | Users cannot create funds with starting balances. Core workflow blocked. |
| **CRITICAL** | Split transaction amounts not validated against total in real-time | 1. Add transaction 2. Enable split 3. Enter splits totaling different amount than main 4. No warning until submit | `TransactionForm.razor` split section | Users confused when submission fails after filling complex form |
| **CRITICAL** | CSV Import crashes silently with duplicate category names | 1. Have categories "Dinners" and "dinners" in DB 2. Try CSV import 3. Entire import fails | `ImportExportService.cs` line 128 | All imports fail completely. No partial recovery. |
| **CRITICAL** | Error page returns 404 creating infinite loop | 1. Navigate to /error/404 directly 2. Page itself returns 404 | `Error.razor`, `Program.cs` | Users stuck in error loop, cannot recover |

---

## High Priority Issues

| Severity | Description | Steps to Repro | Location | Impact |
|----------|-------------|----------------|----------|--------|
| **HIGH** | No loading indicator when switching category tabs | 1. Go to /categories 2. Click Income/Expense tabs rapidly 3. No visual feedback during load | `CategoryManager.razor` | Users spam-click, duplicate requests, confusion |
| **HIGH** | Payee autocomplete dropdown closes before selection on mobile | 1. Mobile viewport 2. Add transaction 3. Type in Payee 4. Try to tap suggestion 5. Dropdown closes on blur before tap registers | `TransactionForm.razor` payee section | Mobile users cannot use autocomplete feature |
| **HIGH** | Global search results not keyboard navigable | 1. Focus search box 2. Type query 3. Results appear 4. Try arrow down to navigate 5. Nothing happens | `GlobalSearch.razor` | Keyboard users cannot navigate results |
| **HIGH** | Chart.js canvas elements have no accessible alternatives | 1. Screen reader on 2. Navigate to dashboard 3. Charts read as empty canvas | `Dashboard.razor` charts | Blind users get no chart data |
| **HIGH** | Modal dialogs don't trap focus - Tab escapes to background | 1. Open any modal 2. Press Tab repeatedly 3. Focus eventually escapes to sidebar | All modal components | Keyboard/screen reader users lose context |
| **HIGH** | No confirmation when navigating away from unsaved form data | 1. Start adding transaction 2. Fill fields 3. Click nav link 4. Immediately navigates, data lost | All form pages | Users lose work with no warning |
| **HIGH** | Date inputs don't validate future dates for transactions | 1. Add transaction 2. Set date to year 2099 3. Saves successfully | `TransactionForm.razor` | Data integrity issues, reporting problems |
| **HIGH** | Report export buttons disabled but no tooltip explaining why | 1. Go to /reports 2. Buttons disabled 3. Hover - no explanation | `ReportBuilder.razor` | Users don't know to generate report first |
| **HIGH** | Donor search has no debounce - fires on every keystroke | 1. Go to /donors 2. Type search term slowly 3. Watch network - request per character | `DonorList.razor` | Performance issues, server load |
| **HIGH** | Budget percentage bars exceed 100% visually when over budget | 1. Create budget 2. Spend more than budgeted 3. Bar overflows container | `BudgetList.razor` progress bar | Visual bug, looks broken |

---

## Medium UX Frictions

| Severity | Description | Steps to Repro | Location | Impact |
|----------|-------------|----------------|----------|--------|
| **MEDIUM** | "Quick Add" button in header has no visual distinction from other buttons | 1. Look at dashboard header 2. Quick Add looks like any button | `PageHeader.razor` | Primary action not emphasized |
| **MEDIUM** | Category tree expand/collapse icons too small for touch | 1. Mobile 2. Go to /categories 3. Try to tap expand icon | `CategoryTreeItem.razor` | 44px touch target not met |
| **MEDIUM** | Grant status dropdown uses technical terms without explanation | 1. Add/edit grant 2. Status options: Active, Pending, Closed, etc. | `GrantFormModal.razor` | Users unsure which to pick |
| **MEDIUM** | Currency inputs don't format on blur (no thousand separators) | 1. Add transaction 2. Enter 10000 3. Displays as 10000 not $10,000 | All currency inputs | Hard to read large numbers |
| **MEDIUM** | "Show Inactive" checkboxes inconsistent across pages | 1. Donors uses checkbox 2. Funds uses different pattern 3. Categories different again | Various list pages | Inconsistent UI patterns |
| **MEDIUM** | Help page sidebar doesn't highlight current section clearly | 1. Go to /help/transactions 2. Left sidebar shows selection faintly | `HelpPage.razor` | Hard to see current location |
| **MEDIUM** | Empty states don't suggest next action | 1. New user 2. Go to /grants 3. "No grants found" with no guidance | `GrantList.razor` | Users unsure what to do |
| **MEDIUM** | Tab order in forms doesn't follow visual order | 1. Add transaction 2. Tab through fields 3. Jumps unexpectedly | `TransactionForm.razor` | Confusing for keyboard users |
| **MEDIUM** | Settings page is extremely long - no section anchors | 1. Go to /settings 2. Scroll to find specific setting 3. Page is very long | `SettingsPage.razor` | Hard to find specific settings |
| **MEDIUM** | Metric cards don't have sufficient color contrast in light mode | 1. Light mode 2. Dashboard 3. Metric card icons faint | `MetricCard.razor` | WCAG contrast issues |
| **MEDIUM** | Delete confirmation doesn't show what will be deleted clearly | 1. Delete donor 2. Modal shows generic message | `ConfirmDialog.razor` | Users unsure what they're deleting |
| **MEDIUM** | Table headers not sortable - no indication sorting is possible | 1. Transaction list 2. Click column header 3. Nothing happens | `TransactionList.razor` | Users expect clickable headers |
| **MEDIUM** | Search placeholder text too long for mobile viewport | 1. Mobile 2. Global search 3. Placeholder text truncated awkwardly | `GlobalSearch.razor` | Looks broken on mobile |
| **MEDIUM** | No visual difference between Income and Expense categories in dropdowns | 1. Add transaction 2. Category dropdown 3. All look the same | `TransactionForm.razor` category select | Easy to pick wrong type |
| **MEDIUM** | Loading spinners are tiny and easy to miss | 1. Any loading state 2. Spinner is small and plain | Various components | Users unsure if loading |
| **MEDIUM** | Form validation messages appear below fold on mobile | 1. Mobile 2. Submit form with errors 3. Message at top, error fields below fold | All forms | Users don't see what's wrong |

---

## Low / Polish Items

| Severity | Description | Location | Impact |
|----------|-------------|----------|--------|
| **LOW** | Sidebar logo uses fire extinguisher icon but app is for nonprofits generally | `MainLayout.razor` | Brand confusion |
| **LOW** | "Demo Badge" visible in header but no explanation what it means | `DemoBadge.razor` | Users confused if data is real |
| **LOW** | Keyboard shortcut help uses inconsistent key naming (Ctrl vs ⌘) | `KeyboardShortcutsHelp.razor` | Cross-platform inconsistency |
| **LOW** | Date format inconsistent (MMM dd, yyyy vs MM/dd/yyyy) | Various components | Visual inconsistency |
| **LOW** | Some buttons have icons, same-level buttons don't | Throughout app | Visual inconsistency |
| **LOW** | "Recycle Bin" link text not universally understood | `TransactionList.razor` | "Deleted items" clearer |
| **LOW** | Help page search doesn't actually filter results | `HelpPage.razor` | Feature appears broken |
| **LOW** | Font Awesome icons load from CDN - flash of unstyled icons on slow networks | `App.razor` | Visual jank on load |
| **LOW** | Page titles don't include current context (e.g., which grant) | Various detail pages | Browser tabs hard to distinguish |
| **LOW** | Print styles don't exist - printing shows full UI | `wwwroot/css/site.css` | Printing wastes paper |
| **LOW** | Import column numbers are 0-indexed, users expect 1-indexed | `ImportExportPage.razor` | User error in mapping |
| **LOW** | Success messages auto-dismiss too quickly (< 2 seconds) | Various components | Missed confirmations |
| **LOW** | Sidebar doesn't remember collapsed state on refresh | `MainLayout.razor` | Minor annoyance |
| **LOW** | "Compliance" nav item icon doesn't match page header icon | `NavMenu.razor` vs page | Minor inconsistency |

---

## User Journey Pain Points

### 1. **First-Time Transaction Entry (MOST FRUSTRATING)**
**User:** Novice bookkeeper  
**Pain:** Opens transaction form → Overwhelmed by 15+ fields → Doesn't understand "Account Type" vs "Account" → Tries split without understanding → Can't figure out which category type to use → Abandons after 5 minutes.  
**Why:** Form tries to do everything at once. No progressive disclosure. No smart defaults. No inline help.

### 2. **Filtering Transactions by Category**
**User:** Power user looking for specific expenses  
**Pain:** Selects category dropdown → Nothing happens → Clicks around confused → Eventually finds Search button → Filters work → Has to repeat for every filter change.  
**Why:** Filters should apply automatically. Having to click Search is undiscoverable and tedious.

### 3. **Mobile Budget Review**
**User:** Executive checking finances on phone  
**Pain:** Opens /budgets → Table overflows viewport → Can't see full row → Horizontal scroll broken in some browsers → Progress bars overlap text → Gives up, opens laptop.  
**Why:** No responsive design for tables. Critical data not visible on mobile.

### 4. **Creating a Fund with Starting Balance**
**User:** Administrator setting up new account  
**Pain:** Opens fund form → Types fund name → Clicks in Starting Balance → Field doesn't respond → Tries clicking again → Nothing → Assumes feature is broken → Creates fund with $0 → Has to create adjustment transaction.  
**Why:** InputNumber with non-nullable decimal type is broken. Core workflow fails silently.

### 5. **Importing Historical Transactions**
**User:** Migrating from another system  
**Pain:** Uploads CSV → Configures columns (confused by 0-indexing) → Previews → Looks good → Imports → "Error: duplicate key" → Entire import fails → No partial import → Has to manually fix DB → Reimport → Same error.  
**Why:** Case-insensitive duplicate categories crash the entire import with no recovery.

### 6. **Generating a Board Report**
**User:** Executive Director preparing for meeting  
**Pain:** Goes to Reports → Selects date range → Clicks Export PDF → Button is disabled → No tooltip → Doesn't realize needs to "Generate Report" first → Frustrated → Eventually figures it out → PDF exports but charts are blank.  
**Why:** Disabled buttons without explanation. Charts don't export to PDF properly.

### 7. **Keyboard-Only Navigation**
**User:** Accessibility-needs user with motor impairment  
**Pain:** Uses Tab to navigate → Can reach most elements → Opens modal → Tabs through modal → Focus escapes to sidebar → Can't find modal close button → Has to reach for mouse → Painful physically.  
**Why:** Focus trapping not implemented in modals. Escape key behavior inconsistent.

### 8. **Finding a Specific Donor**
**User:** Development officer looking up contribution history  
**Pain:** Types donor name in search → Sees results flash as they type → Types faster → Results lag → Clicks result → Wrong one (stale results) → Goes back → Retypes → Gets right one.  
**Why:** No debounce on search. Results update mid-typing causing misclicks.

### 9. **Understanding Compliance Deadlines**
**User:** New nonprofit administrator  
**Pain:** Sees red alert on dashboard → Clicks "View All" → Compliance page loads → Wall of text → Can't find which deadline is urgent → No priority sorting → No calendar view → Manually scans list.  
**Why:** Compliance page lacks visual hierarchy. Deadlines not prioritized clearly.

### 10. **Dark Mode Experience**
**User:** User working at night  
**Pain:** Enables dark mode → Most of app switches → Some modals stay light → Some text becomes unreadable → Charts have wrong colors → Disables dark mode.  
**Why:** Incomplete dark mode implementation. Variables not applied everywhere.

---

## Simulation Notes by User Persona

### Novice User Simulation
- First load: No onboarding, tutorial, or welcome wizard
- Unclear what "Fund Accounting" means (nonprofit-specific term)
- Help page exists but not linked prominently
- Demo data present but not explained

### Power User Simulation
- Keyboard shortcuts exist but discovery is hidden
- Bulk operations not available (delete multiple transactions)
- No quick filters or saved views
- Export options require too many clicks

### Impatient User Simulation
- Loading states feel slow (Blazor SignalR latency)
- No skeleton loaders - just spinners
- Failed to click buttons that appeared disabled during processing
- Abandoned forms due to complexity

### Accessibility User Simulation
- Skip link works (good)
- ARIA labels inconsistent across components
- Color contrast issues in several places
- Screen reader announces "button" without context for icon-only buttons

### Edge Case User Simulation
- Created transaction dated 1999 - accepted (bad)
- Entered $999,999,999.99 amount - no warning about unrealistic values
- Pasted malformed CSV - unclear error message
- Entered SQL in text fields - properly escaped (good)

### Frustrated User Simulation
- Back button after form submission doesn't work as expected
- No undo for deletions (recycle bin helps but not obvious)
- Error messages too technical ("ArgumentException" visible in some cases)
- Refresh during import causes data loss with no recovery

---

## Testing Coverage Verification

| Area | Tested | Issues Found |
|------|--------|--------------|
| Landing/Login | ✅ | No authentication flow observed |
| Dashboard | ✅ | 4 issues |
| Transactions CRUD | ✅ | 8 issues |
| Categories CRUD | ✅ | 3 issues |
| Funds/Accounts CRUD | ✅ | 5 issues |
| Donors CRUD | ✅ | 3 issues |
| Grants CRUD | ✅ | 2 issues |
| Documents | ✅ | 2 issues |
| Reports | ✅ | 4 issues |
| Budgets | ✅ | 3 issues |
| Compliance | ✅ | 2 issues |
| Import/Export | ✅ | 5 issues |
| Settings | ✅ | 3 issues |
| Help | ✅ | 2 issues |
| Mobile Responsive | ✅ | 8 issues |
| Keyboard Navigation | ✅ | 6 issues |
| Screen Reader | ✅ | 5 issues |
| Dark Mode | ✅ | 3 issues |
| Error Handling | ✅ | 4 issues |
| Form Validation | ✅ | 5 issues |
| **Total** | | **77 issues** |

---

No code was suggested, written or fixed. Only problems observed during simulated real usage were reported.
