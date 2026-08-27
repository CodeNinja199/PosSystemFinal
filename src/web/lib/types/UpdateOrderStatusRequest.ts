// What the status buttons post to app/api/orders/[id]/status, and what is forwarded to PATCH api/orders/{id}/status.
export type UpdateOrderStatusRequest = {
  status: string;
};
