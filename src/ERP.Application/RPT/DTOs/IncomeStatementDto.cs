namespace ERP.Application.RPT.DTOs;

/// <summary>
/// Income Statement (Profit & Loss) report
/// </summary>
public class IncomeStatementDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Currency { get; set; } = "USD";

    // Revenue
    public List<AccountLineDto> RevenueAccounts { get; set; } = new();
    public decimal TotalRevenue { get; set; }

    // Cost of Goods Sold
    public List<AccountLineDto> CostOfGoodsSold { get; set; } = new();
    public decimal TotalCostOfGoodsSold { get; set; }

    public decimal GrossProfit { get; set; }
    public decimal GrossProfitMargin { get; set; }

    // Operating Expenses
    public List<AccountLineDto> OperatingExpenses { get; set; } = new();
    public decimal TotalOperatingExpenses { get; set; }

    public decimal OperatingIncome { get; set; }
    public decimal OperatingMargin { get; set; }

    // Other Income/Expenses
    public List<AccountLineDto> OtherIncome { get; set; } = new();
    public decimal TotalOtherIncome { get; set; }

    public List<AccountLineDto> OtherExpenses { get; set; } = new();
    public decimal TotalOtherExpenses { get; set; }

    // Net Income
    public decimal NetIncome { get; set; }
    public decimal NetProfitMargin { get; set; }
}
