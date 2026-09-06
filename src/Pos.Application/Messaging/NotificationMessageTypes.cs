namespace Pos.Application.Messaging;

// The two message types the POS API sends. The Notification API stores whatever arrives, without knowing who sent it.
public static class NotificationMessageTypes
{
    public const string OrderPlaced = "order.placed";

    public const string StockLow = "stock.low";
}