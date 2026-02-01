using NonProfitFinance.DTOs;

namespace NonProfitFinance.Services;

public interface IForm990Service
{
    /// <summary>
    /// Get Form 990 data for a fiscal year
    /// </summary>
    Task<Form990DataDto> GetForm990DataAsync(int fiscalYear);
    
    /// <summary>
    /// Export Form 990 data to PDF worksheet
    /// </summary>
    Task<byte[]> ExportForm990ToPdfAsync(int fiscalYear);
    
    /// <summary>
    /// Export Form 990 data to Excel with line mappings
    /// </summary>
    Task<byte[]> ExportForm990ToExcelAsync(int fiscalYear);
    
    /// <summary>
    /// Get revenue totals for 990 threshold check
    /// </summary>
    Task<decimal> GetGrossRevenueAsync(int fiscalYear);
    
    /// <summary>
    /// Determine which Form 990 is required based on revenue
    /// </summary>
    Task<Form990Type> GetRequiredFormTypeAsync(int fiscalYear);
}

public enum Form990Type
{
    Form990N,      // e-Postcard - Gross receipts ? $50,000
    Form990EZ,     // Short form - Gross receipts < $200,000 and total assets < $500,000
    Form990,       // Full form - Gross receipts ? $200,000 or total assets ? $500,000
    Form990PF      // Private foundations
}

public record Form990DataDto(
    int FiscalYear,
    Form990Type RequiredFormType,
    Form990PartI PartI,
    Form990PartVIII PartVIII,
    Form990PartIX PartIX,
    decimal TotalRevenue,
    decimal TotalExpenses,
    decimal NetAssets
);

// Part I - Summary
public record Form990PartI(
    decimal GrossReceipts,             // Line 6
    decimal Contributions,             // Line 8
    decimal ProgramServiceRevenue,     // Line 9
    decimal InvestmentIncome,          // Line 10
    decimal OtherRevenue,              // Line 11
    decimal TotalRevenue,              // Line 12
    decimal GrantsAndSimilar,          // Line 13
    decimal Benefits,                  // Line 14
    decimal Salaries,                  // Line 15
    decimal OtherExpenses,             // Line 16
    decimal TotalExpenses,             // Line 17 (13+14+15+16)
    decimal RevenueMinusExpenses,      // Line 18 (12-17)
    decimal TotalAssets,               // Line 19
    decimal TotalLiabilities,          // Line 20
    decimal NetAssets                  // Line 21 (19-20)
);

// Part VIII - Statement of Revenue
public record Form990PartVIII(
    // Contributions, Gifts, Grants (Lines 1a-1f)
    decimal FederatedCampaigns,        // 1a
    decimal MembershipDues,            // 1b
    decimal FundraisingEvents,         // 1c
    decimal RelatedOrganizations,      // 1d
    decimal GovernmentGrants,          // 1e
    decimal OtherContributions,        // 1f
    decimal TotalContributions,        // 1h
    
    // Program Service Revenue (Lines 2a-2g)
    List<ProgramServiceRevenueItem> ProgramServices,
    decimal TotalProgramServiceRevenue, // 2g
    
    // Other Revenue
    decimal InvestmentIncomeInterest,  // 3
    decimal DividendsAndInterest,      // 4
    decimal RentalIncome,              // 6a-c
    decimal GainLossAssets,            // 7
    decimal FundraisingGrossIncome,    // 8a
    decimal GamingGrossIncome,         // 9a
    decimal MiscRevenue,               // 11
    decimal TotalRevenue               // 12
);

public record ProgramServiceRevenueItem(
    string Description,
    string BusinessCode,
    decimal Amount
);

// Part IX - Statement of Functional Expenses
public record Form990PartIX(
    List<Form990ExpenseLine> ExpenseLines,
    decimal TotalFunctionalExpenses,
    decimal ProgramServicesTotal,
    decimal ManagementTotal,
    decimal FundraisingTotal
);

public record Form990ExpenseLine(
    int LineNumber,
    string Description,
    decimal Total,
    decimal ProgramServices,
    decimal ManagementAndGeneral,
    decimal Fundraising
);
