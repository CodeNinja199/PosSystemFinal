// One line of an order as the API returns it.
export type OrderItemResponse = {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
};
