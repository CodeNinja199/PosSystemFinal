using Pos.Application.Dtos;
using Pos.Application.Interfaces;
using Pos.Domain.Entities;

namespace Pos.Application.Services;

// Called by StoresController. The store list is public because registration needs it before there is a token.
public class StoreService
{
    private readonly IStoreRepository _storeRepository;

    public StoreService(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    public async Task<List<StoreResponse>> GetStoresAsync()
    {
        List<Store> storesFromRepository = await _storeRepository.GetAllStoresAsync();

        List<StoreResponse> storeResponses = new List<StoreResponse>();
        foreach (Store store in storesFromRepository)
        {
            StoreResponse storeResponse = new StoreResponse
            {
                Id = store.Id,
                Name = store.Name
            };
            storeResponses.Add(storeResponse);
        }

        return storeResponses;
    }
}