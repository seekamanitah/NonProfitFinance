using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IOcrService _ocrService;
    private readonly string _storagePath;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt",
        ".jpg", ".jpeg", ".png", ".gif", ".bmp",
        ".zip", ".rar"
    };
    private const long MaxFileSize = 50 * 1024 * 1024; // 50 MB

    public DocumentService(ApplicationDbContext context, IWebHostEnvironment environment, IOcrService ocrService)
    {
        _context = context;
        _ocrService = ocrService;
        _storagePath = Path.Combine(environment.ContentRootPath, "Documents");
        
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<List<DocumentDto>> GetAllAsync(DocumentFilterRequest? filter = null)
    {
        var query = _context.Documents
            .Include(d => d.Grant)
            .Include(d => d.Donor)
            .AsQueryable();

        if (filter != null)
        {
            if (filter.Type.HasValue)
                query = query.Where(d => d.Type == filter.Type.Value);

            if (filter.GrantId.HasValue)
                query = query.Where(d => d.GrantId == filter.GrantId.Value);

            if (filter.DonorId.HasValue)
                query = query.Where(d => d.DonorId == filter.DonorId.Value);

            if (filter.TransactionId.HasValue)
                query = query.Where(d => d.TransactionId == filter.TransactionId.Value);

            if (!filter.IncludeArchived)
                query = query.Where(d => !d.IsArchived);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                query = query.Where(d =>
                    d.OriginalFileName.ToLower().Contains(term) ||
                    (d.Description != null && d.Description.ToLower().Contains(term)) ||
                    (d.Tags != null && d.Tags.ToLower().Contains(term)));
            }
        }
        else
        {
            query = query.Where(d => !d.IsArchived);
        }

        return await query
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => MapToDto(d))
            .ToListAsync();
    }

    public async Task<DocumentDto?> GetByIdAsync(int id)
    {
        var doc = await _context.Documents
            .Include(d => d.Grant)
            .Include(d => d.Donor)
            .FirstOrDefaultAsync(d => d.Id == id);

        return doc == null ? null : MapToDto(doc);
    }

    public async Task<List<DocumentDto>> GetByGrantIdAsync(int grantId)
    {
        return await _context.Documents
            .Include(d => d.Grant)
            .Where(d => d.GrantId == grantId && !d.IsArchived)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => MapToDto(d))
            .ToListAsync();
    }

    public async Task<List<DocumentDto>> GetByDonorIdAsync(int donorId)
    {
        return await _context.Documents
            .Include(d => d.Donor)
            .Where(d => d.DonorId == donorId && !d.IsArchived)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => MapToDto(d))
            .ToListAsync();
    }

    public async Task<List<DocumentDto>> GetByTransactionIdAsync(int transactionId)
    {
        return await _context.Documents
            .Where(d => d.TransactionId == transactionId && !d.IsArchived)
            .OrderByDescending(d => d.UploadedAt)
            .Select(d => MapToDto(d))
            .ToListAsync();
    }

    public async Task<DocumentDto> UploadAsync(Stream fileStream, string fileName, string contentType, UploadDocumentRequest request)
    {
        // Validate file
        var extension = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"File type '{extension}' is not allowed.");
        }

        if (fileStream.Length > MaxFileSize)
        {
            throw new InvalidOperationException($"File size exceeds the maximum limit of {MaxFileSize / (1024 * 1024)} MB.");
        }

        // Generate unique filename
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var storagePath = Path.Combine(_storagePath, uniqueFileName);

        // Save file
        using (var fileOut = new FileStream(storagePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileOut);
        }

        // Create database record
        var document = new Document
        {
            FileName = uniqueFileName,
            OriginalFileName = fileName,
            ContentType = contentType,
            FileSize = fileStream.Length,
            StoragePath = storagePath,
            Description = request.Description,
            Type = request.Type,
            GrantId = request.GrantId,
            DonorId = request.DonorId,
            TransactionId = request.TransactionId,
            Tags = request.Tags,
            UploadedAt = DateTime.UtcNow
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        return MapToDto(document);
    }

    public async Task<(byte[] Content, string ContentType, string FileName)?> DownloadAsync(int id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null || !File.Exists(document.StoragePath))
            return null;

        var content = await File.ReadAllBytesAsync(document.StoragePath);
        return (content, document.ContentType, document.OriginalFileName);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null)
            return false;

        // Delete file if exists
        if (File.Exists(document.StoragePath))
        {
            File.Delete(document.StoragePath);
        }

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<DocumentDto?> UpdateAsync(int id, UpdateDocumentRequest request)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null)
            return null;

        document.Description = request.Description;
        document.Type = request.Type;
        document.Tags = request.Tags;
        document.IsArchived = request.IsArchived;

        await _context.SaveChangesAsync();

        return MapToDto(document);
    }

    private static DocumentDto MapToDto(Document d) => new(
        d.Id,
        d.FileName,
        d.OriginalFileName,
        d.ContentType,
        d.FileSize,
        d.Description,
        d.Type,
        d.GrantId,
        d.Grant?.Name,
        d.DonorId,
        d.Donor?.Name,
        d.TransactionId,
        d.UploadedAt,
        d.UploadedBy,
        d.Tags,
        d.IsArchived
    );

    public async Task<OcrResult?> ProcessDocumentOcrAsync(int documentId)
    {
        var document = await _context.Documents.FindAsync(documentId);
        if (document == null || !File.Exists(document.StoragePath))
            return null;

        // Check if file is an image
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        var extension = Path.GetExtension(document.OriginalFileName).ToLower();
        
        if (!imageExtensions.Contains(extension))
        {
            return new OcrResult
            {
                Success = false,
                ErrorMessage = "OCR only supports image files (jpg, jpeg, png, gif, bmp)"
            };
        }

        // Process with OCR
        var options = new OcrOptions
        {
            PreprocessImage = true,
            ParseReceipt = document.Type == DocumentType.Receipt
        };

        return await _ocrService.ExtractTextFromImageAsync(document.StoragePath, options);
    }
}
