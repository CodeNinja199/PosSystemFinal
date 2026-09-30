import type { CartItem } from "@/lib/types/CartItem";
import type { CheckoutAmounts } from "@/lib/types/CheckoutAmounts";

// The browser's copy of the API's pricing, used only to show the GST before the sale is made; the API works every
// figure out again when the order is placed, and that is the one charged.
//
// Money is counted in whole paisa, because adding up prices as fractions of a rupee drifts (0.1 + 0.2 is not 0.3 in
// JavaScript). Half a paisa rounds up, which for amounts that are never negative is the API's rule of rounding halves
// away from zero (requirement 60) - so for the same prices the preview and the receipt always agree. The one way they
// can differ is a price an admin changed after the product went into the cart: the API charges the price at the moment
// of the sale, and the receipt shows that.
export function workOutCheckoutAmounts(
  cartItems: CartItem[],
  gstPercentage: number,
): CheckoutAmounts {
  let subtotalInPaisa = 0;
  for (const item of cartItems) {
    const unitPriceInPaisa = Math.round(item.unitPrice * 100);
    subtotalInPaisa = subtotalInPaisa + unitPriceInPaisa * item.quantity;
  }

  const gstInPaisa = Math.round((subtotalInPaisa * gstPercentage) / 100);
  const totalInPaisa = subtotalInPaisa + gstInPaisa;

  const checkoutAmounts: CheckoutAmounts = {
    subtotal: subtotalInPaisa / 100,
    gst: gstInPaisa / 100,
    total: totalInPaisa / 100,
  };

  return checkoutAmounts;
}
