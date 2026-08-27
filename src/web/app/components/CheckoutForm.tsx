"use client";

import { useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { useDispatch, useSelector } from "react-redux";
import { clearCart } from "@/lib/cartSlice";
import { selectCartItems } from "@/lib/selectCartItems";
import type { AppDispatch } from "@/lib/store";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { OrderItemRequest } from "@/lib/types/OrderItemRequest";
import type { OrderResponse } from "@/lib/types/OrderResponse";
import type { PlaceOrderRequest } from "@/lib/types/PlaceOrderRequest";

// Reads the cart from the store, asks for the payment method, posts to app/api/checkout, and empties the cart on success.
export function CheckoutForm() {
  const cartItems = useSelector(selectCartItems);
  const dispatch = useDispatch<AppDispatch>();
  const router = useRouter();
  const [paymentMethod, setPaymentMethod] = useState("Cash");
  const [errorMessage, setErrorMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

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

  async function handleCheckoutFormSubmit(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();
    setErrorMessage("");
    setIsSubmitting(true);

    const orderItems: OrderItemRequest[] = [];
    for (const item of cartItems) {
      orderItems.push({ productId: item.productId, quantity: item.quantity });
    }
    const placeOrderRequest: PlaceOrderRequest = {
      items: orderItems,
      paymentMethod: paymentMethod,
    };

    try {
      const response = await fetch("/api/checkout", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(placeOrderRequest),
      });

      if (response.ok === false) {
        const errorBody: ApiErrorResponse = await response.json();
        setErrorMessage(errorBody.message);
        setIsSubmitting(false);
        return;
      }

      const placedOrder: OrderResponse = await response.json();
      dispatch(clearCart());
      router.push(`/orders/${placedOrder.id}`);
    } catch {
      setErrorMessage("The server could not be reached.");
      setIsSubmitting(false);
    }
  }

  function handlePaymentMethodChange(
    event: React.ChangeEvent<HTMLInputElement>,
  ) {
    setPaymentMethod(event.target.value);
  }

  if (cartItems.length === 0) {
    return <p>Your cart is empty.</p>;
  }

  const lineElements: React.ReactElement[] = [];
  for (const item of cartItems) {
    lineElements.push(
      <li key={item.productId}>
        {item.quantity} x {item.productName} = Rs{" "}
        {item.unitPrice * item.quantity}
      </li>,
    );
  }

  let buttonText = "Place order";
  if (isSubmitting) {
    buttonText = "Placing order…";
  }

  let errorElement = null;
  if (errorMessage !== "") {
    errorElement = <p className="text-red-700">{errorMessage}</p>;
  }

  return (
    <form
      onSubmit={handleCheckoutFormSubmit}
      className="flex max-w-md flex-col gap-4"
    >
      <ul className="list-disc pl-5">{lineElements}</ul>
      <p className="font-bold">Total: Rs {cartTotal}</p>
      <fieldset className="flex flex-col gap-1">
        <legend className="mb-1">Payment method</legend>
        <label htmlFor="cash">
          <input
            id="cash"
            type="radio"
            name="paymentMethod"
            value="Cash"
            checked={paymentMethod === "Cash"}
            onChange={handlePaymentMethodChange}
          />{" "}
          Cash
        </label>
        <label htmlFor="card">
          <input
            id="card"
            type="radio"
            name="paymentMethod"
            value="Card"
            checked={paymentMethod === "Card"}
            onChange={handlePaymentMethodChange}
          />{" "}
          Card
        </label>
      </fieldset>
      {errorElement}
      <button
        type="submit"
        disabled={isSubmitting}
        className="w-fit bg-accent px-4 py-2 text-white disabled:opacity-60"
      >
        {buttonText}
      </button>
    </form>
  );
}
