# ??? Multi-Module Implementation Plan
## NonProfit Finance: Inventory & Building Maintenance Modules

**Date:** 2024  
**Scope:** Add Landing Page, Inventory Management, and Building Maintenance modules  
**Architecture:** Single .NET 10 Blazor Server app with shared EF Core database  
**Estimated Time:** 4-6 weeks

---

## ?? Executive Summary

This plan implements three major additions to the existing Financial module:
1. **Landing Page** - Module selector with role-based routing
2. **Inventory Module** - Complete inventory management system
3. **Building Maintenance Module** - Project/repair tracking system

All modules share the existing ApplicationDbContext and maintain UI consistency with the Financial module.

---

## ?? Phase 1: Foundation & Database (Days 1-3)

### Step 1.1: Database Schema Extension

**New Entities to Create:**

#### Inventory Module:
- `InventoryItem` - Core inventory item
- `InventoryCategory` - Category hierarchy
- `Location` - Storage locations (hierarchical)
- `InventoryTransaction` - Movement tracking
- `StockAdjustment` - Adjustments and corrections
- `Vendor` - Suppliers

#### Building Maintenance Module:
- `Project` - Maintenance projects
- `MaintenanceTask` - Individual tasks
- `ServiceRequest` - Repair requests
- `WorkOrder` - Work assignments
- `MaintenanceSchedule` - Preventive maintenance
- `Building` - Building/location hierarchy
- `Contractor` - External contractors

#### Shared/Cross-Module:
- `CustomField` - EAV field definitions
- `CustomFieldValue` - EAV values
- `UserModuleAccess` - Per-module permissions
- `AuditLog` - Cross-module audit trail

**Files to Create:**
- `Models/Inventory/InventoryItem.cs`
- `Models/Inventory/InventoryCategory.cs`
- `Models/Inventory/Location.cs`
- `Models/Inventory/InventoryTransaction.cs`
- `Models/Inventory/StockAdjustment.cs`
- `Models/Maintenance/Project.cs`
- `Models/Maintenance/MaintenanceTask.cs`
- `Models/Maintenance/ServiceRequest.cs`
- `Models/Maintenance/WorkOrder.cs`
- `Models/Maintenance/Building.cs`
- `Models/Maintenance/Contractor.cs`
- `Models/Shared/CustomField.cs`
- `Models/Shared/CustomFieldValue.cs`
- `Models/Shared/UserModuleAccess.cs`
- `Models/Enums/InventoryEnums.cs`
- `Models/Enums/MaintenanceEnums.cs`

**Update:**
- `Data/ApplicationDbContext.cs` - Add DbSets for all new entities
- `Data/DataSeeder.cs` - Add sample data for new modules

---

### Step 1.2: Authentication & Authorization

**Create Role Structure:**
- Extend ApplicationUser with ModuleAccess property
- Add roles: `Treasurer`, `InventoryManager`, `MaintenanceManager`, `Admin`
- Create `Services/ModuleAccessService.cs` for authorization checks

**Files to Create:**
- `Services/IModuleAccessService.cs`
- `Services/ModuleAccessService.cs`
- `Models/ModulePermission.cs`

**Update:**
- `Models/ApplicationUser.cs` - Add module access properties
- `Program.cs` - Register new services

---

### Step 1.3: Migration & Testing

**Tasks:**
- Create EF Core migration for all new entities
- Test migration up/down
- Seed initial data for development
- Verify no breaking changes to Financial module

**Commands:**
```bash
dotnet ef migrations add AddInventoryAndMaintenanceModules
dotnet ef database update
```

---

## ?? Phase 2: Landing Page (Days 4-5)

### Step 2.1: Landing Page Component

**Create:**
- `Components/Pages/LandingPage.razor` - Main landing page
- `Components/Shared/ModuleTile.razor` - Reusable module card
- `wwwroot/css/landing.css` - Landing page styles

