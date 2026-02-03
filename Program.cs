using Microsoft.EntityFrameworkCore;
using NonProfitFinance.BackgroundServices;
using NonProfitFinance.Components;
using NonProfitFinance.Data;
using NonProfitFinance.Middleware;
using NonProfitFinance.Models;
using NonProfitFinance.Services;
using QuestPDF.Infrastructure;
using System.Globalization;

// Configure QuestPDF License (Community - free for organizations with less than $1M revenue)
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Set culture to US English to ensure $ currency symbol everywhere
var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Add database context with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
    
    // Configure warnings - suppress pending model changes since we use manual SQL for table creation
    options.ConfigureWarnings(warnings =>
    {
        warnings.Log(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuted);
        // Suppress pending model changes warning - we handle schema manually via ExecuteSqlRawAsync
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning);
    });
});

// Add ASP.NET Identity with secure cookie configuration
builder.Services.AddIdentity<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure cookie security
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
});

// Register application services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IDonorService, DonorService>();
builder.Services.AddScoped<IGrantService, GrantService>();
builder.Services.AddScoped<IFundService, FundService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReportCustomizationService, ReportCustomizationService>();
builder.Services.AddScoped<IReportPresetService, ReportPresetService>();
builder.Services.AddScoped<IScheduledReportService, ScheduledReportService>();
builder.Services.AddScoped<IImportExportService, ImportExportService>();
builder.Services.AddScoped<IImportPresetService, ImportPresetService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IPdfExportService, PdfExportService>();
builder.Services.AddScoped<IForm990Service, Form990Service>();
builder.Services.AddScoped<IComplianceService, ComplianceService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IRecurringTransactionService, RecurringTransactionService>();
builder.Services.AddScoped<IBankConnectionService, BankConnectionService>();
builder.Services.AddScoped<ICashFlowService, CashFlowService>();
builder.Services.AddScoped<IDemoDataService, DemoDataService>();
builder.Services.AddScoped<IAutoCategorizationService, AutoCategorizationService>();
builder.Services.AddScoped<IPrintableFormsService, PrintableFormsService>();
builder.Services.AddScoped<IPONumberService, PONumberService>();
builder.Services.AddScoped<IKeyboardShortcutService, KeyboardShortcutService>();
builder.Services.AddScoped<IAccessibilityService, AccessibilityService>();
builder.Services.AddScoped<ISpellCheckService, SpellCheckService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IDatabaseResetService, DatabaseResetService>();
builder.Services.AddScoped<IDataIntegrityService, DataIntegrityService>();
builder.Services.AddScoped<IToastService, ToastService>();
builder.Services.AddScoped<IDuplicateDetectionService, DuplicateDetectionService>();
builder.Services.AddHttpContextAccessor();

// Add HttpClient for Blazor Server components (needed for API calls from components)
builder.Services.AddScoped(sp => 
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var request = httpContextAccessor.HttpContext?.Request;
    var baseUri = request != null 
        ? $"{request.Scheme}://{request.Host}" 
        : "http://localhost:5000"; // Fallback for background services
    return new HttpClient { BaseAddress = new Uri(baseUri) };
});

builder.Services.AddSingleton<IBackupService, BackupService>();
builder.Services.AddSingleton<IOcrService, OcrService>();
builder.Services.AddScoped<ITextToSpeechService, TextToSpeechService>();
builder.Services.AddSingleton<IOrganizationService, OrganizationService>();

// Background Services
builder.Services.AddHostedService<BackupHostedService>();
builder.Services.AddHostedService<RecurringTransactionHostedService>();

// Add Health Checks
builder.Services.AddHealthChecks();

// HIGH-10 fix: Configure Blazor circuit options to hide detailed errors in production
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        // Only show detailed errors in development
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });

// Add controllers for API
builder.Services.AddControllers();

// Add API explorer and Swagger for development only - HIGH-03 fix
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Production error handling
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Global exception handler for all environments
// Use /error without the status code in the path to avoid loop issues
app.UseStatusCodePages(context =>
{
    var statusCode = context.HttpContext.Response.StatusCode;
    if (statusCode >= 400 && statusCode < 600)
    {
        context.HttpContext.Response.Redirect($"/error/{statusCode}");
    }
    return Task.CompletedTask;
});

