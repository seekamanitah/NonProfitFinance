using NonProfitFinance.Models;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.Online;
using OpenCvSharp;
using System.Text.RegularExpressions;
using System.Globalization;

namespace NonProfitFinance.Services;

/// <summary>
/// OCR service using PaddleOCR for text extraction from images
/// PaddleOCR provides higher accuracy and doesn't require manual model downloads
/// </summary>
public class OcrService : IOcrService, IDisposable
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<OcrService> _logger;
    private PaddleOcrAll? _ocr;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _disposed;

    public bool IsInitialized => _ocr != null;

    public OcrService(IWebHostEnvironment environment, ILogger<OcrService> logger)
    {
        _environment = environment;
        _logger = logger;
        
        // Initialize asynchronously
        _ = InitializeEngineAsync();
    }

    private async Task InitializeEngineAsync()
    {
        await _initLock.WaitAsync();
        try
        {
            if (_ocr != null) return;

            _logger.LogInformation("Initializing PaddleOCR engine...");
            
            // Use OnlineFullModels which will download models on first use
            var model = await OnlineFullModels.EnglishV4.DownloadAsync();
            
            _ocr = new PaddleOcrAll(model)
            {
                AllowRotateDetection = true, // Detect and correct image rotation
                Enable180Classification = true // Handle upside-down images
            };
            
            _logger.LogInformation("PaddleOCR engine initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize PaddleOCR engine");
            _ocr = null;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<OcrResult> ExtractTextFromImageAsync(Stream imageStream, OcrOptions? options = null)
    {
        // Ensure engine is initialized
        if (_ocr == null)
        {
            await InitializeEngineAsync();
            
            if (_ocr == null)
            {
                return new OcrResult
                {
                    Success = false,
                    ErrorMessage = "OCR engine not initialized. Models may be downloading. Please try again in a moment."
                };
            }
        }

        options ??= new OcrOptions();

        try
        {
            // Read image into memory
            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            // Convert bytes to Mat for PaddleOCR
            using var mat = Mat.FromImageData(imageBytes);
            
            // Process with PaddleOCR
            var result = _ocr.Run(mat);
            
            var extractedText = result.Text;
            var confidence = CalculateAverageConfidence(result);

            var ocrResult = new OcrResult
            {
                ExtractedText = extractedText,
                Confidence = confidence,
                Success = true
            };

            // Try to parse as receipt if requested
            if (options.ParseReceipt && confidence > 50)
            {
                ocrResult.ReceiptData = ParseReceiptData(extractedText);
            }

            return ocrResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OCR processing");
            return new OcrResult
            {
                Success = false,
                ErrorMessage = $"OCR processing failed: {ex.Message}"
            };
        }
    }

    public async Task<OcrResult> ExtractTextFromImageAsync(string imagePath, OcrOptions? options = null)
    {
        try
        {
            using var fileStream = File.OpenRead(imagePath);
            return await ExtractTextFromImageAsync(fileStream, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading image file: {Path}", imagePath);
            return new OcrResult
            {
                Success = false,
                ErrorMessage = $"Error reading file: {ex.Message}"
            };
        }
    }

    public async Task<OcrResult> ProcessReceiptAsync(Stream imageStream)
    {
        var options = new OcrOptions { ParseReceipt = true };
        return await ExtractTextFromImageAsync(imageStream, options);
    }

    private float CalculateAverageConfidence(PaddleOcrResult result)
    {
        if (result.Regions.Length == 0) return 0f;
        
        var totalConfidence = result.Regions.Sum(r => r.Score);
        return (totalConfidence / result.Regions.Length) * 100;
    }

    private ReceiptData ParseReceiptData(string text)
    {
        var receiptData = new ReceiptData
        {
            ReceiptConfidence = 0
        };

        // Parse merchant name (usually first non-empty line)
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > 0)
        {
            receiptData.Merchant = lines[0].Trim();
            receiptData.ReceiptConfidence += 20;
        }

        // Parse date (various formats)
        var datePatterns = new[]
        {
            @"\b(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})\b",
            @"\b(\d{4}[/-]\d{1,2}[/-]\d{1,2})\b",
            @"\b([A-Z][a-z]{2,8}\s+\d{1,2},?\s+\d{4})\b"
        };

        foreach (var pattern in datePatterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success && DateTime.TryParse(match.Value, out var date))
            {
                receiptData.Date = date;
                receiptData.ReceiptConfidence += 20;
                break;
            }
        }

        // Parse total amount
        var totalPatterns = new[]
        {
            @"(?:TOTAL|Total|AMOUNT|Amount|BALANCE|Balance)[\s:]*\$?(\d+\.?\d{0,2})",
            @"\$(\d+\.\d{2})\s*(?:TOTAL|Total|USD)",
            @"(?:^|\s)(\d+\.\d{2})\s*$" // Last line with price format
        };

        foreach (var pattern in totalPatterns)
        {
            var match = Regex.Match(text, pattern, RegexOptions.Multiline);
            if (match.Success && decimal.TryParse(match.Groups[1].Value, out var amount))
            {
                receiptData.Total = amount;
                receiptData.ReceiptConfidence += 30;
                break;
            }
        }

        // Parse line items (simple pattern: text followed by price)
        var itemPattern = @"^(.+?)\s+\$?(\d+\.?\d{0,2})$";
        foreach (var line in lines)
        {
            var match = Regex.Match(line.Trim(), itemPattern);
            if (match.Success && decimal.TryParse(match.Groups[2].Value, out var itemAmount))
            {
                var description = match.Groups[1].Value.Trim();
                if (!string.IsNullOrEmpty(description) && itemAmount > 0)
                {
                    receiptData.Items.Add(new ReceiptLineItem
                    {
                        Description = description,
                        Amount = itemAmount
                    });
                }
            }
        }

        if (receiptData.Items.Any())
        {
            receiptData.ReceiptConfidence += 30;
        }

        return receiptData;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _ocr?.Dispose();
        _initLock?.Dispose();
        _disposed = true;
    }
}

