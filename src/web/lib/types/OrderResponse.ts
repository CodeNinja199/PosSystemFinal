import type { OrderItemResponse } from "@/lib/types/OrderItemResponse";

// An order as the API returns it.
export type OrderResponse = {
  id: number;
  userId: number;
  placedAt: string;
  status: string;
  paymentMethod: string;
  subtotal: number;
  gstPercentage: number;
  gstAmount: number;
  total: number;
  amountTendered: number | null;
  changeDue: number | null;
  items: OrderItemResponse[];
};
