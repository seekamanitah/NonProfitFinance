namespace NonProfitFinance.Models;

/// <summary>
/// Result of OCR text extraction from an image
/// </summary>
public class OcrResult
{
    /// <summary>
    /// Extracted text from the image
    /// </summary>
    public string ExtractedText { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score (0-100)
    /// </summary>
    public float Confidence { get; set; }

    /// <summary>
    /// Whether the OCR processing was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if processing failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Extracted receipt data (if detected as receipt)
    /// </summary>
    public ReceiptData? ReceiptData { get; set; }
}

/// <summary>
/// Parsed receipt data from OCR
/// </summary>
public class ReceiptData
{
    /// <summary>
    /// Merchant/vendor name
    /// </summary>
    public string? Merchant { get; set; }

    /// <summary>
    /// Transaction date
    /// </summary>
    public DateTime? Date { get; set; }

    /// <summary>
    /// Total amount
    /// </summary>
    public decimal? Total { get; set; }

    /// <summary>
    /// Line items if detected
    /// </summary>
    public List<ReceiptLineItem> Items { get; set; } = new();

    /// <summary>
    /// Confidence that this is a receipt
    /// </summary>
    public float ReceiptConfidence { get; set; }
}

/// <summary>
/// Individual line item from a receipt
/// </summary>
public class ReceiptLineItem
{
    public string? Description { get; set; }
    public decimal? Amount { get; set; }
    public int? Quantity { get; set; }
}

/// <summary>
/// OCR processing options
/// </summary>
public class OcrOptions
{
    /// <summary>
    /// Language for OCR (default: eng)
    /// </summary>
    public string Language { get; set; } = "eng";

    /// <summary>
    /// Whether to preprocess the image (resize, grayscale, etc.)
    /// </summary>
    public bool PreprocessImage { get; set; } = true;

    /// <summary>
    /// Whether to attempt receipt parsing
    /// </summary>
    public bool ParseReceipt { get; set; } = false;

    /// <summary>
    /// DPI for image processing (default: 300)
    /// </summary>
    public int Dpi { get; set; } = 300;
}
