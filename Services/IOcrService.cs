using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

/// <summary>
/// Service for OCR (Optical Character Recognition) processing
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Extract text from an image file
    /// </summary>
    Task<OcrResult> ExtractTextFromImageAsync(Stream imageStream, OcrOptions? options = null);

    /// <summary>
    /// Extract text from an image file path
    /// </summary>
    Task<OcrResult> ExtractTextFromImageAsync(string imagePath, OcrOptions? options = null);

    /// <summary>
    /// Process receipt image and extract structured data
    /// </summary>
    Task<OcrResult> ProcessReceiptAsync(Stream imageStream);

    /// <summary>
    /// Check if OCR engine is initialized and ready
    /// </summary>
    bool IsInitialized { get; }
}
