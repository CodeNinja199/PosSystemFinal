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
        int currentUserId = CurrentUserClaims.GetUserId(User);
        int storeId = CurrentUserClaims.GetStoreId(User);

        OrderResponse placedOrder = await _orderService.PlaceOrderAsync(placeOrderRequest, currentUserId, storeId);

        return StatusCode(StatusCodes.Status201Created, placedOrder);
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMyOrders()
    {
        int currentUserId = CurrentUserClaims.GetUserId(User);

        List<OrderResponse> myOrders = await _orderService.GetOrdersForUserAsync(currentUserId);

        return Ok(myOrders);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        int currentUserId = CurrentUserClaims.GetUserId(User);
        UserRole currentUserRole = CurrentUserClaims.GetRole(User);
        int storeId = CurrentUserClaims.GetStoreId(User);

        OrderResponse order = await _orderService.GetOrderByIdAsync(id, currentUserId, currentUserRole, storeId);

        return Ok(order);
    }

    [Authorize(Roles = "Admin,Cashier")]
    [HttpGet]
    public async Task<IActionResult> GetAllOrders()
    {
        int storeId = CurrentUserClaims.GetStoreId(User);

        List<OrderResponse> orders = await _orderService.GetAllOrdersAsync(storeId);

        return Ok(orders);
    }

    [Authorize(Roles = "Admin,Cashier")]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusRequest updateOrderStatusRequest)
    {
        int storeId = CurrentUserClaims.GetStoreId(User);

        OrderResponse updatedOrder = await _orderService.UpdateOrderStatusAsync(id, updateOrderStatusRequest, storeId);

        return Ok(updatedOrder);
    }
}