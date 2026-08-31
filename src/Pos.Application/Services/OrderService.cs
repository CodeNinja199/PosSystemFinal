using Microsoft.Extensions.Logging;

using Pos.Application.Dtos;
using Pos.Application.Exceptions;
using Pos.Application.Interfaces;
using Pos.Application.Messaging;
using Pos.Domain.Entities;
using Pos.Domain.Enums;

namespace Pos.Application.Services;

// Called by OrdersController after the [Authorize] check passes. Holds the checkout and order rules.
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<OrderService> _logger;
    private readonly INotificationMessagePublisher _notificationMessagePublisher;

    public OrderService(IOrderRepository orderRepository, IProductRepository productRepository, ILogger<OrderService> logger, INotificationMessagePublisher notificationMessagePublisher)
    {
        _logger = logger;
        _notificationMessagePublisher = notificationMessagePublisher;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    // Order of work: check every line -> build the order and reduce stock -> one save. Nothing is written before every check passes.
    public async Task<OrderResponse> PlaceOrderAsync(PlaceOrderRequest placeOrderRequest, int currentUserId, int storeId)
    {
        HashSet<int> productIdsSeen = new HashSet<int>();
        foreach (OrderItemRequest orderItemRequest in placeOrderRequest.Items)
        {
            bool isFirstTimeSeen = productIdsSeen.Add(orderItemRequest.ProductId);
            if (isFirstTimeSeen == false)
            {
                throw new ValidationException($"Product {orderItemRequest.ProductId} is listed more than once.");
            }
        }

        List<int> requestedProductIds = new List<int>(productIdsSeen);
        List<Product> productsFromRepository = await _productRepository.GetProductsByIdsAsync(requestedProductIds, storeId);

        Dictionary<int, Product> productsById = new Dictionary<int, Product>();
        foreach (Product product in productsFromRepository)
        {
            productsById.Add(product.Id, product);
        }

        if (placeOrderRequest.PaymentMethod == null)
        {
            throw new ValidationException("A payment method is required.");
        }

        Order newOrder = new Order
        {
            StoreId = storeId,
            UserId = currentUserId,
            PlacedAt = DateTime.UtcNow,
            Status = OrderStatus.Placed,
            PaymentMethod = placeOrderRequest.PaymentMethod.Value,
            Total = 0
        };

        List<Product> productsBelowLowStockThreshold = new List<Product>();

        foreach (OrderItemRequest orderItemRequest in placeOrderRequest.Items)
        {
            bool doesProductExist = productsById.ContainsKey(orderItemRequest.ProductId);
            if (doesProductExist == false)
            {
                throw new NotFoundException($"Product {orderItemRequest.ProductId} was not found.");
            }

            Product productForItem = productsById[orderItemRequest.ProductId];
            bool hasEnoughStock = productForItem.StockQuantity >= orderItemRequest.Quantity;
            if (hasEnoughStock == false)
            {
                throw new ConflictException($"Not enough stock of {productForItem.Name}: {productForItem.StockQuantity} left.");
            }

            OrderItem orderItem = new OrderItem
            {
                ProductId = productForItem.Id,
                ProductName = productForItem.Name,
                UnitPrice = productForItem.Price,
                Quantity = orderItemRequest.Quantity
            };
            newOrder.Items.Add(orderItem);
            newOrder.Total = newOrder.Total + productForItem.Price * orderItemRequest.Quantity;

            productForItem.StockQuantity = productForItem.StockQuantity - orderItemRequest.Quantity;

            bool isBelowLowStockThreshold = productForItem.StockQuantity <= productForItem.LowStockThreshold;
            if (isBelowLowStockThreshold)
            {
                productsBelowLowStockThreshold.Add(productForItem);
            }
        }

        await _orderRepository.SaveNewOrderAsync(newOrder);

        _logger.LogInformation("Order {OrderId} placed by user {UserId} in store {StoreId} for {Total}", newOrder.Id, currentUserId, storeId, newOrder.Total);

        // Only after the save is confirmed: a warning for a stock level that was never saved would be a lie.
        foreach (Product productBelowThreshold in productsBelowLowStockThreshold)
        {
            _logger.LogWarning("Product {ProductName} in store {StoreId} is low on stock: {StockQuantity} left", productBelowThreshold.Name, storeId, productBelowThreshold.StockQuantity);
        }

        OrderResponse orderResponse = MapOrderToResponse(newOrder);

        return orderResponse;
    }

    public async Task<List<OrderResponse>> GetOrdersForUserAsync(int currentUserId)
    {
        List<Order> ordersFromRepository = await _orderRepository.GetOrdersForUserAsync(currentUserId);

        List<OrderResponse> orderResponses = new List<OrderResponse>();
        foreach (Order order in ordersFromRepository)
        {
            OrderResponse orderResponse = MapOrderToResponse(order);
            orderResponses.Add(orderResponse);
        }

        return orderResponses;
    }

    // A customer may only open their own orders; a cashier or admin may open any order of the store.
    public async Task<OrderResponse> GetOrderByIdAsync(int orderId, int currentUserId, UserRole currentUserRole, int storeId)
    {
        Order? orderFromRepository = await _orderRepository.GetOrderByIdAsync(orderId, storeId);
        if (orderFromRepository == null)
        {
            throw new NotFoundException($"Order {orderId} was not found.");
        }

        bool isOwnOrder = orderFromRepository.UserId == currentUserId;
        bool isStaff = currentUserRole == UserRole.Cashier || currentUserRole == UserRole.Admin;
        if (isOwnOrder == false && isStaff == false)
        {
            throw new ForbiddenException("This order belongs to another customer.");
        }

        OrderResponse orderResponse = MapOrderToResponse(orderFromRepository);

        return orderResponse;
    }

    public async Task<List<OrderResponse>> GetAllOrdersAsync(int storeId)
    {
        List<Order> ordersFromRepository = await _orderRepository.GetAllOrdersAsync(storeId);

        List<OrderResponse> orderResponses = new List<OrderResponse>();
        foreach (Order order in ordersFromRepository)
        {
            OrderResponse orderResponse = MapOrderToResponse(order);
            orderResponses.Add(orderResponse);
        }

        return orderResponses;
    }

    // An order moves from Placed to Completed or Cancelled once, and never changes again.
    public async Task<OrderResponse> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest updateOrderStatusRequest, int storeId)
    {
        if (updateOrderStatusRequest.Status == null)
        {
            throw new ValidationException("A status is required.");
        }

        OrderStatus newStatus = updateOrderStatusRequest.Status.Value;
        bool isAllowedStatus = newStatus == OrderStatus.Completed || newStatus == OrderStatus.Cancelled;
        if (isAllowedStatus == false)
        {
            throw new ValidationException("Status must be Completed or Cancelled.");
        }

        Order? orderFromRepository = await _orderRepository.GetOrderByIdAsync(orderId, storeId);
        if (orderFromRepository == null)
        {
            throw new NotFoundException($"Order {orderId} was not found.");
        }

        if (orderFromRepository.Status != OrderStatus.Placed)
        {
            throw new ConflictException($"Order {orderId} is already {orderFromRepository.Status}.");
        }

        orderFromRepository.Status = newStatus;
        await _orderRepository.SaveOrderAsync(orderFromRepository);

        OrderResponse orderResponse = MapOrderToResponse(orderFromRepository);

        return orderResponse;
    }

    private static OrderResponse MapOrderToResponse(Order order)
    {
        List<OrderItemResponse> itemResponses = new List<OrderItemResponse>();
        foreach (OrderItem orderItem in order.Items)
        {
            OrderItemResponse itemResponse = new OrderItemResponse
            {
                ProductId = orderItem.ProductId,
                ProductName = orderItem.ProductName,
                UnitPrice = orderItem.UnitPrice,
                Quantity = orderItem.Quantity,
                LineTotal = orderItem.UnitPrice * orderItem.Quantity
            };
            itemResponses.Add(itemResponse);
        }

        OrderResponse orderResponse = new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            PlacedAt = DateTime.SpecifyKind(order.PlacedAt, DateTimeKind.Utc),
            Status = order.Status.ToString(),
            PaymentMethod = order.PaymentMethod.ToString(),
            Total = order.Total,
            Items = itemResponses
        };

        return orderResponse;
    }
}