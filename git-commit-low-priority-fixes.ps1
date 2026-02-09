# Commit LOW Priority Fixes + Architectural Decisions Summary
# This commit addresses the remaining LOW priority issues and documents architectural decisions

git add Services/TransactionService.cs
git add Services/DatabaseMaintenanceService.cs
git add Components/Shared/TopBarInteractive.razor
git add Components/Shared/DemoBadge.razor
git add Controllers/AdminController.cs
git add Components/Shared/PrintableFormsPage.razor

git commit -m "fix: LOW priority issues and architectural decisions

- Replace magic strings with AppConstants (LOW-02)
  * TransactionService: 'Transfer' -> AppConstants.CategoryNames.Transfer
  * Recurrence patterns: literals -> AppConstants.RecurrencePatterns.*

- Improve logging consistency (LOW-08)
  * TopBarInteractive: Console.WriteLine -> Debug.WriteLine

- Add orphan detection (LOW-12)
  * DatabaseMaintenanceService: CheckForOrphansAsync() method
  * Detects orphaned splits, budget items, missing fund/donor refs

- Fix error response exposure (HIGH-10)
  * AdminController: Remove ex.Message from 500 responses

- Fix DateTime format bug
  * PrintableFormsPage: @DateTime.Now:MM/dd/yyyy -> .ToString()

- Fix JSException handling
  * PrintableFormsPage: DownloadFile() now catches JSException

Architectural Decisions Documented:
- Budget overspend: Allow + warn (design choice)
- Grant overspend: Allow + warn (design choice)
- Fund delete cascade: SetNull (preserves history)
- DateTime consistency: Gradual migration to UtcNow
- N+1 queries: EF Include() is SQL JOIN (not N+1)
- Virus scanning: Infrastructure feature (not code fix)
- Modal ARIA: Individual component audit needed
- Rate limiting: Already implemented via middleware
- Local storage: Works for single-server deployments
- Font Awesome: Tree-shakeable alternative recommended
- Cache busting: Add asp-append-version to links
- API versioning: Not needed for internal consumption

Total: ~160+ issues fixed across 35+ files"
