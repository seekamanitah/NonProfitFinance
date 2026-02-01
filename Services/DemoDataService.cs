using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public interface IDemoDataService
{
    /// <summary>
    /// Check if demo data exists in the database
    /// </summary>
    Task<bool> HasDemoDataAsync();
    
    /// <summary>
    /// Seed comprehensive demo data for all entities
    /// </summary>
    Task<DemoDataResult> SeedDemoDataAsync();
    
    /// <summary>
    /// Remove all demo data from the database
    /// </summary>
    Task<DemoDataResult> ClearDemoDataAsync();
}

public record DemoDataResult(
    bool Success,
    string Message,
    int TransactionsCreated,
    int DonorsCreated,
    int GrantsCreated,
    int FundsCreated
);

public class DemoDataService : IDemoDataService
{
    private readonly ApplicationDbContext _context;
    private readonly Random _random = new();
    
    // Demo data marker - transactions with this tag are demo data
    private const string DemoTag = "DEMO_DATA";

    public DemoDataService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasDemoDataAsync()
    {
        return await _context.Transactions.AnyAsync(t => t.Tags != null && t.Tags.Contains(DemoTag));
    }

    public async Task<DemoDataResult> SeedDemoDataAsync()
    {
        try
        {
            // Check if demo data already exists
            if (await HasDemoDataAsync())
            {
                return new DemoDataResult(false, "Demo data already exists. Clear it first.", 0, 0, 0, 0);
            }

            // Create Funds
            var funds = await CreateDemoFundsAsync();
            
            // Create Donors
            var donors = await CreateDemoDonorsAsync();
            
            // Create Grants
            var grants = await CreateDemoGrantsAsync();
            
            // Get categories for transactions
            var incomeCategories = await _context.Categories
                .Where(c => c.Type == CategoryType.Income && !c.IsArchived)
                .ToListAsync();
            var expenseCategories = await _context.Categories
                .Where(c => c.Type == CategoryType.Expense && !c.IsArchived)
                .ToListAsync();

            // Create Transactions (12 months of data)
            var transactions = await CreateDemoTransactionsAsync(
                incomeCategories, expenseCategories, donors, grants, funds);

            await _context.SaveChangesAsync();

            // Update fund balances
            foreach (var fund in funds)
            {
                await UpdateFundBalanceAsync(fund.Id);
            }

            // Update donor totals
            foreach (var donor in donors)
            {
                await UpdateDonorTotalsAsync(donor.Id);
            }

            // Update grant usage
            foreach (var grant in grants)
            {
                await UpdateGrantUsageAsync(grant.Id);
            }

            return new DemoDataResult(
                true,
                "Demo data created successfully!",
                transactions.Count,
                donors.Count,
                grants.Count,
                funds.Count
            );
        }
        catch (Exception ex)
        {
            return new DemoDataResult(false, $"Error: {ex.Message}", 0, 0, 0, 0);
        }
    }

    public async Task<DemoDataResult> ClearDemoDataAsync()
    {
        try
        {
            // Delete demo transactions (cascade deletes splits)
            var demoTransactions = await _context.Transactions
                .Where(t => t.Tags != null && t.Tags.Contains(DemoTag))
                .ToListAsync();
            _context.Transactions.RemoveRange(demoTransactions);

            // Delete demo donors
            var demoDonors = await _context.Donors
                .Where(d => d.Notes != null && d.Notes.Contains(DemoTag))
                .ToListAsync();
            _context.Donors.RemoveRange(demoDonors);

            // Delete demo grants
            var demoGrants = await _context.Grants
                .Where(g => g.Notes != null && g.Notes.Contains(DemoTag))
                .ToListAsync();
            _context.Grants.RemoveRange(demoGrants);

            // Delete demo funds (except default ones)
            var demoFunds = await _context.Funds
                .Where(f => f.Description != null && f.Description.Contains(DemoTag))
                .ToListAsync();
            _context.Funds.RemoveRange(demoFunds);

            await _context.SaveChangesAsync();

            return new DemoDataResult(
                true,
                "Demo data cleared successfully!",
                demoTransactions.Count,
                demoDonors.Count,
                demoGrants.Count,
                demoFunds.Count
            );
        }
        catch (Exception ex)
        {
            return new DemoDataResult(false, $"Error: {ex.Message}", 0, 0, 0, 0);
        }
    }

