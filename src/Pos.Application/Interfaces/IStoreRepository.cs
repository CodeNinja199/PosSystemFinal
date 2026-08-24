using Pos.Domain.Entities;

namespace Pos.Application.Interfaces;

// Implemented by StoreRepository in Infrastructure. Used by StoreService for the store list and by AuthService at registration.
public interface IStoreRepository
{
    Task<List<Store>> GetAllStoresAsync();

    Task<Store?> GetStoreByIdAsync(int storeId);
}