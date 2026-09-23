// Set by the cart slice every time something is added, and read by CartToast so it can show a short
// "Added to cart" message. The counter is what makes adding the same product twice show the message twice:
// the product name and quantity on their own could be identical, and nothing would appear to have changed.
export type AddedToCartNotice = {
  productName: string;
  quantity: number;
  noticeNumber: number;
};
