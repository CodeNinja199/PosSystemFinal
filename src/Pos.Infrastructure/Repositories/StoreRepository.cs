using Microsoft.EntityFrameworkCore;

using Pos.Application.Interfaces;
using Pos.Domain.Entities;
using Pos.Infrastructure.Data;

namespace Pos.Infrastructure.Repositories;

// Registered as scoped in Program.cs because it holds the scoped PosDbContext.
public class StoreRepository : IStoreRepository
{
    private readonly PosDbContext _context;

    public StoreRepository(PosDbContext context)
    {
        _context = context;
    }

    public async Task<List<Store>> GetAllStoresAsync()
    {
        List<Store> allStores = await _context.Stores
            .OrderBy(store => store.Name)
            .ToListAsync();

        return allStores;
    }

    public async Task<Store?> GetStoreByIdAsync(int storeId)
    {
        Store? storeFromDatabase = await _context.Stores
            .FirstOrDefaultAsync(store => store.Id == storeId);

        return storeFromDatabase;
    }
}