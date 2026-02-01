namespace NonProfitFinance.Models;

public class Document
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DocumentType Type { get; set; }
    
    // Related entities (optional - a document can be linked to one of these)
    public int? GrantId { get; set; }
    public Grant? Grant { get; set; }
    
    public int? DonorId { get; set; }
    public Donor? Donor { get; set; }
    
    public int? TransactionId { get; set; }
    public Transaction? Transaction { get; set; }
    
    // Metadata
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string? UploadedBy { get; set; }
    public string? Tags { get; set; }
    public bool IsArchived { get; set; } = false;
}

public enum DocumentType
{
    General,
    GrantAgreement,
    GrantReport,
    Receipt,
    Invoice,
    Contract,
    DonorAcknowledgment,
    TaxDocument,
    BankStatement,
    Compliance,
    Other
}
