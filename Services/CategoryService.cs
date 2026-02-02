using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.DTOs;
using NonProfitFinance.Models;

namespace NonProfitFinance.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    private const int MaxCategoryDepth = 5; // Limit nesting to 5 levels

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> GetAllAsync(CategoryType? type = null, bool includeArchived = false)
    {
        var query = _context.Categories.AsQueryable();

        if (type.HasValue)
            query = query.Where(c => c.Type == type.Value);

        if (!includeArchived)
            query = query.Where(c => !c.IsArchived);

        var categories = await query
            .Include(c => c.Parent)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return categories.Select(MapToDto).ToList();
    }

    public async Task<List<CategoryDto>> GetTreeAsync(CategoryType type)
    {
        var allCategories = await _context.Categories
            .Where(c => c.Type == type && !c.IsArchived)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        var rootCategories = allCategories.Where(c => c.ParentId == null).ToList();
        return rootCategories.Select(c => BuildTree(c, allCategories)).ToList();
    }

    private CategoryDto BuildTree(Category category, List<Category> allCategories)
    {
        var children = allCategories
            .Where(c => c.ParentId == category.Id)
            .Select(c => BuildTree(c, allCategories))
            .ToList();

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.Color,
            category.Icon,
            category.Type,
            category.BudgetLimit,
            category.IsArchived,
            category.SortOrder,
            category.ParentId,
            null,
            children.Count > 0 ? children : null
        );
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Parent)
            .Include(c => c.Children.Where(ch => !ch.IsArchived))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return null;

        return MapToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
    {
        // Validate no duplicate name at same level (case-insensitive)
        var exists = await _context.Categories.AnyAsync(c =>
            c.ParentId == request.ParentId &&
            c.Name.ToLower() == request.Name.ToLower() &&
            c.Type == request.Type);

        if (exists)
            throw new InvalidOperationException($"A category named '{request.Name}' already exists at this level (case-insensitive).");

        // Validate category depth limit
        if (request.ParentId.HasValue)
        {
            var depth = await GetCategoryDepthAsync(request.ParentId.Value);
            if (depth >= MaxCategoryDepth)
                throw new InvalidOperationException($"Cannot create category: maximum nesting depth of {MaxCategoryDepth} levels exceeded.");
        }

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            Color = request.Color,
            Icon = request.Icon,
            Type = request.Type,
            BudgetLimit = request.BudgetLimit,
            ParentId = request.ParentId,
            SortOrder = request.SortOrder
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    private async Task<int> GetCategoryDepthAsync(int categoryId)
    {
        int depth = 0;
        int? currentId = categoryId;
        
        while (currentId.HasValue && depth < MaxCategoryDepth + 1)
        {
            var category = await _context.Categories
                .Where(c => c.Id == currentId.Value)
                .Select(c => new { c.ParentId })
                .FirstOrDefaultAsync();
            
            if (category == null) break;
            
            depth++;
            currentId = category.ParentId;
        }
        
        return depth;
    }

    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return null;

        // Validate no duplicate name at same level (excluding self, case-insensitive)
        var exists = await _context.Categories.AnyAsync(c =>
            c.Id != id &&
            c.ParentId == request.ParentId &&
            c.Name.ToLower() == request.Name.ToLower() &&
            c.Type == category.Type);

        if (exists)
            throw new InvalidOperationException($"A category named '{request.Name}' already exists at this level (case-insensitive).");

        // Prevent circular reference
        if (request.ParentId.HasValue)
        {
            if (request.ParentId == id)
                throw new InvalidOperationException("A category cannot be its own parent.");

            if (await IsDescendantAsync(request.ParentId.Value, id))
                throw new InvalidOperationException("Cannot move a category under its own descendant.");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.Color = request.Color;
        category.Icon = request.Icon;
        category.BudgetLimit = request.BudgetLimit;
        category.ParentId = request.ParentId;
        category.SortOrder = request.SortOrder;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    private async Task<bool> IsDescendantAsync(int potentialDescendantId, int ancestorId)
    {
        var current = await _context.Categories.FindAsync(potentialDescendantId);
        while (current != null && current.ParentId.HasValue)
        {
            if (current.ParentId == ancestorId) return true;
            current = await _context.Categories.FindAsync(current.ParentId);
        }
        return false;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return false;

        // Check for transactions
        if (await HasTransactionsAsync(id))
            throw new InvalidOperationException("Cannot delete category with associated transactions. Archive it instead.");

        // Check for children
        if (category.Children.Any())
            throw new InvalidOperationException("Cannot delete category with subcategories. Delete or move them first.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ArchiveAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;

        category.IsArchived = true;
        category.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;

        category.IsArchived = false;
        category.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HasTransactionsAsync(int id)
    {
        return await _context.Transactions.AnyAsync(t => t.CategoryId == id) ||
               await _context.TransactionSplits.AnyAsync(ts => ts.CategoryId == id);
    }

    private static CategoryDto MapToDto(Category category)
    {
        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.Color,
            category.Icon,
            category.Type,
            category.BudgetLimit,
            category.IsArchived,
            category.SortOrder,
            category.ParentId,
            category.Parent?.Name,
            category.Children?.Where(c => !c.IsArchived)
                .Select(MapToDto)
                .ToList()
        );
    }
}
