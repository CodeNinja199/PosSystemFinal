"use client";

import { useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { useDispatch, useSelector } from "react-redux";
import { callWebApi } from "@/lib/callWebApi";
import { clearCart } from "@/lib/cartSlice";
import { formatRupees } from "@/lib/formatRupees";
import { selectCartItems } from "@/lib/selectCartItems";
import type { AppDispatch } from "@/lib/store";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { OrderItemRequest } from "@/lib/types/OrderItemRequest";
import type { OrderResponse } from "@/lib/types/OrderResponse";
import type { PlaceOrderRequest } from "@/lib/types/PlaceOrderRequest";
import { workOutCheckoutAmounts } from "@/lib/workOutCheckoutAmounts";

type CheckoutFormProps = {
  isWalkInSale: boolean;
  gstPercentage: number;
};

// Reads the cart from the store, shows the subtotal, the GST, and the total before the sale is made (requirement 62), asks
// for the payment method and, at the counter, the cash handed over, posts to app/api/checkout, and empties the cart on success.
export function CheckoutForm(props: CheckoutFormProps) {
  const isWalkInSale = props.isWalkInSale;
  const gstPercentage = props.gstPercentage;
  const cartItems = useSelector(selectCartItems);
  const dispatch = useDispatch<AppDispatch>();
  const router = useRouter();
  const [paymentMethod, setPaymentMethod] = useState("Cash");
  const [amountTenderedText, setAmountTenderedText] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const amounts = useMemo(
    function workOutAmounts() {
      return workOutCheckoutAmounts(cartItems, gstPercentage);
    },
    [cartItems, gstPercentage],
  );

  const isCashAtTheCounter = isWalkInSale && paymentMethod === "Cash";

  const changeDue = useMemo(
    function workOutChangeDue() {
      if (amountTenderedText === "") {
        return null;
      }

      const amountTendered = Number(amountTenderedText);
      if (Number.isNaN(amountTendered)) {
        return null;
      }

      // The change comes from the total with the GST on it, counted in whole paisa like the total itself.
      const amountTenderedInPaisa = Math.round(amountTendered * 100);
      const totalInPaisa = Math.round(amounts.total * 100);
      return (amountTenderedInPaisa - totalInPaisa) / 100;
    },
    [amountTenderedText, amounts.total],
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

    let amountTendered: number | null = null;
    if (isCashAtTheCounter) {
      amountTendered = Number(amountTenderedText);
    }

    const placeOrderRequest: PlaceOrderRequest = {
      items: orderItems,
      paymentMethod: paymentMethod,
      amountTendered: amountTendered,
    };

    try {
      const response = await callWebApi("/api/checkout", {
        method: "POST",
        body: placeOrderRequest,
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

  function handleAmountTenderedChange(
    event: React.ChangeEvent<HTMLInputElement>,
  ) {
    setAmountTenderedText(event.target.value);
  }

  if (cartItems.length === 0) {
    return <p>Your cart is empty.</p>;
  }

  const lineElements: React.ReactElement[] = [];
  for (const item of cartItems) {
    lineElements.push(
      <li key={item.productId}>
        {item.quantity} x {item.productName} = Rs{" "}
        {formatRupees(item.unitPrice * item.quantity)}
      </li>,
    );
  }

  let buttonText = "Place order";
  if (isSubmitting) {
    buttonText = "Placing order…";
  }
  if (isWalkInSale) {
    buttonText = "Complete sale";
    if (isSubmitting) {
      buttonText = "Completing sale…";
    }
  }

  let amountTenderedElement = null;
  if (isCashAtTheCounter) {
    let changeElement = null;
    if (changeDue !== null) {
      if (changeDue >= 0) {
        changeElement = (
          <p className="font-bold">Change due: Rs {formatRupees(changeDue)}</p>
        );
      } else {
        changeElement = (
          <p className="text-red-700">
            That is Rs {formatRupees(Math.abs(changeDue))} short of the total.
          </p>
        );
      }
    }

    amountTenderedElement = (
      <div className="flex flex-col gap-1">
        <label htmlFor="amountTendered">Amount tendered (Rs)</label>
        <input
          id="amountTendered"
          type="number"
          min="0.01"
          step="0.01"
          value={amountTenderedText}
          onChange={handleAmountTenderedChange}
          required
          className="w-40 border border-gray-400 px-2 py-1"
        />
        {changeElement}
      </div>
    );
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
      <div className="flex flex-col gap-1">
        <p>Subtotal: Rs {formatRupees(amounts.subtotal)}</p>
        <p>
          GST ({gstPercentage}%): Rs {formatRupees(amounts.gst)}
        </p>
        <p className="font-bold">Total: Rs {formatRupees(amounts.total)}</p>
      </div>
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
      {amountTenderedElement}
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
