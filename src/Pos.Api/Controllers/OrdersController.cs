using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Application.Dtos;
using Pos.Application.Services;

namespace Pos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PlaceOrder(PlaceOrderRequest placeOrderRequest)
    {
        int currentUserId = GetCurrentUserId();

        OrderResponse placedOrder = await _orderService.PlaceOrderAsync(placeOrderRequest, currentUserId);

        return StatusCode(StatusCodes.Status201Created, placedOrder);
    }

    // The user id claim was put in the token by JwtLoginTokenCreator and read back by the JWT bearer middleware.
    private int GetCurrentUserId()
    {
        Claim? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            throw new InvalidOperationException("The token has no user id claim.");
        }

        int currentUserId = int.Parse(userIdClaim.Value);

        return currentUserId;
    }
}