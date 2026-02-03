using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Models;
using NonProfitFinance.Models.Inventory;
using NonProfitFinance.Models.Maintenance;
using NonProfitFinance.Models.Shared;

namespace NonProfitFinance.Data;

/// <summary>
/// Application database context for nonprofit financial management.
/// Inherits from IdentityDbContext to support ASP.NET Identity authentication.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ==================== Financial Module ====================
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TransactionSplit> TransactionSplits => Set<TransactionSplit>();
    public DbSet<Fund> Funds => Set<Fund>();
    public DbSet<Donor> Donors => Set<Donor>();
    public DbSet<Grant> Grants => Set<Grant>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<CategorizationRule> CategorizationRules => Set<CategorizationRule>();
    public DbSet<ReportPreset> ReportPresets => Set<ReportPreset>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BudgetLineItem> BudgetLineItems => Set<BudgetLineItem>();
    public DbSet<ImportPreset> ImportPresets => Set<ImportPreset>();
    public DbSet<ScheduledReport> ScheduledReports => Set<ScheduledReport>();

    // ==================== Inventory Module ====================
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryCategory> InventoryCategories => Set<InventoryCategory>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

    // ==================== Maintenance Module ====================
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<MaintenanceTask> MaintenanceTasks => Set<MaintenanceTask>();
    public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Contractor> Contractors => Set<Contractor>();

    // ==================== Shared/Cross-Module ====================
    public DbSet<CustomField> CustomFields => Set<CustomField>();
    public DbSet<CustomFieldValue> CustomFieldValues => Set<CustomFieldValue>();
    public DbSet<UserModuleAccess> UserModuleAccess => Set<UserModuleAccess>();
    
    // ==================== Audit ====================
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    
    // ==================== Settings & Preferences ====================
    public DbSet<ReportColumnSettings> ReportColumnSettings => Set<ReportColumnSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // AuditLog: Performance indexes for querying
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(a => a.EntityType);
            entity.HasIndex(a => a.EntityId);
            entity.HasIndex(a => a.Timestamp);
            entity.HasIndex(a => new { a.EntityType, a.EntityId });
        });

        // Category: Self-referencing hierarchy
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Description).HasMaxLength(500);
            entity.Property(c => c.Color).HasMaxLength(7); // Hex color #RRGGBB

            entity.HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => new { c.ParentId, c.Name }).IsUnique();
        });

        // Transaction
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Description).HasMaxLength(500);
            entity.Property(t => t.Amount).HasPrecision(18, 2);
            entity.Property(t => t.Payee).HasMaxLength(200);
            entity.Property(t => t.Tags).HasMaxLength(500);

            entity.HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Fund)
                .WithMany(f => f.Transactions)
                .HasForeignKey(t => t.FundId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.Donor)
                .WithMany(d => d.Transactions)
                .HasForeignKey(t => t.DonorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.Grant)
                .WithMany(g => g.Transactions)
                .HasForeignKey(t => t.GrantId)
                .OnDelete(DeleteBehavior.SetNull);

            // Performance indexes
            entity.HasIndex(t => t.Date);
            entity.HasIndex(t => t.Type);
            entity.HasIndex(t => t.CategoryId);
            entity.HasIndex(t => t.FundId);
            entity.HasIndex(t => t.DonorId);
            entity.HasIndex(t => t.GrantId);
            entity.HasIndex(t => t.Payee);
            entity.HasIndex(t => t.ReferenceNumber);
            entity.HasIndex(t => t.PONumber);
            entity.HasIndex(t => new { t.Date, t.Type }); // Composite for reports
            entity.HasIndex(t => new { t.FundId, t.Date }); // Fund balance queries
            
            // Concurrency token for optimistic locking - use simple version counter for SQLite
            // Don't use IsRowVersion() as SQLite doesn't support automatic row versioning
            entity.Property(t => t.RowVersion)
                .HasDefaultValue(0u)
                .IsConcurrencyToken();
            
            // Soft delete query filter - excludes deleted records by default
            entity.HasQueryFilter(t => !t.IsDeleted);
            entity.HasIndex(t => t.IsDeleted);
        });

        // TransactionSplit
        modelBuilder.Entity<TransactionSplit>(entity =>
        {
            entity.HasKey(ts => ts.Id);
            entity.Property(ts => ts.Amount).HasPrecision(18, 2);
            entity.Property(ts => ts.Description).HasMaxLength(500);

            entity.HasOne(ts => ts.Transaction)
                .WithMany(t => t.Splits)
                .HasForeignKey(ts => ts.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ts => ts.Category)
                .WithMany()
                .HasForeignKey(ts => ts.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Fund
        modelBuilder.Entity<Fund>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Name).IsRequired().HasMaxLength(100);
            entity.Property(f => f.Description).HasMaxLength(500);
            entity.Property(f => f.Balance).HasPrecision(18, 2);
            
            // Unique constraint on Fund.Name - MED-02 fix
            entity.HasIndex(f => f.Name).IsUnique();
            
            // Concurrency token for optimistic locking - use simple version counter for SQLite
            // Don't use IsRowVersion() as SQLite doesn't support automatic row versioning
            entity.Property(f => f.RowVersion)
                .HasDefaultValue(0u)
                .IsConcurrencyToken();
        });

        // Donor
        modelBuilder.Entity<Donor>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(200);
            entity.Property(d => d.Email).HasMaxLength(254);
            entity.Property(d => d.Phone).HasMaxLength(20);
            entity.Property(d => d.Address).HasMaxLength(500);
            entity.Property(d => d.Notes).HasMaxLength(1000);
            entity.Property(d => d.TotalContributions).HasPrecision(18, 2);
            
            // Performance indexes
            entity.HasIndex(d => d.Name);
            entity.HasIndex(d => d.Type);
            entity.HasIndex(d => d.IsActive);
            entity.HasIndex(d => d.Email); // HIGH-02 fix
        });

        // Grant
        modelBuilder.Entity<Grant>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Name).IsRequired().HasMaxLength(200);
            entity.Property(g => g.GrantorName).IsRequired().HasMaxLength(200);
            entity.Property(g => g.Amount).HasPrecision(18, 2);
            entity.Property(g => g.Restrictions).HasMaxLength(1000);
            entity.Property(g => g.Notes).HasMaxLength(1000);
            
            // Concurrency token for optimistic locking - use simple version counter for SQLite
            entity.Property(g => g.RowVersion)
                .HasDefaultValue(0u)
                .IsConcurrencyToken();
            
            // Performance indexes
            entity.HasIndex(g => g.Status);
            entity.HasIndex(g => g.EndDate);
        });

        // Document
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.FileName).IsRequired().HasMaxLength(255);
            entity.Property(d => d.OriginalFileName).IsRequired().HasMaxLength(255);
            entity.Property(d => d.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(d => d.StoragePath).IsRequired().HasMaxLength(500);
            entity.Property(d => d.Description).HasMaxLength(500);
            entity.Property(d => d.Tags).HasMaxLength(500);
            entity.Property(d => d.UploadedBy).HasMaxLength(100);

            entity.HasOne(d => d.Grant)
                .WithMany()
                .HasForeignKey(d => d.GrantId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Donor)
                .WithMany()
                .HasForeignKey(d => d.DonorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.Transaction)
                .WithMany()
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Performance indexes - HIGH-02 fix
            entity.HasIndex(d => d.OriginalFileName);
            entity.HasIndex(d => d.Type);
            entity.HasIndex(d => d.UploadedAt);
            entity.HasIndex(d => d.IsArchived);
        });

        // CategorizationRule
        modelBuilder.Entity<CategorizationRule>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(200);
            entity.Property(r => r.MatchPattern).IsRequired().HasMaxLength(500);

            entity.HasOne(r => r.Category)
                .WithMany()
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => r.Priority);
            entity.HasIndex(r => r.IsActive);
        });

        // Budget
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Name).IsRequired().HasMaxLength(200);
            entity.Property(b => b.Year).IsRequired();

            entity.HasIndex(b => b.Year);
        });

        // BudgetLineItem
        modelBuilder.Entity<BudgetLineItem>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.CategoryName).IsRequired().HasMaxLength(100);

            entity.HasOne(b => b.Budget)
                .WithMany(b => b.LineItems)
                .HasForeignKey(b => b.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.Category)
                .WithMany()
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(b => new { b.BudgetId, b.CategoryId }).IsUnique();
        });

        // ==================== Inventory Module Entities ====================

        // InventoryItem
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Name).IsRequired().HasMaxLength(200);
            entity.Property(i => i.Description).HasMaxLength(1000);
            entity.Property(i => i.SKU).HasMaxLength(100);
            entity.Property(i => i.Barcode).HasMaxLength(100);
            entity.Property(i => i.UnitCost).HasPrecision(18, 2);

            entity.HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(i => i.Location)
                .WithMany(l => l.Items)
                .HasForeignKey(i => i.LocationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(i => i.Name);
            entity.HasIndex(i => i.SKU);
            entity.HasIndex(i => i.Barcode);
            entity.HasIndex(i => i.Status);
            entity.HasIndex(i => i.CategoryId);
            entity.HasIndex(i => i.LocationId);
        });

        // InventoryCategory
        modelBuilder.Entity<InventoryCategory>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Description).HasMaxLength(1000);
            entity.Property(c => c.Color).HasMaxLength(7);
            entity.Property(c => c.Icon).HasMaxLength(100);

            entity.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => c.Name);
            entity.HasIndex(c => c.ParentCategoryId);
        });

        // Location
        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Name).IsRequired().HasMaxLength(200);
            entity.Property(l => l.Description).HasMaxLength(1000);
            entity.Property(l => l.Code).HasMaxLength(50);
            entity.Property(l => l.Address).HasMaxLength(500);

            entity.HasOne(l => l.ParentLocation)
                .WithMany(l => l.SubLocations)
                .HasForeignKey(l => l.ParentLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(l => l.Name);
            entity.HasIndex(l => l.Code);
            entity.HasIndex(l => l.ParentLocationId);
        });

        // InventoryTransaction
        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.UnitCost).HasPrecision(18, 2);
            entity.Property(t => t.TotalCost).HasPrecision(18, 2);
            entity.Property(t => t.ReferenceNumber).HasMaxLength(100);
            entity.Property(t => t.PerformedBy).IsRequired().HasMaxLength(100);

            entity.HasOne(t => t.Item)
                .WithMany(i => i.Transactions)
                .HasForeignKey(t => t.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.FromLocation)
                .WithMany()
                .HasForeignKey(t => t.FromLocationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.ToLocation)
                .WithMany()
                .HasForeignKey(t => t.ToLocationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(t => t.LinkedTransaction)
                .WithMany()
                .HasForeignKey(t => t.LinkedTransactionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(t => t.ItemId);
            entity.HasIndex(t => t.TransactionDate);
            entity.HasIndex(t => t.Type);
        });

        // ==================== Maintenance Module Entities ====================

        // Building
        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Name).IsRequired().HasMaxLength(200);
            entity.Property(b => b.Description).HasMaxLength(1000);
            entity.Property(b => b.Code).HasMaxLength(50);
            entity.Property(b => b.Address).HasMaxLength(500);

            entity.HasOne(b => b.ParentBuilding)
                .WithMany(b => b.ChildBuildings)
                .HasForeignKey(b => b.ParentBuildingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(b => b.Name);
            entity.HasIndex(b => b.Code);
            entity.HasIndex(b => b.Type);
        });

        // Contractor
        modelBuilder.Entity<Contractor>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(300);
            entity.Property(c => c.ContactName).HasMaxLength(200);
            entity.Property(c => c.Phone).HasMaxLength(20);
            entity.Property(c => c.Email).HasMaxLength(200);
            entity.Property(c => c.HourlyRate).HasPrecision(18, 2);

            entity.HasIndex(c => c.Name);
            entity.HasIndex(c => c.IsActive);
        });

        // Project
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(300);
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.Property(p => p.CostEstimate).HasPrecision(18, 2);
            entity.Property(p => p.ActualCost).HasPrecision(18, 2);
            entity.Property(p => p.BudgetAmount).HasPrecision(18, 2);

            entity.HasOne(p => p.Building)
                .WithMany(b => b.Projects)
                .HasForeignKey(p => p.BuildingId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(p => p.Contractor)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.ContractorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(p => p.Fund)
                .WithMany()
                .HasForeignKey(p => p.FundId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(p => p.Grant)
                .WithMany()
                .HasForeignKey(p => p.GrantId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.Type);
            entity.HasIndex(p => p.Priority);
            entity.HasIndex(p => p.DueDate);
        });

        // MaintenanceTask
        modelBuilder.Entity<MaintenanceTask>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(300);
            entity.Property(t => t.Description).HasMaxLength(2000);
            entity.Property(t => t.EstimatedCost).HasPrecision(18, 2);
            entity.Property(t => t.ActualCost).HasPrecision(18, 2);

            entity.HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(t => t.ProjectId);
            entity.HasIndex(t => t.Status);
        });

        // ServiceRequest
        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.RequestNumber).HasMaxLength(50);
            entity.Property(r => r.Title).IsRequired().HasMaxLength(300);
            entity.Property(r => r.Description).IsRequired().HasMaxLength(4000);
            entity.Property(r => r.SubmittedBy).IsRequired().HasMaxLength(200);

            entity.HasOne(r => r.Building)
                .WithMany(b => b.ServiceRequests)
                .HasForeignKey(r => r.BuildingId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(r => r.Project)
                .WithMany()
                .HasForeignKey(r => r.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(r => r.WorkOrder)
                .WithMany()
                .HasForeignKey(r => r.WorkOrderId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(r => r.RequestNumber);
            entity.HasIndex(r => r.Status);
            entity.HasIndex(r => r.Priority);
            entity.HasIndex(r => r.SubmittedAt);
        });

        // WorkOrder
        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.WorkOrderNumber).HasMaxLength(50);
            entity.Property(w => w.Title).IsRequired().HasMaxLength(300);
            entity.Property(w => w.Description).IsRequired().HasMaxLength(4000);
            entity.Property(w => w.EstimatedCost).HasPrecision(18, 2);
            entity.Property(w => w.LaborCost).HasPrecision(18, 2);
            entity.Property(w => w.MaterialsCost).HasPrecision(18, 2);
            entity.Property(w => w.OtherCost).HasPrecision(18, 2);

            entity.HasOne(w => w.Project)
                .WithMany(p => p.WorkOrders)
                .HasForeignKey(w => w.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(w => w.Building)
                .WithMany(b => b.WorkOrders)
                .HasForeignKey(w => w.BuildingId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(w => w.Contractor)
                .WithMany(c => c.WorkOrders)
                .HasForeignKey(w => w.ContractorId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(w => w.WorkOrderNumber);
            entity.HasIndex(w => w.Status);
            entity.HasIndex(w => w.Priority);
            entity.HasIndex(w => w.ScheduledDate);
        });

        // ==================== Shared Entities ====================

        // CustomField
        modelBuilder.Entity<CustomField>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(f => f.Name).IsRequired().HasMaxLength(200);
            entity.Property(f => f.Label).HasMaxLength(200);
            entity.Property(f => f.Options).HasMaxLength(4000);

            entity.HasIndex(f => f.EntityType);
            entity.HasIndex(f => new { f.EntityType, f.Name }).IsUnique();
        });

        // CustomFieldValue
        modelBuilder.Entity<CustomFieldValue>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(v => v.StringValue).HasMaxLength(4000);
            entity.Property(v => v.NumericValue).HasPrecision(18, 6);

            entity.HasOne(v => v.CustomField)
                .WithMany(f => f.Values)
                .HasForeignKey(v => v.CustomFieldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(v => new { v.EntityType, v.EntityId });
            entity.HasIndex(v => v.CustomFieldId);
        });

        // UserModuleAccess
        modelBuilder.Entity<UserModuleAccess>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.UserId).IsRequired().HasMaxLength(450);
            entity.Property(a => a.ModuleName).IsRequired().HasMaxLength(100);
            entity.Property(a => a.SecondaryPasswordHash).HasMaxLength(256);

            entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => new { a.UserId, a.ModuleName }).IsUnique();
            entity.HasIndex(a => a.ModuleName);
        });
    }
}

