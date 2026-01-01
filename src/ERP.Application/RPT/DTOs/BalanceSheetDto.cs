namespace ERP.Application.RPT.DTOs;

/// <summary>
/// Balance Sheet report showing Assets = Liabilities + Equity
/// </summary>
public class BalanceSheetDto
{
    public DateTime AsOfDate { get; set; }
    public string Currency { get; set; } = "USD";

    // Assets
    public List<AccountLineDto> CurrentAssets { get; set; } = new();
    public decimal TotalCurrentAssets { get; set; }

    public List<AccountLineDto> FixedAssets { get; set; } = new();
    public decimal TotalFixedAssets { get; set; }

    public List<AccountLineDto> OtherAssets { get; set; } = new();
    public decimal TotalOtherAssets { get; set; }

    public decimal TotalAssets { get; set; }

    // Liabilities
    public List<AccountLineDto> CurrentLiabilities { get; set; } = new();
    public decimal TotalCurrentLiabilities { get; set; }

    public List<AccountLineDto> LongTermLiabilities { get; set; } = new();
    public decimal TotalLongTermLiabilities { get; set; }

    public decimal TotalLiabilities { get; set; }

    // Equity
    public List<AccountLineDto> Equity { get; set; } = new();
    public decimal TotalEquity { get; set; }

    public decimal TotalLiabilitiesAndEquity { get; set; }

    // Validation
    public bool IsBalanced => Math.Abs(TotalAssets - TotalLiabilitiesAndEquity) < 0.01m;
}

public class AccountLineDto
{
    public string AccountNumber { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public decimal Balance { get; set; }
}
