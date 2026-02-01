using NonProfitFinance.DTOs;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public interface IDocumentService
{
    Task<List<DocumentDto>> GetAllAsync(DocumentFilterRequest? filter = null);
    Task<DocumentDto?> GetByIdAsync(int id);
    Task<List<DocumentDto>> GetByGrantIdAsync(int grantId);
    Task<List<DocumentDto>> GetByDonorIdAsync(int donorId);
    Task<List<DocumentDto>> GetByTransactionIdAsync(int transactionId);
    Task<DocumentDto> UploadAsync(Stream fileStream, string fileName, string contentType, UploadDocumentRequest request);
    Task<(byte[] Content, string ContentType, string FileName)?> DownloadAsync(int id);
    Task<bool> DeleteAsync(int id);
    Task<DocumentDto?> UpdateAsync(int id, UpdateDocumentRequest request);
    Task<OcrResult?> ProcessDocumentOcrAsync(int documentId);
}

public record DocumentDto(
    int Id,
    string FileName,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    string? Description,
    DocumentType Type,
    int? GrantId,
    string? GrantName,
    int? DonorId,
    string? DonorName,
    int? TransactionId,
    DateTime UploadedAt,
    string? UploadedBy,
    string? Tags,
    bool IsArchived
);

public record UploadDocumentRequest(
    string? Description,
    DocumentType Type,
    int? GrantId,
    int? DonorId,
    int? TransactionId,
    string? Tags
);

public record UpdateDocumentRequest(
    string? Description,
    DocumentType Type,
    string? Tags,
    bool IsArchived
);

public record DocumentFilterRequest(
    DocumentType? Type = null,
    int? GrantId = null,
    int? DonorId = null,
    int? TransactionId = null,
    bool IncludeArchived = false,
    string? SearchTerm = null
);
