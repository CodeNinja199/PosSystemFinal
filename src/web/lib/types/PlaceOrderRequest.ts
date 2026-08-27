import type { OrderItemRequest } from "@/lib/types/OrderItemRequest";

// What the checkout form posts to app/api/checkout, and what the handler forwards to POST api/orders.
export type PlaceOrderRequest = {
  items: OrderItemRequest[];
  paymentMethod: string;
};
