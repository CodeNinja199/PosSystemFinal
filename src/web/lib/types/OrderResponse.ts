import type { OrderItemResponse } from "@/lib/types/OrderItemResponse";

// An order as the API returns it.
export type OrderResponse = {
  id: number;
  userId: number;
  placedAt: string;
  status: string;
  paymentMethod: string;
  total: number;
  items: OrderItemResponse[];
};