**Features:**
- Large centered card with organization logo
- Three module tiles (Financial, Inventory, Maintenance)
- Role-based visibility and routing
- Quick stats summary bar
- User profile corner (name, role, logout)

**UI Specifications:**
```
???????????????????????????????????????????
?  [Logo] Organization Name               ?
???????????????????????????????????????????
?  ????????????  ????????????  ???????????
?  ?Financial ?  ?Inventory ?  ?Maint.  ??
?  ?  $$$     ?  ?  ??     ?  ?  ??    ??
?  ? [Treasurer]? [Manager] ?  [Manager]??
?  ????????????  ????????????  ???????????
???????????????????????????????????????????
?  Cash: $50K | Items: 1,234 | Open: 12  ?
???????????????????????????????????????????
```

---

### Step 2.2: Routing & Navigation

**Update:**
- `Components/Routes.razor` - Add landing page route
- `Components/Layout/NavMenu.razor` - Add module switcher
- `Components/Layout/MainLayout.razor` - Add breadcrumb for modules
- `Program.cs` - Set landing page as default route

**Route Structure:**
```
/ or /landing           ? Landing Page
/financial/*            ? Financial Module (existing)
/inventory/*            ? Inventory Module (new)
/maintenance/*          ? Maintenance Module (new)
```

---

## ?? Phase 3: Inventory Module - Core (Days 6-10)

### Step 3.1: Service Layer

**Create:**
- `Services/Inventory/IInventoryItemService.cs`
- `Services/Inventory/InventoryItemService.cs`
- `Services/Inventory/IInventoryCategoryService.cs`
- `Services/Inventory/InventoryCategoryService.cs`
- `Services/Inventory/ILocationService.cs`
- `Services/Inventory/LocationService.cs`
- `Services/Inventory/IInventoryTransactionService.cs`
- `Services/Inventory/InventoryTransactionService.cs`
- `DTOs/Inventory/InventoryDtos.cs`

**Register in Program.cs:**
```csharp
builder.Services.AddScoped<IInventoryItemService, InventoryItemService>();
builder.Services.AddScoped<IInventoryCategoryService, InventoryCategoryService>();
// ... etc
```

---

### Step 3.2: Layout & Navigation

**Create:**
- `Components/Inventory/Layout/InventoryLayout.razor`
- `Components/Inventory/Layout/InventoryNavMenu.razor`
- `wwwroot/css/inventory.css`

**Navigation Items:**
- Dashboard
- Items List
- Add Item
- Categories
- Locations
- Transactions
- Reports
- Settings

---

### Step 3.3: Dashboard

**Create:**
- `Components/Pages/Inventory/InventoryDashboard.razor`
- `Components/Inventory/Widgets/InventoryMetrics.razor`
- `Components/Inventory/Charts/StockLevelChart.razor`
- `Components/Inventory/Charts/CategoryBreakdownChart.razor`

**Features:**
- Top metrics (Total Items, Low Stock Alerts, Total Value)
- Bar chart by category
- Pie chart by status (In Stock, Low, Out of Stock)
- Recent activity table
- Quick actions (Add Item, Record Movement)

---

### Step 3.4: Items List & Management

**Create:**
- `Components/Pages/Inventory/ItemsList.razor`
- `Components/Pages/Inventory/ItemDetail.razor`
- `Components/Pages/Inventory/ItemForm.razor`
- `Components/Inventory/Shared/ItemCard.razor`

**Features:**
- Filterable/sortable table
- Search by name, SKU, category
- Bulk actions (adjust qty, update location)
- Detail view with history timeline
- Photo upload
- Custom fields rendering

---

## ?? Phase 4: Inventory Module - Advanced (Days 11-15)

### Step 4.1: Categories & Locations

**Create:**
- `Components/Pages/Inventory/CategoryManager.razor`
- `Components/Pages/Inventory/LocationManager.razor`
- `Components/Inventory/Shared/CategoryTreeView.razor`
- `Components/Inventory/Shared/LocationHierarchy.razor`

