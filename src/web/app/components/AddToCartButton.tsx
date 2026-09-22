"use client";

import { useEffect, useState } from "react";
import { useDispatch } from "react-redux";
import { addItem } from "@/lib/cartSlice";
import type { AppDispatch } from "@/lib/store";
import type { CartItem } from "@/lib/types/CartItem";
import type { ProductResponse } from "@/lib/types/ProductResponse";

type AddToCartButtonProps = {
  product: ProductResponse;
};

// How long the "Added to cart" confirmation stays on the button before it offers to add again.
const ConfirmationMilliseconds = 1800;

// Used by ProductCard and the product detail page. Dispatches one line of the product to the cart slice.
// The cart lives in the browser, so nothing is sent to the server and there is no response to wait for:
// without a confirmation on the button a shopper cannot tell the click worked without opening the cart.
export function AddToCartButton(props: AddToCartButtonProps) {
  const product = props.product;
  const dispatch = useDispatch<AppDispatch>();
  const [wasJustAdded, setWasJustAdded] = useState(false);

  // Clears the confirmation after a moment. The timer is cancelled if the shopper leaves the page
  // first, so nothing tries to set state on a component that is no longer on screen.
  useEffect(
    function clearConfirmationAfterAMoment() {
      if (wasJustAdded === false) {
        return;
      }

      const timer = setTimeout(function stopShowingConfirmation() {
        setWasJustAdded(false);
      }, ConfirmationMilliseconds);

      return function cancelTimer() {
        clearTimeout(timer);
      };
    },
    [wasJustAdded],
  );

  function handleAddToCartButtonClick() {
    const cartItem: CartItem = {
      productId: product.id,
      productName: product.name,
      unitPrice: product.price,
      quantity: 1,
    };
    dispatch(addItem(cartItem));
    setWasJustAdded(true);
  }

  const isOutOfStock = product.stockQuantity === 0;

  // The tick carries the message on its own. The accent is already green, so a second green
  // would be too close to read at a glance; the confirmation goes dark instead.
  let buttonText = "Add to cart";
  if (wasJustAdded) {
    buttonText = "✓ Added to cart";
  }

  let buttonClassName =
    "min-w-36 px-3 py-1 text-white disabled:opacity-60 bg-accent";
  if (wasJustAdded) {
    buttonClassName = "min-w-36 px-3 py-1 text-white disabled:opacity-60 bg-gray-800";
  }

  return (
    <button
      type="button"
      onClick={handleAddToCartButtonClick}
      disabled={isOutOfStock}
      className={buttonClassName}
    >
      {/* aria-live so a screen reader announces the change, which a colour swap alone would not do. */}
      <span aria-live="polite">{buttonText}</span>
    </button>
  );
}