    private async Task<List<Fund>> CreateDemoFundsAsync()
    {
        var funds = new List<Fund>
        {
            new() { Name = "General Operating Fund", Type = FundType.Unrestricted, Description = $"Main operating fund. {DemoTag}", Balance = 0 },
            new() { Name = "Equipment Reserve", Type = FundType.Restricted, Description = $"Reserved for equipment purchases. {DemoTag}", Balance = 0 },
            new() { Name = "Training Fund", Type = FundType.TemporarilyRestricted, Description = $"Training and education expenses. {DemoTag}", Balance = 0 },
            new() { Name = "Building Maintenance", Type = FundType.Restricted, Description = $"Facility maintenance and repairs. {DemoTag}", Balance = 0 },
            new() { Name = "Emergency Reserve", Type = FundType.PermanentlyRestricted, Description = $"Emergency response reserve. {DemoTag}", Balance = 0 }
        };

        _context.Funds.AddRange(funds);
        await _context.SaveChangesAsync();
        return funds;
    }

    private async Task<List<Donor>> CreateDemoDonorsAsync()
    {
        var donors = new List<Donor>
        {
            new() { Name = "Smith Family Foundation", Type = DonorType.Foundation, Email = "contact@smithfamily.org", Phone = "(555) 123-4567", IsActive = true, Notes = DemoTag },
            new() { Name = "ABC Corporation", Type = DonorType.Corporate, Email = "giving@abccorp.com", Phone = "(555) 234-5678", IsActive = true, Notes = DemoTag },
            new() { Name = "John & Mary Williams", Type = DonorType.Individual, Email = "williams@email.com", Phone = "(555) 345-6789", IsActive = true, Notes = DemoTag },
            new() { Name = "Community First Bank", Type = DonorType.Corporate, Email = "community@cfbank.com", Phone = "(555) 456-7890", IsActive = true, Notes = DemoTag },
            new() { Name = "Robert Johnson", Type = DonorType.Individual, Email = "rjohnson@email.com", Phone = "(555) 567-8901", IsActive = true, Notes = DemoTag },
            new() { Name = "Tennessee State Government", Type = DonorType.Government, Email = "grants@tn.gov", Phone = "(615) 741-2000", IsActive = true, Notes = DemoTag },
            new() { Name = "Local Business Alliance", Type = DonorType.Corporate, Email = "info@lba.org", Phone = "(555) 678-9012", IsActive = true, Notes = DemoTag },
            new() { Name = "The Patterson Trust", Type = DonorType.Foundation, Email = "trust@patterson.org", Phone = "(555) 789-0123", IsActive = true, Notes = DemoTag },
            new() { Name = "Maria Garcia", Type = DonorType.Individual, Email = "mgarcia@email.com", Phone = "(555) 890-1234", IsActive = true, Notes = DemoTag },
            new() { Name = "Volunteer Fire Insurance Co", Type = DonorType.Corporate, Email = "support@vfic.com", Phone = "(555) 901-2345", IsActive = true, Notes = DemoTag }
        };

        _context.Donors.AddRange(donors);
        await _context.SaveChangesAsync();
        return donors;
    }