**Features:**
- Hierarchical tree view (reuse from Financial Categories)
- Drag-drop reordering
- CRUD operations
- Color/icon assignment
- Items per location view

---

### Step 4.2: Transactions & Movements

**Create:**
- `Components/Pages/Inventory/TransactionsList.razor`
- `Components/Pages/Inventory/RecordMovement.razor`
- `Components/Pages/Inventory/StockAdjustment.razor`

**Features:**
- Movement log (add, remove, transfer, adjust)
- Filters (date range, item, user, type)
- Reason codes
- Link to Financial transactions (purchases)
- Batch operations

---

### Step 4.3: Reports

**Create:**
- `Components/Pages/Inventory/InventoryReports.razor`
- `Services/Inventory/InventoryReportService.cs`

**Report Types:**
- Stock Levels by Category
- Low Stock Alert
- Usage Trends (line chart)
- Value by Location
- Movement History
- Exports (CSV, PDF, Excel)

---

### Step 4.4: Settings & Configuration

**Create:**
- `Components/Pages/Inventory/InventorySettings.razor`
- `Models/Inventory/InventorySettings.cs`
- `Services/Inventory/InventorySettingsService.cs`

**Features:**
- Default units, categories
- Low stock thresholds
- Auto-reorder settings
- Notification preferences
- Import/Export templates

---

## ?? Phase 5: Building Maintenance Module - Core (Days 16-20)

### Step 5.1: Service Layer

**Create:**
- `Services/Maintenance/IProjectService.cs`
- `Services/Maintenance/ProjectService.cs`
- `Services/Maintenance/IServiceRequestService.cs`
- `Services/Maintenance/ServiceRequestService.cs`
- `Services/Maintenance/IMaintenanceTaskService.cs`
- `Services/Maintenance/MaintenanceTaskService.cs`
- `Services/Maintenance/IContractorService.cs`
- `Services/Maintenance/ContractorService.cs`
- `DTOs/Maintenance/MaintenanceDtos.cs`

---

### Step 5.2: Layout & Navigation

**Create:**
- `Components/Maintenance/Layout/MaintenanceLayout.razor`
- `Components/Maintenance/Layout/MaintenanceNavMenu.razor`
- `wwwroot/css/maintenance.css`

**Navigation Items:**
- Dashboard
- Projects
- Service Requests
- Work Orders
- Preventive Maintenance
- Buildings
- Contractors
- Reports
- Settings

---

### Step 5.3: Dashboard

**Create:**
- `Components/Pages/Maintenance/MaintenanceDashboard.razor`
- `Components/Maintenance/Widgets/ProjectMetrics.razor`
- `Components/Maintenance/Charts/ProjectStatusChart.razor`
- `Components/Maintenance/Charts/CostTrendChart.razor`

**Features:**
- Metrics (Open Projects, Urgent Repairs, Budget Spent)
- Pie chart: Project status
- Bar chart: Costs by project type
- Calendar view: Upcoming maintenance
- Overdue alerts

---

### Step 5.4: Projects Management

**Create:**
- `Components/Pages/Maintenance/ProjectsList.razor`
- `Components/Pages/Maintenance/ProjectDetail.razor`
- `Components/Pages/Maintenance/ProjectForm.razor`
- `Components/Maintenance/Shared/ProjectTimeline.razor`
- `Components/Maintenance/Shared/TaskList.razor`

**Features:**
- Project list (filters: status, priority, type)
- Gantt-style timeline
- Task management
- Cost tracking (estimate vs actual)
- Link to Financial expenses/funds
- Document attachments
- Notes/comments

---

## ?? Phase 6: Building Maintenance Module - Advanced (Days 21-25)

### Step 6.1: Service Requests & Work Orders

**Create:**
- `Components/Pages/Maintenance/ServiceRequestsList.razor`
- `Components/Pages/Maintenance/ServiceRequestForm.razor`
- `Components/Pages/Maintenance/WorkOrdersList.razor`
- `Components/Pages/Maintenance/WorkOrderDetail.razor`

