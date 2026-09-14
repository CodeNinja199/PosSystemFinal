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
    private readonly IUserRepository _userRepository;

    public OrderService(IOrderRepository orderRepository, IProductRepository productRepository, ILogger<OrderService> logger, INotificationMessagePublisher notificationMessagePublisher, IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _logger = logger;
        _notificationMessagePublisher = notificationMessagePublisher;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    // Order of work: check and price every line -> check the cash covers the total -> reduce stock -> one save. Nothing is written before every check passes.
    public async Task<OrderResponse> PlaceOrderAsync(PlaceOrderRequest placeOrderRequest, int currentUserId, UserRole currentUserRole, int storeId)
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

        bool isCardPayment = placeOrderRequest.PaymentMethod.Value == PaymentMethod.Card;
        if (isCardPayment && placeOrderRequest.AmountTendered != null)
        {
            throw new ValidationException("An amount tendered only applies to a cash payment.");
        }

        // A walk-in sale is paid at the counter, so it is complete the moment it is rung up. A customer's order waits for the store.
        bool isWalkInSale = currentUserRole == UserRole.Cashier || currentUserRole == UserRole.Admin;
        OrderStatus startingStatus = OrderStatus.Placed;
        if (isWalkInSale)
        {
            startingStatus = OrderStatus.Completed;
        }

        // The cashier is holding the money, so the receipt must say how much was handed over and what went back.
        bool isCashPayment = placeOrderRequest.PaymentMethod.Value == PaymentMethod.Cash;
        if (isWalkInSale && isCashPayment && placeOrderRequest.AmountTendered == null)
        {
            throw new ValidationException("A cash sale at the counter needs the amount tendered.");
        }

        Order newOrder = new Order
        {
            StoreId = storeId,
            UserId = currentUserId,
            PlacedAt = DateTime.UtcNow,
            Status = startingStatus,
            PaymentMethod = placeOrderRequest.PaymentMethod.Value,
            AmountTendered = placeOrderRequest.AmountTendered,
            Total = 0
        };

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
        }

        // The total is only known once every line is priced, and stock must not move for a sale the cash does not cover.
        if (placeOrderRequest.AmountTendered != null)
        {
            bool coversTheTotal = placeOrderRequest.AmountTendered.Value >= newOrder.Total;
            if (coversTheTotal == false)
            {
                throw new ValidationException($"Amount tendered Rs {placeOrderRequest.AmountTendered.Value} is less than the total Rs {newOrder.Total}.");
            }
        }

        List<Product> productsBelowLowStockThreshold = new List<Product>();
        foreach (OrderItem orderItem in newOrder.Items)
        {
            Product productForItem = productsById[orderItem.ProductId];
            productForItem.StockQuantity = productForItem.StockQuantity - orderItem.Quantity;

            bool isBelowLowStockThreshold = productForItem.StockQuantity <= productForItem.LowStockThreshold;
            if (isBelowLowStockThreshold)
            {
                productsBelowLowStockThreshold.Add(productForItem);
            }
        }

        await _orderRepository.SaveNewOrderAsync(newOrder);

        _logger.LogInformation("Order {OrderId} placed by user {UserId} in store {StoreId} for {Total}", newOrder.Id, currentUserId, storeId, newOrder.Total);

        // Only after the awaited save: the message must never describe an order that was not written.
        // The buyer of a walk-in sale has no account, so there is nobody to send it to.
        if (isWalkInSale == false)
        {
            NotificationMessage orderPlacedMessage = new NotificationMessage
            {
                Type = NotificationMessageTypes.OrderPlaced,
                RecipientUserId = currentUserId,
                Message = $"Order {newOrder.Id} placed. Total Rs {newOrder.Total}."
            };
            await _notificationMessagePublisher.PublishAsync(orderPlacedMessage);
        }

        // Only after the save is confirmed: a warning for a stock level that was never saved would be a lie.
        if (productsBelowLowStockThreshold.Count > 0)
        {
            List<User> storeAdmins = await _userRepository.GetAdminsAsync(storeId);
            foreach (Product productBelowThreshold in productsBelowLowStockThreshold)
            {
                _logger.LogWarning("Product {ProductName} in store {StoreId} is low on stock: {StockQuantity} left", productBelowThreshold.Name, storeId, productBelowThreshold.StockQuantity);

                foreach (User storeAdmin in storeAdmins)
                {
                    NotificationMessage stockLowMessage = new NotificationMessage
                    {
                        Type = NotificationMessageTypes.StockLow,
                        RecipientUserId = storeAdmin.Id,
                        Message = $"{productBelowThreshold.Name} is low on stock: {productBelowThreshold.StockQuantity} left (threshold {productBelowThreshold.LowStockThreshold})."
                    };
                    await _notificationMessagePublisher.PublishAsync(stockLowMessage);
                }
            }
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

        // The change is worked out from two values on the same row, so a receipt can never drift from what was saved.
        decimal? changeDue = null;
        if (order.AmountTendered != null)
        {
            changeDue = order.AmountTendered.Value - order.Total;
        }

        OrderResponse orderResponse = new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            PlacedAt = DateTime.SpecifyKind(order.PlacedAt, DateTimeKind.Utc),
            Status = order.Status.ToString(),
            PaymentMethod = order.PaymentMethod.ToString(),
            Total = order.Total,
            AmountTendered = order.AmountTendered,
            ChangeDue = changeDue,
            Items = itemResponses
        };

        return orderResponse;
    }
}