    private async Task<List<Grant>> CreateDemoGrantsAsync()
    {
        var today = DateTime.Today;
        var grants = new List<Grant>
        {
            new() 
            { 
                Name = "FEMA Assistance to Firefighters Grant", 
                GrantorName = "Federal Emergency Management Agency",
                Amount = 150000m,
                StartDate = today.AddMonths(-6),
                EndDate = today.AddMonths(18),
                Status = GrantStatus.Active,
                GrantNumber = "AFG-2024-001",
                Restrictions = "Equipment and training only",
                Notes = DemoTag
            },
            new() 
            { 
                Name = "Tennessee Fire Prevention Grant", 
                GrantorName = "TN Department of Commerce & Insurance",
                Amount = 25000m,
                StartDate = today.AddMonths(-3),
                EndDate = today.AddMonths(9),
                Status = GrantStatus.Active,
                GrantNumber = "TDFP-2024-123",
                Restrictions = "Fire prevention and education programs",
                Notes = DemoTag
            },
            new() 
            { 
                Name = "Community Development Block Grant", 
                GrantorName = "County Government",
                Amount = 50000m,
                StartDate = today.AddMonths(-12),
                EndDate = today.AddMonths(-1),
                Status = GrantStatus.Completed,
                GrantNumber = "CDBG-2023-456",
                Notes = DemoTag
            },
            new() 
            { 
                Name = "First Responder Equipment Grant", 
                GrantorName = "State Emergency Management Agency",
                Amount = 75000m,
                StartDate = today,
                EndDate = today.AddYears(2),
                Status = GrantStatus.Active,
                GrantNumber = "SEMA-2024-789",
                Restrictions = "First responder equipment and PPE",
                Notes = DemoTag,
                NextReportDueDate = today.AddMonths(3)
            },
            new() 
            { 
                Name = "Volunteer Recruitment Initiative", 
                GrantorName = "National Volunteer Fire Council",
                Amount = 10000m,
                StartDate = today.AddMonths(1),
                EndDate = today.AddMonths(13),
                Status = GrantStatus.Pending,
                Notes = DemoTag
            }
        };

        _context.Grants.AddRange(grants);
        await _context.SaveChangesAsync();
        return grants;
    }

    private async Task<List<Transaction>> CreateDemoTransactionsAsync(
        List<Category> incomeCategories,
        List<Category> expenseCategories,
        List<Donor> donors,
        List<Grant> grants,
        List<Fund> funds)
    {
        var transactions = new List<Transaction>();
        var today = DateTime.Today;

        // Sample payees for expenses
        var payees = new[]
        {
            "Fire Equipment Supply Co", "Uniforms Plus", "County Gas Station", "Office Depot",
            "Verizon Wireless", "Tennessee Electric", "Water Utility District", "Auto Parts Inc",
            "Training Center LLC", "Insurance Providers Inc", "Building Supplies Co", "Medical Supply Co"
        };

        // Generate 12 months of transactions
        for (int month = -11; month <= 0; month++)
        {
            var monthDate = today.AddMonths(month);
            var daysInMonth = DateTime.DaysInMonth(monthDate.Year, monthDate.Month);

            // Income transactions (3-5 per month)
            var incomeCount = _random.Next(3, 6);
            for (int i = 0; i < incomeCount && incomeCategories.Any(); i++)
            {
                var category = incomeCategories[_random.Next(incomeCategories.Count)];
                var donor = donors[_random.Next(donors.Count)];
                var fund = funds[_random.Next(funds.Count)];
                var amount = _random.Next(500, 15000);
                var day = _random.Next(1, daysInMonth + 1);

                transactions.Add(new Transaction
                {
                    Date = new DateTime(monthDate.Year, monthDate.Month, day),
                    Amount = amount,
                    Description = $"Contribution from {donor.Name}",
                    Type = TransactionType.Income,
                    CategoryId = category.Id,
                    FundType = fund.Type,
                    FundId = fund.Id,
                    DonorId = donor.Id,
                    Payee = donor.Name,
                    Tags = DemoTag
                });
            }

            // Grant income (1 per month for active grants)
            var activeGrants = grants.Where(g => g.Status == GrantStatus.Active).ToList();
            if (activeGrants.Any() && incomeCategories.Any())
            {
                var grant = activeGrants[_random.Next(activeGrants.Count)];
                var grantCategory = incomeCategories.FirstOrDefault(c => c.Name.Contains("Grant")) ?? incomeCategories[0];
                var amount = _random.Next(5000, 25000);
                var day = _random.Next(1, daysInMonth + 1);

                transactions.Add(new Transaction
                {
                    Date = new DateTime(monthDate.Year, monthDate.Month, day),
                    Amount = amount,
                    Description = $"Grant disbursement - {grant.Name}",
                    Type = TransactionType.Income,
                    CategoryId = grantCategory.Id,
                    FundType = FundType.Restricted,
                    FundId = funds.FirstOrDefault(f => f.Type == FundType.Restricted)?.Id,
                    GrantId = grant.Id,
                    Tags = DemoTag
                });
            }

            // Expense transactions (8-12 per month)
            var expenseCount = _random.Next(8, 13);
            for (int i = 0; i < expenseCount && expenseCategories.Any(); i++)
            {
                var category = expenseCategories[_random.Next(expenseCategories.Count)];
                var fund = funds[_random.Next(funds.Count)];
                var payee = payees[_random.Next(payees.Length)];
                var amount = _random.Next(50, 5000);
                var day = _random.Next(1, daysInMonth + 1);

                transactions.Add(new Transaction
                {
                    Date = new DateTime(monthDate.Year, monthDate.Month, day),
                    Amount = amount,
                    Description = $"{category.Name} - {payee}",
                    Type = TransactionType.Expense,
                    CategoryId = category.Id,
                    FundType = fund.Type,
                    FundId = fund.Id,
                    Payee = payee,
                    Tags = DemoTag
                });
            }

            // Recurring transactions (utilities, insurance, etc.)
            if (expenseCategories.Any())
            {
                // Electric bill
                var utilityCategory = expenseCategories.FirstOrDefault(c => c.Name.Contains("Utilit")) ?? expenseCategories[0];
                transactions.Add(new Transaction
                {
                    Date = new DateTime(monthDate.Year, monthDate.Month, 15),
                    Amount = _random.Next(400, 800),
                    Description = "Monthly electric bill",
                    Type = TransactionType.Expense,
                    CategoryId = utilityCategory.Id,
                    FundType = FundType.Unrestricted,
                    FundId = funds.FirstOrDefault(f => f.Type == FundType.Unrestricted)?.Id,
                    Payee = "Tennessee Electric",
                    Tags = $"{DemoTag},recurring",
                    IsRecurring = true
                });

                // Insurance
                var insuranceCategory = expenseCategories.FirstOrDefault(c => c.Name.Contains("Insur")) ?? expenseCategories[0];
                transactions.Add(new Transaction
                {
                    Date = new DateTime(monthDate.Year, monthDate.Month, 1),
                    Amount = 1500,
                    Description = "Monthly insurance premium",
                    Type = TransactionType.Expense,
                    CategoryId = insuranceCategory.Id,
                    FundType = FundType.Unrestricted,
                    FundId = funds.FirstOrDefault(f => f.Type == FundType.Unrestricted)?.Id,
                    Payee = "Fire Department Insurance Co",
                    Tags = $"{DemoTag},recurring",
                    IsRecurring = true
                });
            }
        }

        _context.Transactions.AddRange(transactions);
        return transactions;
    }

