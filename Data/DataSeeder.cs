using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Models;

namespace NonProfitFinance.Data;

/// <summary>
/// Seeds default categories for nonprofit financial management.
/// Based on typical fire department/501(c)(3) category structures.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedDefaultCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync())
        {
            return; // Already seeded
        }

        // Seed Income Categories
        var incomeCategories = CreateIncomeCategories();
        context.Categories.AddRange(incomeCategories);

        // Seed Expense Categories
        var expenseCategories = CreateExpenseCategories();
        context.Categories.AddRange(expenseCategories);

        // Seed default funds
        var funds = CreateDefaultFunds();
        context.Funds.AddRange(funds);

        await context.SaveChangesAsync();
        
        // Seed inventory categories
        await SeedInventoryCategoriesAsync(context);
    }

    private static List<Category> CreateIncomeCategories()
    {
        var categories = new List<Category>();
        int sortOrder = 0;

        // Contributions (25-45%)
        var contributions = new Category
        {
            Name = "Contributions",
            Description = "Donations and contributions from individuals and organizations",
            Type = CategoryType.Income,
            Color = "#4CAF50",
            Icon = "heart",
            SortOrder = sortOrder++
        };
        categories.Add(contributions);

        var contributionSubs = new[]
        {
            ("Individual/Small Business", "Donations from individual donors and small businesses", "#66BB6A"),
            ("Corporate", "Corporate donations and sponsorships", "#81C784"),
            ("Legacies/Bequests", "Estate gifts and bequests", "#A5D6A7"),
            ("Donated Goods/Services", "In-kind donations of goods and services", "#C8E6C9"),
            ("In-Kind Gifts", "Non-cash gifts and volunteer services", "#E8F5E9")
        };
        foreach (var (name, desc, color) in contributionSubs)
        {
            categories.Add(new Category
            {
                Name = name,
                Description = desc,
                Type = CategoryType.Income,
                Color = color,
                Parent = contributions,
                SortOrder = sortOrder++
            });
        }

        // Grants (20-40%)
        var grants = new Category
        {
            Name = "Grants",
            Description = "Grant funding from government and private sources",
            Type = CategoryType.Income,
            Color = "#2196F3",
            Icon = "award",
            SortOrder = sortOrder++
        };
        categories.Add(grants);

        var grantSubs = new[]
        {
            ("Federal Grants", "Grants from federal government agencies", "#42A5F5"),
            ("State Grants", "Grants from state government", "#64B5F6"),
            ("Local Government Grants", "Grants from local/municipal government", "#90CAF9"),
            ("Foundation/Trust Grants", "Private foundation and trust grants", "#BBDEFB"),
            ("Nonprofit Organization Grants", "Grants from other nonprofit organizations", "#E3F2FD")
        };
        foreach (var (name, desc, color) in grantSubs)
        {
            categories.Add(new Category
            {
                Name = name,
                Description = desc,
                Type = CategoryType.Income,
                Color = color,
                Parent = grants,
                SortOrder = sortOrder++
            });
        }

        // Government Contracts/Fees (15-35%)
        var govContracts = new Category
        {
            Name = "Government Contracts/Fees",
            Description = "Revenue from government contracts and service fees",
            Type = CategoryType.Income,
            Color = "#9C27B0",
            Icon = "file-text",
            SortOrder = sortOrder++
        };
        categories.Add(govContracts);

        var govSubs = new[]
        {
            ("Local Contracts", "Contracts with local government entities", "#AB47BC"),
            ("Fire Protection Services", "Fees for fire protection services", "#BA68C8"),
            ("Emergency Response Fees", "Fees for emergency response services", "#CE93D8")
        };
        foreach (var (name, desc, color) in govSubs)
        {
            categories.Add(new Category
            {
                Name = name,
                Description = desc,
                Type = CategoryType.Income,
                Color = color,
                Parent = govContracts,
                SortOrder = sortOrder++
            });
        }

        // Fundraising Events (5-20%)
        var fundraising = new Category
        {
            Name = "Fundraising Events",
            Description = "Revenue from fundraising activities and events",
            Type = CategoryType.Income,
            Color = "#FF9800",
            Icon = "calendar",
            SortOrder = sortOrder++
        };
        categories.Add(fundraising);

        var fundSubs = new[]
        {
            ("Special Events Revenue", "Revenue from special fundraising events", "#FFA726"),
            ("Bingo/Raffles", "Revenue from bingo nights and raffles", "#FFB74D"),
            ("Non-Gift Event Income", "Event income that is not considered a gift", "#FFCC80")
        };
        foreach (var (name, desc, color) in fundSubs)
        {
            categories.Add(new Category
            {
                Name = name,
                Description = desc,
                Type = CategoryType.Income,
                Color = color,
                Parent = fundraising,
                SortOrder = sortOrder++
            });
        }

        // Investment/Other Income (0-10%)
        var investment = new Category
        {
            Name = "Investment/Other Income",
            Description = "Investment returns and miscellaneous income",
            Type = CategoryType.Income,
            Color = "#607D8B",
            Icon = "trending-up",
            SortOrder = sortOrder++
        };
        categories.Add(investment);

        var invSubs = new[]
        {
            ("Interest/Dividends", "Interest and dividend income from investments", "#78909C"),
            ("Asset Sales", "Income from sale of assets", "#90A4AE"),
            ("Insurance Recoveries", "Payments received from insurance claims", "#B0BEC5"),
            ("Miscellaneous Revenue", "Other miscellaneous income sources", "#CFD8DC")
        };
        foreach (var (name, desc, color) in invSubs)
        {
            categories.Add(new Category
            {
                Name = name,
                Description = desc,
                Type = CategoryType.Income,
                Color = color,
                Parent = investment,
                SortOrder = sortOrder++
            });
        }

        return categories;
    }

    private static List<Category> CreateExpenseCategories()
    {
        var categories = new List<Category>();
        int sortOrder = 100; // Start at 100 to separate from income

        // Personnel (40-60%)
        var personnel = new Category
        {
            Name = "Personnel",
            Description = "Salaries, benefits, and personnel-related expenses",
            Type = CategoryType.Expense,
            Color = "#F44336",
            Icon = "users",
            SortOrder = sortOrder++
        };
        categories.Add(personnel);

        var personnelSubs = new[]
        {
            ("Salaries/Wages", "Regular employee salaries and wages", "#EF5350"),
            ("Benefits/Pensions", "Employee benefits and pension contributions", "#E57373"),
            ("Payroll Taxes", "Employer payroll tax obligations", "#EF9A9A"),
            ("Volunteer Reimbursements", "Reimbursements to volunteers", "#FFCDD2"),
            ("Training Stipends", "Stipends for training and education", "#FFEBEE")
        };
        foreach (var (name, desc, color) in personnelSubs)
        {
            categories.Add(new Category
            {
                Name = name,
                Description = desc,
                Type = CategoryType.Expense,
                Color = color,
                Parent = personnel,
                SortOrder = sortOrder++
            });
        }

        // Equipment/Supplies (15-30%)
        var equipment = new Category
        {
            Name = "Equipment/Supplies",
            Description = "Equipment purchases and operational supplies",
            Type = CategoryType.Expense,
            Color = "#E91E63",
            Icon = "tool",
            SortOrder = sortOrder++
        };
        categories.Add(equipment);

        var equipSubs = new[]
        {
            ("Fire Gear/PPE", "Personal protective equipment and fire gear", "#EC407A"),
            ("Vehicles/Apparatus", "Fire trucks, ambulances, and other vehicles", "#F06292"),
            ("Medical Supplies", "Medical and first aid supplies", "#F48FB1"),
            ("Fuel", "Fuel for vehicles and equipment", "#F8BBD9"),
            ("Office Supplies", "General office supplies", "#FCE4EC")
        };
        foreach (var (name, desc, color) in equipSubs)
        {
            categories.Add(new Category
            {
                Name = name,
                Description = desc,
                Type = CategoryType.Expense,
                Color = color,
                Parent = equipment,
                SortOrder = sortOrder++
            });
        }

        // Maintenance/Repairs (8-15%)
        var maintenance = new Category
        {
            Name = "Maintenance/Repairs",
            Description = "Maintenance and repair expenses",
            Type = CategoryType.Expense,
            Color = "#795548",
            Icon = "wrench",
            SortOrder = sortOrder++
        };
        categories.Add(maintenance);

        var maintSubs = new[]
        {
            ("Vehicle Maintenance", "Maintenance of vehicles and apparatus", "#8D6E63"),
            ("Building Repairs", "Repairs to buildings and facilities", "#A1887F"),
            ("Equipment Servicing", "Servicing and repair of equipment", "#BCAAA4")
        };
        foreach (var (name, desc, color) in maintSubs)
        {
            categories.Add(new Category
            {
                Name = name,
                Description = desc,
                Type = CategoryType.Expense,
                Color = color,
                Parent = maintenance,
                SortOrder = sortOrder++
            });
        }

        // Facilities/Utilities (5-12%)
        var facilities = new Category
        {
            Name = "Facilities/Utilities",
            Description = "Facility costs and utility expenses",
            Type = CategoryType.Expense,
            Color = "#FF5722",
            Icon = "home",
            SortOrder = sortOrder++
        };
        categories.Add(facilities);

        var facSubs = new[]
        {
            ("Rent/Mortgage", "Building rent or mortgage payments", "#FF7043"),
            ("Utilities", "Electric, gas, water, and other utilities", "#FF8A65"),
            ("Insurance", "Property and liability insurance", "#FFAB91"),
            ("Property Taxes", "Property tax payments", "#FFCCBC")
        };
        foreach (var (name, desc, color) in facSubs)
        {
            categories.Add(new Category
            {
                Name = name,
                Description = desc,
                Type = CategoryType.Expense,
                Color = color,
                Parent = facilities,
                SortOrder = sortOrder++
            });
        }

        // Training/Education (3-10%)
        var training = new Category
        {
            Name = "Training/Education",
            Description = "Training and professional development expenses",
            Type = CategoryType.Expense,
            Color = "#3F51B5",
            Icon = "book",
            SortOrder = sortOrder++
        };
        categories.Add(training);

        var trainSubs = new[]
        {
            ("Courses/Certifications", "Training courses and certification programs", "#5C6BC0"),
            ("Conferences", "Conference attendance and registration fees", "#7986CB"),
            ("Staff Development", "General staff development activities", "#9FA8DA")
        };
        foreach (var (name, desc, color) in trainSubs)
        {
            categories.Add(new Category
            {
                Name = name,
                Description = desc,
                Type = CategoryType.Expense,
                Color = color,
                Parent = training,
                SortOrder = sortOrder++
            });
        }

        // Administrative (5-12%)
        var administrative = new Category
        {
            Name = "Administrative",
            Description = "Administrative and operational overhead",
            Type = CategoryType.Expense,
            Color = "#009688",
            Icon = "briefcase",
            SortOrder = sortOrder++
        };
        categories.Add(administrative);

        var adminSubs = new[]
        {
            ("Accounting/Legal Fees", "Professional accounting and legal services", "#26A69A"),
            ("Office Expenses", "General office operational expenses", "#4DB6AC"),
            ("Telecom/Postage", "Telecommunications and postal services", "#80CBC4"),
            ("Advertising", "Advertising and public relations", "#B2DFDB")
        };
        foreach (var (name, desc, color) in adminSubs)
        {
            categories.Add(new Category
            {
                Name = name,
                Description = desc,
                Type = CategoryType.Expense,
                Color = color,
                Parent = administrative,
                SortOrder = sortOrder++
            });
        }

        // Program/Operations (5-15%)
        var operations = new Category
        {
            Name = "Program/Operations",
            Description = "Program delivery and operational expenses",
            Type = CategoryType.Expense,
            Color = "#00BCD4",
            Icon = "activity",
            SortOrder = sortOrder++
        };
        categories.Add(operations);

        var opsSubs = new[]
        {
            ("Emergency Response Costs", "Direct costs of emergency responses", "#26C6DA"),
            ("Community Outreach", "Community education and outreach programs", "#4DD0E1"),
            ("Contract Services", "Contracted services for operations", "#80DEEA")
        };
        foreach (var (name, desc, color) in opsSubs)
        {
            categories.Add(new Category
            {
                Name = name,
                Description = desc,
                Type = CategoryType.Expense,
                Color = color,
                Parent = operations,
                SortOrder = sortOrder++
            });
        }

        // Fundraising Expenses (3-10%)
        var fundraisingExp = new Category
        {
            Name = "Fundraising Expenses",
            Description = "Costs associated with fundraising activities",
            Type = CategoryType.Expense,
            Color = "#FFEB3B",
            Icon = "dollar-sign",
            SortOrder = sortOrder++
        };
        categories.Add(fundraisingExp);

        var fundExpSubs = new[]
        {
            ("Event Costs", "Direct costs of fundraising events", "#FFEE58"),
            ("Donor Management", "Donor database and management costs", "#FFF176"),
            ("Marketing", "Marketing materials and campaigns", "#FFF59D")
        };
        foreach (var (name, desc, color) in fundExpSubs)
        {
            categories.Add(new Category
            {
                Name = name,
                Description = desc,
                Type = CategoryType.Expense,
                Color = color,
                Parent = fundraisingExp,
                SortOrder = sortOrder++
            });
        }

        return categories;
    }

    private static List<Fund> CreateDefaultFunds()
    {
        return new List<Fund>
        {
            new Fund
            {
                Name = "General Operating Fund",
                Type = FundType.Unrestricted,
                Description = "Main operating fund for general organizational expenses"
            },
            new Fund
            {
                Name = "Equipment Fund",
                Type = FundType.Restricted,
                Description = "Restricted fund for equipment purchases and maintenance"
            },
            new Fund
            {
                Name = "Training Fund",
                Type = FundType.Restricted,
                Description = "Restricted fund for training and professional development"
            },
            new Fund
            {
                Name = "Emergency Reserve",
                Type = FundType.Unrestricted,
                Description = "Reserve fund for unexpected expenses and emergencies"
            },
            new Fund
            {
                Name = "Building Fund",
                Type = FundType.Restricted,
                Description = "Restricted fund for building improvements and capital projects"
            }
        };
    }

    private static async Task SeedInventoryCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.InventoryCategories.AnyAsync())
        {
            return; // Already seeded
        }

        var categories = new List<Models.Inventory.InventoryCategory>
        {
            // Equipment
            new() { Name = "Fire Equipment", Description = "Firefighting equipment and tools", DisplayOrder = 1, IsActive = true },
            new() { Name = "Medical Equipment", Description = "Medical supplies and equipment", DisplayOrder = 2, IsActive = true },
            new() { Name = "Safety Equipment", Description = "Personal protective equipment", DisplayOrder = 3, IsActive = true },
            
            // Supplies
            new() { Name = "Office Supplies", Description = "General office supplies", DisplayOrder = 4, IsActive = true },
            new() { Name = "Cleaning Supplies", Description = "Cleaning and maintenance supplies", DisplayOrder = 5, IsActive = true },
            new() { Name = "Training Materials", Description = "Training and educational materials", DisplayOrder = 6, IsActive = true },
            
            // Tools
            new() { Name = "Hand Tools", Description = "Hand tools and equipment", DisplayOrder = 7, IsActive = true },
            new() { Name = "Power Tools", Description = "Power tools and machinery", DisplayOrder = 8, IsActive = true },
            
            // Vehicles
            new() { Name = "Vehicle Parts", Description = "Replacement parts for vehicles", DisplayOrder = 9, IsActive = true },
            new() { Name = "Vehicle Accessories", Description = "Vehicle accessories and attachments", DisplayOrder = 10, IsActive = true }
        };

        context.InventoryCategories.AddRange(categories);
        await context.SaveChangesAsync();

        // Add locations
        await SeedInventoryLocationsAsync(context);
    }

    private static async Task SeedInventoryLocationsAsync(ApplicationDbContext context)
    {
        if (await context.Locations.AnyAsync())
        {
            return; // Already seeded
        }

        var locations = new List<Models.Inventory.Location>
        {
            new() { Name = "Station 1", Description = "Main fire station", Code = "ST1", DisplayOrder = 1, IsActive = true },
            new() { Name = "Station 2", Description = "Secondary station", Code = "ST2", DisplayOrder = 2, IsActive = true },
            new() { Name = "Truck 1", Description = "Fire truck 1", Code = "T1", DisplayOrder = 3, IsActive = true },
            new() { Name = "Truck 2", Description = "Fire truck 2", Code = "T2", DisplayOrder = 4, IsActive = true },
            new() { Name = "Storage Room A", Description = "Main storage area", Code = "SRA", DisplayOrder = 5, IsActive = true },
            new() { Name = "Storage Room B", Description = "Secondary storage", Code = "SRB", DisplayOrder = 6, IsActive = true }
        };

        context.Locations.AddRange(locations);
        await context.SaveChangesAsync();
    }
}