**Features:**
- Request submission form (area, urgency, description)
- Request queue with priority sorting
- Work order generation
- Assignment to contractors/staff
- Status tracking
- Completion checklist

---

### Step 6.2: Preventive Maintenance

**Create:**
- `Components/Pages/Maintenance/PreventiveSchedule.razor`
- `Components/Pages/Maintenance/MaintenanceCalendar.razor`
- `Components/Maintenance/Shared/RecurringTaskForm.razor`

**Features:**
- Calendar view
- Recurring task scheduling
- Completion tracking
- History log
- Auto-generate work orders
- Email reminders

---

### Step 6.3: Buildings & Contractors

**Create:**
- `Components/Pages/Maintenance/BuildingManager.razor`
- `Components/Pages/Maintenance/ContractorsList.razor`
- `Components/Pages/Maintenance/ContractorDetail.razor`
- `Components/Maintenance/Shared/BuildingHierarchy.razor`

**Features:**
- Building tree (Station ? Floor ? Room)
- Associated projects per location
- Contractor profiles (contact, services, rating)
- Contract management
- Performance tracking

---

### Step 6.4: Reports

**Create:**
- `Components/Pages/Maintenance/MaintenanceReports.razor`
- `Services/Maintenance/MaintenanceReportService.cs`

**Report Types:**
- Costs by Project Type
- Completion Rates
- Overdue Projects
- Contractor Performance
- Annual Maintenance Summary
- Budget vs Actual
- Exports (CSV, PDF)

---

## ?? Phase 7: Integration & Cross-Module Features (Days 26-28)

### Step 7.1: Financial Integration

**Create:**
- `Services/Integration/ModuleIntegrationService.cs`
- `Components/Shared/LinkedExpenseWidget.razor`
- `Components/Shared/BudgetAllocationWidget.razor`

**Features:**
- Link inventory purchases to Financial transactions
- Link project costs to Financial expenses
- Allocate funds to projects
- Budget tracking across modules
- Consolidated reporting

---

### Step 7.2: Custom Fields System (EAV)

**Create:**
- `Services/CustomFields/ICustomFieldService.cs`
- `Services/CustomFields/CustomFieldService.cs`
- `Components/Shared/CustomFieldEditor.razor`
- `Components/Shared/CustomFieldRenderer.razor`

**Features:**
- Define custom fields per entity type
- Field types: Text, Number, Date, Dropdown, Checkbox
- Dynamic form rendering
- Search/filter by custom fields
- Import/export custom data

---

### Step 7.3: Shared Components Library

**Create:**
- `Components/Shared/DataTable.razor` - Reusable table with sorting/filtering
- `Components/Shared/TreeView.razor` - Generic hierarchical tree
- `Components/Shared/Calendar.razor` - Calendar component
- `Components/Shared/FileUpload.razor` - Drag-drop file upload
- `Components/Shared/Timeline.razor` - Event timeline
- `Components/Shared/StatusBadge.razor` - Colored status indicators

---

## ?? Phase 8: UI Consistency & Polish (Days 29-30)

### Step 8.1: Theme Consistency

**Update:**
- `wwwroot/css/site.css` - Ensure variables used across all modules
- `wwwroot/css/inventory.css` - Match Financial theme
- `wwwroot/css/maintenance.css` - Match Financial theme
- `wwwroot/css/landing.css` - Unified look