    private async Task UpdateFundBalanceAsync(int fundId)
    {
        var fund = await _context.Funds.FindAsync(fundId);
        if (fund == null) return;

        var income = await _context.Transactions
            .Where(t => t.FundId == fundId && t.Type == TransactionType.Income)
            .SumAsync(t => t.Amount);

        var expenses = await _context.Transactions
            .Where(t => t.FundId == fundId && t.Type == TransactionType.Expense)
            .SumAsync(t => t.Amount);

        fund.Balance = income - expenses;
        await _context.SaveChangesAsync();
    }

    private async Task UpdateDonorTotalsAsync(int donorId)
    {
        var donor = await _context.Donors.FindAsync(donorId);
        if (donor == null) return;

        donor.TotalContributions = await _context.Transactions
            .Where(t => t.DonorId == donorId && t.Type == TransactionType.Income)
            .SumAsync(t => t.Amount);

        donor.LastContributionDate = await _context.Transactions
            .Where(t => t.DonorId == donorId && t.Type == TransactionType.Income)
            .MaxAsync(t => (DateTime?)t.Date);

        await _context.SaveChangesAsync();
    }

    private async Task UpdateGrantUsageAsync(int grantId)
    {
        var grant = await _context.Grants.FindAsync(grantId);
        if (grant == null) return;

        grant.AmountUsed = await _context.Transactions
            .Where(t => t.GrantId == grantId && t.Type == TransactionType.Income)
            .SumAsync(t => t.Amount);

        await _context.SaveChangesAsync();
    }
}
