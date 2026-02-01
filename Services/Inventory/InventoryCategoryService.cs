using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.DTOs.Inventory;
using NonProfitFinance.Models.Inventory;

namespace NonProfitFinance.Services.Inventory;

public class InventoryCategoryService : IInventoryCategoryService
{
    private readonly ApplicationDbContext _context;

    public InventoryCategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<InventoryCategoryDto>> GetAllAsync()
    {
        var categories = await _context.InventoryCategories
            .Include(c => c.ParentCategory)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(MapToDto).ToList();
    }

    public async Task<InventoryCategoryDto?> GetByIdAsync(int id)
    {
        var category = await _context.InventoryCategories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return null;

        var dto = MapToDto(category);
        
        // Load subcategories
        var subCategories = await _context.InventoryCategories
            .Include(c => c.ParentCategory)
            .Where(c => c.ParentCategoryId == id && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return dto with { SubCategories = subCategories.Select(MapToDto).ToList() };
    }

    public async Task<InventoryCategoryDto> CreateAsync(CreateInventoryCategoryRequest request)
    {
        var category = new InventoryCategory
        {
            Name = request.Name,
            Description = request.Description,
            ParentCategoryId = request.ParentCategoryId,
            IsActive = true
        };

        _context.InventoryCategories.Add(category);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(category.Id))!;
    }

    public async Task<InventoryCategoryDto?> UpdateAsync(int id, UpdateInventoryCategoryRequest request)
    {
        var category = await _context.InventoryCategories.FindAsync(id);
        if (category == null) return null;

        // Prevent circular reference
        if (request.ParentCategoryId.HasValue && await IsDescendantOf(id, request.ParentCategoryId.Value))
        {
            throw new InvalidOperationException("Cannot set parent to a descendant category");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.ParentCategoryId = request.ParentCategoryId;
        category.IsActive = request.IsActive;
        // No UpdatedAt in model

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.InventoryCategories.FindAsync(id);
        if (category == null) return false;

        if (!await CanDeleteAsync(id))
        {
            throw new InvalidOperationException("Cannot delete category with subcategories or items");
        }

        // Soft delete
        category.IsActive = false;
        // No UpdatedAt in model
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<InventoryCategoryDto>> GetRootCategoriesAsync()
    {
        var categories = await _context.InventoryCategories
            .Where(c => c.ParentCategoryId == null && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(MapToDto).ToList();
    }

    public async Task<List<InventoryCategoryDto>> GetSubCategoriesAsync(int parentId)
    {
        var categories = await _context.InventoryCategories
            .Include(c => c.ParentCategory)
            .Where(c => c.ParentCategoryId == parentId && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(MapToDto).ToList();
    }

    public async Task<List<InventoryCategoryDto>> GetCategoryTreeAsync()
    {
        var allCategories = await _context.InventoryCategories
            .Include(c => c.ParentCategory)
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var categoryDict = allCategories.ToDictionary(c => c.Id, c => MapToDto(c));

        // Build tree structure
        var rootCategories = new List<InventoryCategoryDto>();

        foreach (var category in categoryDict.Values)
        {
            if (category.ParentCategoryId == null)
            {
                rootCategories.Add(category);
            }
            else if (categoryDict.TryGetValue(category.ParentCategoryId.Value, out var parent))
            {
                var subCategories = parent.SubCategories?.ToList() ?? new List<InventoryCategoryDto>();
                subCategories.Add(category);
                categoryDict[parent.Id] = parent with { SubCategories = subCategories };
            }
        }

        return rootCategories;
    }

    public async Task<bool> MoveCategoryAsync(int categoryId, int? newParentId)
    {
        var category = await _context.InventoryCategories.FindAsync(categoryId);
        if (category == null) return false;

        // Prevent circular reference
        if (newParentId.HasValue && await IsDescendantOf(categoryId, newParentId.Value))
        {
            throw new InvalidOperationException("Cannot move category to a descendant");
        }

        category.ParentCategoryId = newParentId;
        // No UpdatedAt in model
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.InventoryCategories.AnyAsync(c => c.Id == id);
    }

    public async Task<bool> HasSubCategoriesAsync(int id)
    {
        return await _context.InventoryCategories.AnyAsync(c => c.ParentCategoryId == id && c.IsActive);
    }

    public async Task<bool> HasItemsAsync(int id)
    {
        return await _context.InventoryItems.AnyAsync(i => i.CategoryId == id && i.IsActive);
    }

    public async Task<bool> CanDeleteAsync(int id)
    {
        return !await HasSubCategoriesAsync(id) && !await HasItemsAsync(id);
    }

    private async Task<bool> IsDescendantOf(int categoryId, int potentialAncestorId)
    {
        if (categoryId == potentialAncestorId) return true;

        var category = await _context.InventoryCategories.FindAsync(potentialAncestorId);
        if (category?.ParentCategoryId == null) return false;

        return await IsDescendantOf(categoryId, category.ParentCategoryId.Value);
    }

    private InventoryCategoryDto MapToDto(InventoryCategory category)
    {
        var itemCount = _context.InventoryItems
            .Count(i => i.CategoryId == category.Id && i.IsActive);

        return new InventoryCategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.ParentCategoryId,
            category.ParentCategory?.Name,
            null, // ColorCode not in model
            null, // Icon not in model
            itemCount,
            category.IsActive,
            category.CreatedAt,
            null // SubCategories populated separately
        );
    }
}