**Color Palette:**
- Financial: Red accent (#dc3545)
- Inventory: Green accent (#28a745)
- Maintenance: Blue accent (#007bff)
- Base: Black/Gray (#212529, #6c757d)

---

### Step 8.2: Responsive Design

**Tasks:**
- Test all pages on mobile, tablet, desktop
- Ensure tables collapse properly
- Test sidebar collapsibility
- Verify touch-friendly buttons
- Test high-contrast mode

---

### Step 8.3: Accessibility

**Tasks:**
- Add ARIA labels to all interactive elements
- Test keyboard navigation
- Verify screen reader compatibility
- Ensure proper heading hierarchy
- Add skip links
- Test with existing accessibility features (OCR, TTS, UI scaling)

---

## ?? Phase 9: Testing & Documentation (Days 31-33)

### Step 9.1: Testing

**Create:**
- `NonProfitFinance.Tests/Inventory/` - Unit tests
- `NonProfitFinance.Tests/Maintenance/` - Unit tests
- `NonProfitFinance.Tests/Integration/` - Integration tests

**Test Coverage:**
- Service layer unit tests
- EF Core query tests
- Authorization tests
- Custom field system tests
- Cross-module integration tests

---

### Step 9.2: Documentation

**Create:**
- `MODULES_IMPLEMENTATION_COMPLETE.md` - Summary
- `INVENTORY_MODULE_GUIDE.md` - User guide
- `MAINTENANCE_MODULE_GUIDE.md` - User guide
- `DEVELOPER_GUIDE_MODULES.md` - Developer docs
- `Components/Pages/Help/HelpInventory.razor` - Help pages
- `Components/Pages/Help/HelpMaintenance.razor` - Help pages

---

### Step 9.3: User Training Materials

**Create:**
- Quick start guides
- Video tutorials (scripts)
- FAQ documents
- Troubleshooting guides

---

## ?? Phase 10: Deployment & Launch (Days 34-35)

### Step 10.1: Pre-Launch Checklist

- [ ] All migrations applied
- [ ] Sample data seeded
- [ ] All tests passing
- [ ] Documentation complete
- [ ] Performance testing done
- [ ] Security audit passed
- [ ] Backup system tested
- [ ] User training scheduled

---

### Step 10.2: Deployment

**Tasks:**
- Create deployment package
- Update database
- Deploy to production
- Monitor for issues
- Collect initial feedback

---

### Step 10.3: Post-Launch Support

**Week 1:**
- Daily monitoring
- Quick bug fixes
- User support
- Gather feedback

**Week 2-4:**
- Implement improvements
- Optimize performance
- Address user requests
- Plan future enhancements

---

## ?? Resource Allocation

### Developer Time Estimates:

| Phase | Days | Complexity |
|-------|------|------------|
| Phase 1: Foundation | 3 | High |
| Phase 2: Landing Page | 2 | Low |
| Phase 3: Inventory Core | 5 | High |
| Phase 4: Inventory Advanced | 5 | Medium |
| Phase 5: Maintenance Core | 5 | High |
| Phase 6: Maintenance Advanced | 5 | Medium |
| Phase 7: Integration | 3 | High |
| Phase 8: UI Polish | 2 | Low |
| Phase 9: Testing/Docs | 3 | Medium |
| Phase 10: Deployment | 2 | Medium |
| **Total** | **35 days** | **~7 weeks** |

---

## ? Success Criteria

### Phase Completion Criteria:

Each phase is complete when:
1. ? All code implemented and compiles
2. ? Unit tests written and passing
3. ? Manual testing completed
4. ? Documentation updated
5. ? Code reviewed
6. ? Merged to main branch

### Overall Success:

- All three modules fully functional
- No breaking changes to Financial module
- UI consistency maintained
- Role-based access working
- Performance acceptable (<2s page loads)
- Zero critical bugs
- User acceptance testing passed

---

## ?? Next Steps

**To begin implementation, I will:**
1. Start with Phase 1 (Foundation & Database)
2. Create all entity models
3. Update ApplicationDbContext
4. Create and apply migration
5. Implement basic seeding

**Ready to proceed?** Let me know if you want to:
- Start Phase 1 implementation
- Adjust the plan
- Focus on a specific module first
- See detailed code for any component

---

**Status:** ? Plan Complete - Ready for Implementation  
**Estimated Duration:** 7 weeks (1 developer)  
**Priority:** High  
**Risk Level:** Medium (large scope, good planning)
