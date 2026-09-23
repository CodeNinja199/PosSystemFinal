"use client";

import { useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { addItem } from "@/lib/cartSlice";
import { selectCartItems } from "@/lib/selectCartItems";
import type { AppDispatch } from "@/lib/store";
import type { CartItem } from "@/lib/types/CartItem";
import type { ProductResponse } from "@/lib/types/ProductResponse";

type AddToCartButtonProps = {
  product: ProductResponse;
};

// Used by ProductCard and the product detail page. A shopper picks a quantity with minus and plus and
// adds it in one go, instead of pressing Add to cart once per unit.
//
// The confirmation is not on this button: it is the CartToast in the layout, so the button stays in its
// normal state and a shopper can keep adding without waiting for it to change back.
export function AddToCartButton(props: AddToCartButtonProps) {
  const product = props.product;
  const dispatch = useDispatch<AppDispatch>();
  const cartItems = useSelector(selectCartItems);

  // One to start with, so a shopper can add a product in a single press - but zero for a product with
  // an empty shelf, because the number on screen should never be a quantity that cannot be added.
  let startingQuantity = 1;
  if (product.stockQuantity === 0) {
    startingQuantity = 0;
  }
  const [quantity, setQuantity] = useState(startingQuantity);

  // What is already in the cart counts against the stock, so a shopper cannot add five, then five more,
  // of a product with six on the shelf.
  let quantityAlreadyInCart = 0;
  for (const item of cartItems) {
    if (item.productId === product.id) {
      quantityAlreadyInCart = item.quantity;
    }
  }

  const quantityStillAvailable = product.stockQuantity - quantityAlreadyInCart;

  function handleDecreaseButtonClick() {
    // The floor is zero, and the Add to cart button is disabled there, so nothing can be added twice by
    // accident and no negative quantity can reach the cart.
    if (quantity > 0) {
      setQuantity(quantity - 1);
    }
  }

  function handleIncreaseButtonClick() {
    if (quantity < quantityStillAvailable) {
      setQuantity(quantity + 1);
    }
  }

  function handleAddToCartButtonClick() {
    const cartItem: CartItem = {
      productId: product.id,
      productName: product.name,
      unitPrice: product.price,
      quantity: quantity,
    };
    dispatch(addItem(cartItem));

    // Back to one, ready for the next product - or to zero when that add took the last of the shelf,
    // so the picker never shows a quantity that can no longer be added.
    const stillAvailableAfterAdding = quantityStillAvailable - quantity;
    if (stillAvailableAfterAdding > 0) {
      setQuantity(1);
    } else {
      setQuantity(0);
    }
  }

  const isOutOfStock = product.stockQuantity === 0;
  const hasNoneLeftToAdd = quantityStillAvailable <= 0;
  // Only the floor matters here: a shopper who is holding a number they cannot add must always be
  // able to bring it down again.
  const canDecrease = quantity > 0;
  const canIncrease = quantity < quantityStillAvailable;
  const canAdd = quantity > 0 && quantity <= quantityStillAvailable;

  let helpText = null;
  if (isOutOfStock) {
    helpText = <p className="text-sm text-gray-700">Out of stock</p>;
  } else if (hasNoneLeftToAdd) {
    helpText = (
      <p className="text-sm text-gray-700">
        All {product.stockQuantity} in your cart
      </p>
    );
  }

  return (
    <div className="flex flex-col gap-2">
      <div className="flex items-center gap-2">
        <button
          type="button"
          onClick={handleDecreaseButtonClick}
          disabled={canDecrease === false}
          aria-label={`One less ${product.name}`}
          className="border border-gray-400 px-2 py-1 leading-none disabled:opacity-40"
        >
          &minus;
        </button>
        <span aria-live="polite" className="min-w-8 text-center">
          {quantity}
        </span>
        <button
          type="button"
          onClick={handleIncreaseButtonClick}
          disabled={canIncrease === false}
          aria-label={`One more ${product.name}`}
          className="border border-gray-400 px-2 py-1 leading-none disabled:opacity-40"
        >
          +
        </button>
      </div>
      <button
        type="button"
        onClick={handleAddToCartButtonClick}
        disabled={canAdd === false}
        className="bg-accent px-3 py-1 text-white disabled:opacity-60"
      >
        Add to cart
      </button>
      {helpText}
    </div>
  );
}
