### AI Copilot Implementation Guide

#### Overview
Guide for AI copilot to build landing page and two new modules (Inventory Management, Building Maintenance/Services/Projects/Repairs) in single .NET Blazor app. Ensure shared EF Core DB context for compatibility with Financial module (e.g., link inventory items to expenses, projects to funds). Maintain consistent UI: red-black-gray palette (fire dept theme), high-contrast dark/light modes, collapsible left sidebar, top bar with logo/quick-add/search/profile, responsive design, large fonts, subtle shadows.

#### Prerequisites
- Existing Financial module with DB entities (Transaction, Category, Donor, Grant, Fund).
- Add new DB entities: InventoryItem (Id, Name, CategoryId, Qty, LocationId, CustomFields), Location (Id, Name, ParentId for hierarchy), Project (Id, Name, Type, Status, Priority, DueDate, AssignedTo, CostEstimate, ActualCost, LinkedExpenses), Vendor (Id, Name, Contact, Rating), MaintenanceTask (Id, ProjectId, Description, Schedule, CompletionDate).
- Use EAV for custom fields across all modules: CustomFields table (EntityType, FieldName, Type), CustomFieldValues (EntityId, FieldId, Value).
- Role-based auth: ASP.NET Identity with roles (Treasurer: Financial access; Member: Inventory; Admin: All). Future per-module password: UserModuleAccess table for secondary creds/logging.

#### Step 1: Landing Page
- Blazor component: Index.razor.
- Layout: Full-screen centered card.
- Elements:
  - Top: Org logo + name.
  - Middle: Two large tiles (Financial: red accent, treasurer icon; Inventory: green accent, gear icon; Building Maintenance: blue accent, wrench icon). Click redirects based on role (e.g., Treasurer to Financial dashboard).
  - Bottom: Role-visible quick stats (Cash Balance | Inventory Value | Open Projects).
  - User corner: Profile (name, role, logout).
- Auth check: Redirect unauthorized users to login or show access denied.
- Consistency: Use shared CSS (App.css) for palette/themes; match Financial's card/table styles.

#### Step 2: Inventory Module
- Base component: InventoryLayout.razor (inherits shared layout).
- Pages (route via NavMenu in sidebar, visible only if authorized):
  - Dashboard: Top metrics (items count, low stock, value); middle charts (bar by category, pie by status); bottom recent activity table.
  - Inventory List: Filterable table (name, category/sub, qty, location, status, last updated, actions). Bulk edit.
  - Items: Add/edit form (name, desc, category, unit, qty, location, min threshold, photo, custom fields). View: History timeline, photo.
  - Categories: Tree view, CRUD (add/edit/delete/reorder), color/icon.
  - Locations: List/cards (e.g., Truck 1), items per location.
  - Transactions/Movements: Log table (filters: date/item/user/type), add form.
  - Reports: Stock levels, low stock, usage trends (line chart), exports (CSV/PDF).
  - Settings: Defaults, notifications, import/export.
- DB Integration: Link Item costs to Financial Transactions (e.g., purchase expense). Custom fields dynamic render in forms/lists.
- UI Consistency: Reuse Financial's tree views, tables, charts (Chart.js/Syncfusion), filters, modals.

#### Step 3: Building Maintenance Module
- Base component: MaintenanceLayout.razor (inherits shared layout).
- Pages (sidebar nav, role-restricted):
  - Dashboard: Metrics (open projects, urgent repairs, budget spent); charts (pie overdue, bar costs).
  - Projects List: Table (name, type, status, priority, due, assigned, estimates, actuals).
  - Project Detail: Timeline (tasks/milestones), attachments, expense log (link to Financial), notes.
  - Repairs/Service Requests: Form for new, queue table (filters: area/urgency/reporter).
  - Preventive Maintenance: Calendar/schedule, recurring tasks list, history.
  - Locations/Buildings: Hierarchical tree (Station > Floor > Room), linked projects/repairs.
  - Vendors/Contractors: List (contact, services, rating, contracts).
  - Reports: Costs by project, completion rates, overdue trends, annual summary, exports.
- DB Integration: Link Project costs to Financial Expenses/Funds (restricted tracking). Custom fields in forms.
- UI Consistency: Match Financial/Inventory styles for tables, charts, forms; shared components for trees, calendars.

#### General Implementation Notes
- Routing: Use Blazor areas or prefixes (e.g., /financial, /inventory, /maintenance).
- Shared Components: Create reusable library (e.g., Shared project) for trees, tables, charts, custom field renderer.
- Testing: Unit test DB queries; integration test role access, custom fields.
- Future Per-Module Lock: On module entry, prompt modal for secondary password; log access in AuditLog table.
- Deployment: Single app publish; ensure DB migrations include new entities without breaking Financial.