// Security headers (CSP, X-Frame-Options, etc.)
app.UseSecurityHeaders();

// Rate limiting to prevent DoS attacks - HIGH-09 fix
app.UseRateLimiting();

// Request logging for API calls
app.UseRequestLogging();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

// Map Blazor components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Apply migrations and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // For fresh databases, delete if corrupted schema is detected
    var dbPath = context.Database.GetDbConnection().DataSource;
    
    // Check if database file exists
    bool needsRecreate = false;
    if (File.Exists(dbPath))
    {
        try
        {
            // Open connection and check schema
            await context.Database.OpenConnectionAsync();
            
            // Check if Funds table has RowVersion column using PRAGMA
            var connection = context.Database.GetDbConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "PRAGMA table_info(Funds)";
            
            bool hasRowVersion = false;
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var columnName = reader.GetString(1); // Column name is at index 1
                    if (columnName.Equals("RowVersion", StringComparison.OrdinalIgnoreCase))
                    {
                        hasRowVersion = true;
                        break;
                    }
                }
            }
            
            // If Funds table exists but doesn't have RowVersion, need to recreate
            if (!hasRowVersion)
            {
                // Check if Funds table exists at all
                cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Funds'";
                var tableExists = await cmd.ExecuteScalarAsync() != null;
                
                if (tableExists)
                {
                    needsRecreate = true;
                    Console.WriteLine("Database schema outdated (missing RowVersion column). Recreating database...");
                }
            }
            
            await context.Database.CloseConnectionAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking database schema: {ex.Message}");
            needsRecreate = true;
        }
    }
    
    if (needsRecreate)
    {
        context.Database.EnsureDeleted();
        Console.WriteLine("Old database deleted. Creating new database with correct schema...");
    }
    
    // Ensure database is created with fresh schema
    context.Database.EnsureCreated();
    
    // Add missing RowVersion columns to existing tables (for databases created before this column was added)
    try
    {
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE Funds ADD COLUMN RowVersion INTEGER NOT NULL DEFAULT 0;");
    }
    catch { /* Column already exists */ }
    
    try
    {
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE Grants ADD COLUMN RowVersion INTEGER NOT NULL DEFAULT 0;");
    }
    catch { /* Column already exists */ }
    
    try
    {
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE Transactions ADD COLUMN RowVersion INTEGER NOT NULL DEFAULT 0;");
    }
    catch { /* Column already exists */ }
    
    // Add missing Fund columns
    try
    {
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE Funds ADD COLUMN StartingBalance REAL NOT NULL DEFAULT 0;");
    }
    catch { /* Column already exists */ }
    
    try
    {
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE Funds ADD COLUMN TargetBalance REAL;");
    }
    catch { /* Column already exists */ }
    
    try
    {
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE Funds ADD COLUMN RestrictionExpiryDate TEXT;");
    }
    catch { /* Column already exists */ }
    
    // Add missing ImportPresets columns (for balance field and default fund selection)
    try
    {
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE ImportPresets ADD COLUMN BalanceColumn INTEGER;");
    }
    catch { /* Column already exists */ }
    
    try
    {
        await context.Database.ExecuteSqlRawAsync("ALTER TABLE ImportPresets ADD COLUMN DefaultFundId INTEGER;");
    }
    catch { /* Column already exists */ }
    
    // Ensure core financial tables exist (manual workaround for migration issues)
    
    // Create Categories table
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS Categories (
                Id INTEGER NOT NULL CONSTRAINT PK_Categories PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                Color TEXT,
                Icon TEXT,
                Type INTEGER NOT NULL,
                BudgetLimit REAL,
                IsArchived INTEGER NOT NULL DEFAULT 0,
                SortOrder INTEGER NOT NULL DEFAULT 0,
                ParentId INTEGER,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT,
                CONSTRAINT FK_Categories_Categories_ParentId FOREIGN KEY (ParentId) REFERENCES Categories (Id) ON DELETE RESTRICT
            );
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE UNIQUE INDEX IF NOT EXISTS IX_Categories_ParentId_Name ON Categories (ParentId, Name);
        ");
    }
    catch { /* Table already exists */ }
    
    // Create Funds table
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS Funds (
                Id INTEGER NOT NULL CONSTRAINT PK_Funds PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Type INTEGER NOT NULL,
                Description TEXT,
                StartingBalance REAL NOT NULL DEFAULT 0,
                Balance REAL NOT NULL DEFAULT 0,
                TargetBalance REAL,
                IsActive INTEGER NOT NULL DEFAULT 1,
                RestrictionExpiryDate TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT,
                RowVersion INTEGER NOT NULL DEFAULT 0
            );
        ");
    }
    catch { /* Table already exists */ }
    
    // Create Donors table
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS Donors (
                Id INTEGER NOT NULL CONSTRAINT PK_Donors PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Type INTEGER NOT NULL,
                Email TEXT,
                Phone TEXT,
                Address TEXT,
                Notes TEXT,
                TotalContributions REAL NOT NULL DEFAULT 0,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT
            );
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_Donors_Name ON Donors (Name);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_Donors_Type ON Donors (Type);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_Donors_IsActive ON Donors (IsActive);
        ");
    }
    catch { /* Table already exists */ }
    
    // Create Grants table
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS Grants (
                Id INTEGER NOT NULL CONSTRAINT PK_Grants PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                GrantorName TEXT NOT NULL,
                Amount REAL NOT NULL,
                Status INTEGER NOT NULL,
                StartDate TEXT NOT NULL,
                EndDate TEXT NOT NULL,
                Restrictions TEXT,
                Notes TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT,
                RowVersion INTEGER NOT NULL DEFAULT 0
            );
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_Grants_Status ON Grants (Status);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_Grants_EndDate ON Grants (EndDate);
        ");
    }
    catch { /* Table already exists */ }
    
    // Create Transactions table
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS Transactions (
                Id INTEGER NOT NULL CONSTRAINT PK_Transactions PRIMARY KEY AUTOINCREMENT,
                Date TEXT NOT NULL,
                Description TEXT,
                Amount REAL NOT NULL,
                Type INTEGER NOT NULL,
                Payee TEXT,
                Tags TEXT,
                CategoryId INTEGER NOT NULL,
                FundId INTEGER,
                DonorId INTEGER,
                GrantId INTEGER,
                PONumber TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT,
                RowVersion INTEGER NOT NULL DEFAULT 0,
                CONSTRAINT FK_Transactions_Categories_CategoryId FOREIGN KEY (CategoryId) REFERENCES Categories (Id) ON DELETE RESTRICT,
                CONSTRAINT FK_Transactions_Funds_FundId FOREIGN KEY (FundId) REFERENCES Funds (Id) ON DELETE SET NULL,
                CONSTRAINT FK_Transactions_Donors_DonorId FOREIGN KEY (DonorId) REFERENCES Donors (Id) ON DELETE SET NULL,
                CONSTRAINT FK_Transactions_Grants_GrantId FOREIGN KEY (GrantId) REFERENCES Grants (Id) ON DELETE SET NULL
            );
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_Transactions_Date ON Transactions (Date);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_Transactions_Type ON Transactions (Type);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_Transactions_CategoryId ON Transactions (CategoryId);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_Transactions_Date_Type ON Transactions (Date, Type);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_Transactions_PONumber ON Transactions (PONumber);
        ");
    }
    catch { /* Table already exists */ }
    
    // Create TransactionSplits table
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS TransactionSplits (
                Id INTEGER NOT NULL CONSTRAINT PK_TransactionSplits PRIMARY KEY AUTOINCREMENT,
                TransactionId INTEGER NOT NULL,
                CategoryId INTEGER NOT NULL,
                Amount REAL NOT NULL,
                Description TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT,
                CONSTRAINT FK_TransactionSplits_Transactions_TransactionId FOREIGN KEY (TransactionId) REFERENCES Transactions (Id) ON DELETE CASCADE,
                CONSTRAINT FK_TransactionSplits_Categories_CategoryId FOREIGN KEY (CategoryId) REFERENCES Categories (Id) ON DELETE RESTRICT
            );
        ");
    }
    catch { /* Table already exists */ }
    
    // Ensure Documents table exists (manual workaround for migration issues)
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS Documents (
                Id INTEGER NOT NULL CONSTRAINT PK_Documents PRIMARY KEY AUTOINCREMENT,
                FileName TEXT NOT NULL,
                OriginalFileName TEXT NOT NULL,
                ContentType TEXT NOT NULL,
                StoragePath TEXT NOT NULL,
                FileSize INTEGER NOT NULL,
                Description TEXT,
                Tags TEXT,
                Type INTEGER NOT NULL,
                UploadedAt TEXT NOT NULL,
                UploadedBy TEXT,
                GrantId INTEGER,
                DonorId INTEGER,
                TransactionId INTEGER,
                IsArchived INTEGER NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT,
                CONSTRAINT FK_Documents_Donors_DonorId FOREIGN KEY (DonorId) REFERENCES Donors (Id) ON DELETE SET NULL,
                CONSTRAINT FK_Documents_Grants_GrantId FOREIGN KEY (GrantId) REFERENCES Grants (Id) ON DELETE SET NULL,
                CONSTRAINT FK_Documents_Transactions_TransactionId FOREIGN KEY (TransactionId) REFERENCES Transactions (Id) ON DELETE SET NULL
            );
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_Documents_DonorId ON Documents (DonorId);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_Documents_GrantId ON Documents (GrantId);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_Documents_TransactionId ON Documents (TransactionId);
        ");
    }
    catch { /* Table already exists */ }
    
    // Ensure Budget tables exist
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS Budgets (
                Id INTEGER NOT NULL CONSTRAINT PK_Budgets PRIMARY KEY AUTOINCREMENT,
                Year INTEGER NOT NULL,
                Name TEXT NOT NULL,
                TotalBudget REAL NOT NULL,
                TotalSpent REAL NOT NULL,
                Remaining REAL NOT NULL,
                PercentageUsed REAL NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT
            );
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_Budgets_Year ON Budgets (Year);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS BudgetLineItems (
                Id INTEGER NOT NULL CONSTRAINT PK_BudgetLineItems PRIMARY KEY AUTOINCREMENT,
                BudgetId INTEGER NOT NULL,
                CategoryId INTEGER NOT NULL,
                CategoryName TEXT NOT NULL,
                CategoryColor TEXT,
                BudgetAmount REAL NOT NULL,
                ActualAmount REAL NOT NULL,
                Variance REAL NOT NULL,
                PercentageUsed REAL NOT NULL,
                CONSTRAINT FK_BudgetLineItems_Budgets_BudgetId FOREIGN KEY (BudgetId) REFERENCES Budgets (Id) ON DELETE CASCADE,
                CONSTRAINT FK_BudgetLineItems_Categories_CategoryId FOREIGN KEY (CategoryId) REFERENCES Categories (Id) ON DELETE RESTRICT
            );
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE UNIQUE INDEX IF NOT EXISTS IX_BudgetLineItems_BudgetId_CategoryId ON BudgetLineItems (BudgetId, CategoryId);
        ");
    }
    catch { /* Tables already exist */ }
    
    // Ensure CategorizationRules table exists
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS CategorizationRules (
                Id INTEGER NOT NULL CONSTRAINT PK_CategorizationRules PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                MatchType INTEGER NOT NULL,
                MatchPattern TEXT NOT NULL,
                CaseSensitive INTEGER NOT NULL DEFAULT 0,
                CategoryId INTEGER NOT NULL,
                IsActive INTEGER NOT NULL DEFAULT 1,
                Priority INTEGER NOT NULL DEFAULT 50,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT,
                CONSTRAINT FK_CategorizationRules_Categories_CategoryId FOREIGN KEY (CategoryId) REFERENCES Categories (Id) ON DELETE RESTRICT
            );
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_CategorizationRules_CategoryId ON CategorizationRules (CategoryId);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_CategorizationRules_IsActive_Priority ON CategorizationRules (IsActive, Priority);
        ");
    }
    catch { /* Table already exists */ }
    
    // Create Inventory module tables
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS InventoryCategories (
                Id INTEGER NOT NULL CONSTRAINT PK_InventoryCategories PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                Color TEXT,
                Icon TEXT,
                ParentCategoryId INTEGER,
                DisplayOrder INTEGER NOT NULL DEFAULT 0,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT,
                CONSTRAINT FK_InventoryCategories_InventoryCategories_ParentCategoryId FOREIGN KEY (ParentCategoryId) REFERENCES InventoryCategories (Id) ON DELETE RESTRICT
            );
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_InventoryCategories_Name ON InventoryCategories (Name);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_InventoryCategories_ParentCategoryId ON InventoryCategories (ParentCategoryId);
        ");
    }
    catch { /* Table already exists */ }
    
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS Locations (
                Id INTEGER NOT NULL CONSTRAINT PK_Locations PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                Code TEXT,
                Address TEXT,
                Type INTEGER NOT NULL DEFAULT 0,
                ParentLocationId INTEGER,
                DisplayOrder INTEGER NOT NULL DEFAULT 0,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT,
                CONSTRAINT FK_Locations_Locations_ParentLocationId FOREIGN KEY (ParentLocationId) REFERENCES Locations (Id) ON DELETE RESTRICT
            );
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_Locations_Name ON Locations (Name);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_Locations_Code ON Locations (Code);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_Locations_ParentLocationId ON Locations (ParentLocationId);
        ");
    }
    catch { /* Table already exists */ }
    
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS InventoryItems (
                Id INTEGER NOT NULL CONSTRAINT PK_InventoryItems PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                SKU TEXT,
                Barcode TEXT,
                CategoryId INTEGER,
                LocationId INTEGER,
                Quantity INTEGER NOT NULL DEFAULT 0,
                MinimumQuantity INTEGER NOT NULL DEFAULT 0,
                UnitCost REAL NOT NULL DEFAULT 0,
                Status INTEGER NOT NULL DEFAULT 0,
                Notes TEXT,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT,
                CONSTRAINT FK_InventoryItems_InventoryCategories_CategoryId FOREIGN KEY (CategoryId) REFERENCES InventoryCategories (Id) ON DELETE SET NULL,
                CONSTRAINT FK_InventoryItems_Locations_LocationId FOREIGN KEY (LocationId) REFERENCES Locations (Id) ON DELETE SET NULL
            );
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_InventoryItems_Name ON InventoryItems (Name);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_InventoryItems_SKU ON InventoryItems (SKU);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_InventoryItems_Barcode ON InventoryItems (Barcode);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_InventoryItems_Status ON InventoryItems (Status);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_InventoryItems_CategoryId ON InventoryItems (CategoryId);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_InventoryItems_LocationId ON InventoryItems (LocationId);
        ");
    }
    catch { /* Table already exists */ }
    
    try
    {
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS InventoryTransactions (
                Id INTEGER NOT NULL CONSTRAINT PK_InventoryTransactions PRIMARY KEY AUTOINCREMENT,
                ItemId INTEGER NOT NULL,
                Type INTEGER NOT NULL,
                Quantity INTEGER NOT NULL,
                UnitCost REAL NOT NULL,
                TotalCost REAL NOT NULL,
                TransactionDate TEXT NOT NULL,
                ReferenceNumber TEXT,
                Notes TEXT,
                PerformedBy TEXT NOT NULL,
                FromLocationId INTEGER,
                ToLocationId INTEGER,
                LinkedTransactionId INTEGER,
                CreatedAt TEXT NOT NULL,
                CONSTRAINT FK_InventoryTransactions_InventoryItems_ItemId FOREIGN KEY (ItemId) REFERENCES InventoryItems (Id) ON DELETE CASCADE,
                CONSTRAINT FK_InventoryTransactions_Locations_FromLocationId FOREIGN KEY (FromLocationId) REFERENCES Locations (Id) ON DELETE SET NULL,
                CONSTRAINT FK_InventoryTransactions_Locations_ToLocationId FOREIGN KEY (ToLocationId) REFERENCES Locations (Id) ON DELETE SET NULL,
                CONSTRAINT FK_InventoryTransactions_Transactions_LinkedTransactionId FOREIGN KEY (LinkedTransactionId) REFERENCES Transactions (Id) ON DELETE SET NULL
            );
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_InventoryTransactions_ItemId ON InventoryTransactions (ItemId);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_InventoryTransactions_TransactionDate ON InventoryTransactions (TransactionDate);
        ");
        
        await context.Database.ExecuteSqlRawAsync(@"
            CREATE INDEX IF NOT EXISTS IX_InventoryTransactions_Type ON InventoryTransactions (Type);
        ");
    }
    catch { /* Table already exists */ }
    
    // Seed default categories if none exist
    await DataSeeder.SeedDefaultCategoriesAsync(context);
}

app.Run();


