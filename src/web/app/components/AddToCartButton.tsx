"use client";

import { useDispatch } from "react-redux";
import { addItem } from "@/lib/cartSlice";
import type { AppDispatch } from "@/lib/store";
import type { CartItem } from "@/lib/types/CartItem";
import type { ProductResponse } from "@/lib/types/ProductResponse";

type AddToCartButtonProps = {
  product: ProductResponse;
};

// Used by ProductCard and the product detail page. Dispatches one line of the product to the cart slice.
export function AddToCartButton(props: AddToCartButtonProps) {
  const product = props.product;
  const dispatch = useDispatch<AppDispatch>();

  function handleAddToCartButtonClick() {
    const cartItem: CartItem = {
      productId: product.id,
      productName: product.name,
      unitPrice: product.price,
      quantity: 1,
    };
    dispatch(addItem(cartItem));
  }

  const isOutOfStock = product.stockQuantity === 0;

  return (
    <button
      type="button"
      onClick={handleAddToCartButtonClick}
      disabled={isOutOfStock}
      className="bg-accent px-3 py-1 text-white disabled:opacity-60"
    >
      Add to cart
    </button>
  );
}
