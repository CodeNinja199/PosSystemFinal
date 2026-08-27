"use client";

import Link from "next/link";
import { useMemo } from "react";
import { useDispatch, useSelector } from "react-redux";
import { removeItem } from "@/lib/cartSlice";
import { selectCartItems } from "@/lib/selectCartItems";
import type { AppDispatch } from "@/lib/store";
import type { CartItem } from "@/lib/types/CartItem";

// The interactive part of the cart page: reads the cart from the store, removes lines, shows the total.
export function CartTable() {
  const cartItems = useSelector(selectCartItems);
  const dispatch = useDispatch<AppDispatch>();

  const cartTotal = useMemo(
    function addUpCartTotal() {
      let total = 0;
      for (const item of cartItems) {
        total = total + item.unitPrice * item.quantity;
      }

      return total;
    },
    [cartItems],
  );

  if (cartItems.length === 0) {
    return <p>Your cart is empty.</p>;
  }

  const rowElements: React.ReactElement[] = [];
  for (const item of cartItems) {
    rowElements.push(
      <CartRow key={item.productId} item={item} dispatch={dispatch} />,
    );
  }

  return (
    <div className="flex flex-col gap-4">
      <table className="w-full border-collapse">
        <thead>
          <tr className="border-b border-gray-300 text-left">
            <th className="py-2">Product</th>
            <th className="py-2">Unit price</th>
            <th className="py-2">Quantity</th>
            <th className="py-2">Line total</th>
            <th className="py-2"></th>
          </tr>
        </thead>
        <tbody>{rowElements}</tbody>
      </table>
      <p className="text-lg font-bold">Total: Rs {cartTotal}</p>
      <Link href="/checkout" className="w-fit bg-accent px-4 py-2 text-white">
        Go to checkout
      </Link>
    </div>
  );
}

type CartRowProps = {
  item: CartItem;
  dispatch: AppDispatch;
};

function CartRow(props: CartRowProps) {
  const item = props.item;
  const dispatch = props.dispatch;

  function handleRemoveButtonClick() {
    dispatch(removeItem(item.productId));
  }

  return (
    <tr className="border-b border-gray-200">
      <td className="py-2">{item.productName}</td>
      <td className="py-2">Rs {item.unitPrice}</td>
      <td className="py-2">{item.quantity}</td>
      <td className="py-2">Rs {item.unitPrice * item.quantity}</td>
      <td className="py-2">
        <button
          type="button"
          onClick={handleRemoveButtonClick}
          className="border border-gray-400 px-2 py-1 hover:bg-gray-100"
        >
          Remove
        </button>
      </td>
    </tr>
  );
}
