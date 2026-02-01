using Microsoft.EntityFrameworkCore;
using NonProfitFinance.Data;
using NonProfitFinance.DTOs.Inventory;
using NonProfitFinance.Models.Enums;
using NonProfitFinance.Models.Inventory;

namespace NonProfitFinance.Services.Inventory;

public class LocationService : ILocationService
{
    private readonly ApplicationDbContext _context;

    public LocationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LocationDto>> GetAllAsync()
    {
        var locations = await _context.Locations
            .Include(l => l.ParentLocation)
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync();

        return locations.Select(MapToDto).ToList();
    }

    public async Task<LocationDto?> GetByIdAsync(int id)
    {
        var location = await _context.Locations
            .Include(l => l.ParentLocation)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (location == null) return null;

        var dto = MapToDto(location);

        // Load sublocations
        var subLocations = await _context.Locations
            .Include(l => l.ParentLocation)
            .Where(l => l.ParentLocationId == id && l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync();

        return dto with { SubLocations = subLocations.Select(MapToDto).ToList() };
    }

    public async Task<LocationDto> CreateAsync(CreateLocationRequest request)
    {
        var location = new Location
        {
            Name = request.Name,
            Description = request.Description,
            ParentLocationId = request.ParentLocationId,
            Address = request.Address,
            ContactPerson = request.ContactPerson,
            ContactPhone = request.Phone,
            IsActive = true
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(location.Id))!;
    }

    public async Task<LocationDto?> UpdateAsync(int id, UpdateLocationRequest request)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null) return null;

        // Prevent circular reference
        if (request.ParentLocationId.HasValue && await IsDescendantOf(id, request.ParentLocationId.Value))
        {
            throw new InvalidOperationException("Cannot set parent to a descendant location");
        }

        location.Name = request.Name;
        location.Description = request.Description;
        location.ParentLocationId = request.ParentLocationId;
        location.Address = request.Address;
        location.ContactPerson = request.ContactPerson;
        location.ContactPhone = request.Phone;
        location.IsActive = request.IsActive;
        // No UpdatedAt in model

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null) return false;

        if (!await CanDeleteAsync(id))
        {
            throw new InvalidOperationException("Cannot delete location with sublocations or items");
        }

        // Soft delete
        location.IsActive = false;
        // No UpdatedAt in model
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<LocationDto>> GetRootLocationsAsync()
    {
        var locations = await _context.Locations
            .Where(l => l.ParentLocationId == null && l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync();

        return locations.Select(MapToDto).ToList();
    }

    public async Task<List<LocationDto>> GetSubLocationsAsync(int parentId)
    {
        var locations = await _context.Locations
            .Include(l => l.ParentLocation)
            .Where(l => l.ParentLocationId == parentId && l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync();

        return locations.Select(MapToDto).ToList();
    }

    public async Task<List<LocationDto>> GetLocationTreeAsync()
    {
        var allLocations = await _context.Locations
            .Include(l => l.ParentLocation)
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync();

        var locationDict = allLocations.ToDictionary(l => l.Id, l => MapToDto(l));

        // Build tree structure
        var rootLocations = new List<LocationDto>();

        foreach (var location in locationDict.Values)
        {
            if (location.ParentLocationId == null)
            {
                rootLocations.Add(location);
            }
            else if (locationDict.TryGetValue(location.ParentLocationId.Value, out var parent))
            {
                var subLocations = parent.SubLocations?.ToList() ?? new List<LocationDto>();
                subLocations.Add(location);
                locationDict[parent.Id] = parent with { SubLocations = subLocations };
            }
        }

        return rootLocations;
    }

    public async Task<bool> MoveLocationAsync(int locationId, int? newParentId)
    {
        var location = await _context.Locations.FindAsync(locationId);
        if (location == null) return false;

        // Prevent circular reference
        if (newParentId.HasValue && await IsDescendantOf(locationId, newParentId.Value))
        {
            throw new InvalidOperationException("Cannot move location to a descendant");
        }

        location.ParentLocationId = newParentId;
        // No UpdatedAt in model
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<LocationDto>> GetByTypeAsync(LocationType type)
    {
        // Location model doesn't have Type property
        // Return all locations as a workaround
        return await GetAllAsync();
    }

    public async Task<List<InventoryItemDto>> GetItemsAtLocationAsync(int locationId)
    {
        var items = await _context.InventoryItems
            .Include(i => i.Category)
            .Include(i => i.Location)
            .Where(i => i.LocationId == locationId && i.IsActive)
            .OrderBy(i => i.Name)
            .ToListAsync();

        return items.Select(MapToItemDto).ToList();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Locations.AnyAsync(l => l.Id == id);
    }

    public async Task<bool> HasSubLocationsAsync(int id)
    {
        return await _context.Locations.AnyAsync(l => l.ParentLocationId == id && l.IsActive);
    }

    public async Task<bool> HasItemsAsync(int id)
    {
        return await _context.InventoryItems.AnyAsync(i => i.LocationId == id && i.IsActive);
    }

    public async Task<bool> CanDeleteAsync(int id)
    {
        return !await HasSubLocationsAsync(id) && !await HasItemsAsync(id);
    }

    private async Task<bool> IsDescendantOf(int locationId, int potentialAncestorId)
    {
        if (locationId == potentialAncestorId) return true;

        var location = await _context.Locations.FindAsync(potentialAncestorId);
        if (location?.ParentLocationId == null) return false;

        return await IsDescendantOf(locationId, location.ParentLocationId.Value);
    }

    private LocationDto MapToDto(Location location)
    {
        var itemCount = _context.InventoryItems
            .Count(i => i.LocationId == location.Id && i.IsActive);

        return new LocationDto(
            location.Id,
            location.Name,
            location.Description,
            location.ParentLocationId,
            location.ParentLocation?.Name,
            LocationType.Facility, // Default since model doesn't have Type property
            location.Address,
            location.ContactPerson,
            location.ContactPhone,
            itemCount,
            location.IsActive,
            location.CreatedAt,
            null // SubLocations populated separately
        );
    }

    private static InventoryItemDto MapToItemDto(InventoryItem item)
    {
        return new InventoryItemDto(
            item.Id,
            item.Name,
            item.SKU,
            item.Description,
            item.CategoryId,
            item.Category?.Name,
            item.LocationId,
            item.Location?.Name,
            item.Quantity,
            item.Unit.ToString(),
            item.MinimumQuantity,
            item.MaximumQuantity,
            null, // ReorderPoint not in model
            item.UnitCost ?? 0,
            item.Quantity * (item.UnitCost ?? 0),
            item.Status,
            item.PhotoUrl,
            item.Barcode,
            item.Manufacturer,
            null, // Supplier not in model
            item.PurchaseDate,
            item.ExpirationDate,
            item.Notes,
            item.IsActive,
            item.CreatedAt,
            item.UpdatedAt ?? item.CreatedAt // Use CreatedAt if UpdatedAt is null
        );
    }
}
