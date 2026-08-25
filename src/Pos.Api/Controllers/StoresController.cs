using Microsoft.AspNetCore.Mvc;

using Pos.Application.Dtos;
using Pos.Application.Services;

namespace Pos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoresController : ControllerBase
{
    private readonly StoreService _storeService;

    public StoresController(StoreService storeService)
    {
        _storeService = storeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetStores()
    {
        List<StoreResponse> stores = await _storeService.GetStoresAsync();

        return Ok(stores);
    }
}