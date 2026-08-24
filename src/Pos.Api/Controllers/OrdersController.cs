using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Pos.Application.Dtos;
using Pos.Application.Services;
using Pos.Domain.Enums;

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

    [Authorize]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyOrders()
    {
        int currentUserId = GetCurrentUserId();

        List<OrderResponse> myOrders = await _orderService.GetOrdersForUserAsync(currentUserId);

        return Ok(myOrders);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        int currentUserId = GetCurrentUserId();
        UserRole currentUserRole = GetCurrentUserRole();

        OrderResponse order = await _orderService.GetOrderByIdAsync(id, currentUserId, currentUserRole);

        return Ok(order);
    }

    [Authorize(Roles = "Admin,Cashier")]
    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        List<OrderResponse> orders = await _orderService.GetAllOrdersAsync();

        return Ok(orders);
    }

    [Authorize(Roles = "Admin,Cashier")]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusRequest updateOrderStatusRequest)
    {
        OrderResponse updatedOrder = await _orderService.UpdateOrderStatusAsync(id, updateOrderStatusRequest);

        return Ok(updatedOrder);
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

    // The role claim was put in the token by JwtLoginTokenCreator; it is the same value [Authorize(Roles = ...)] reads.
    private UserRole GetCurrentUserRole()
    {
        Claim? roleClaim = User.FindFirst(ClaimTypes.Role);
        if (roleClaim == null)
        {
            throw new InvalidOperationException("The token has no role claim.");
        }

        UserRole currentUserRole = Enum.Parse<UserRole>(roleClaim.Value);

        return currentUserRole;
    }
}