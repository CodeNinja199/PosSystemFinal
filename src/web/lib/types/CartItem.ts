// One line of the cart, kept in the browser only. The API recomputes the price at checkout from the product id.
export type CartItem = {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
};
