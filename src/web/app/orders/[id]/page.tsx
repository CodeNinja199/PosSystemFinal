"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { callWebApi } from "@/lib/callWebApi";
import { formatRupees } from "@/lib/formatRupees";
import { useRequireLogin } from "@/lib/useRequireLogin";
import type { OrderResponse } from "@/lib/types/OrderResponse";

// The receipt: one order with its lines, in a single column. A customer may only open their own; staff may open any order of the store.
//
// A client component because the token lives in localStorage, which only the browser can read. It reads through
// callWebApi rather than readFromApi because the two refusals below are told apart by their status code, and the
// error readFromApi throws does not carry one.
export default function OrderReceiptPage() {
  const parameters = useParams<{ id: string }>();
  const orderId = parameters.id;
  const { isReady } = useRequireLogin(null);
  const [order, setOrder] = useState<OrderResponse | null>(null);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(
    function loadTheOrder() {
      if (isReady === false) {
        return;
      }

      async function readTheOrder() {
        try {
          const orderResponse = await callWebApi(
            `/api/gateway/pos/orders/${orderId}`,
            { method: "GET", body: null },
          );

          // notFound() renders Next's 404 page but may only be called on the server, so a missing
          // order is a message here like the other refusal.
          if (orderResponse.status === 404) {
            setErrorMessage("That order could not be found.");
            return;
          }
          if (orderResponse.status === 403) {
            setErrorMessage("This order belongs to another customer.");
            return;
          }
          if (orderResponse.ok === false) {
            setErrorMessage("The order could not be loaded.");
            return;
          }

          const loadedOrder: OrderResponse = await orderResponse.json();
          setOrder(loadedOrder);
        } catch {
          setErrorMessage("The order could not be loaded.");
        }
      }

      readTheOrder();
    },
    [isReady, orderId],
  );

  // Nothing of this page is drawn once the stored login has gone: the redirect from useRequireLogin
  // is already on its way, and what was fetched belonged to whoever was signed in a moment ago.
  if (isReady === false) {
    return <p>Loading…</p>;
  }

  if (errorMessage !== "") {
    return <p>{errorMessage}</p>;
  }

  if (order === null) {
    return <p>Loading…</p>;
  }

  const placedAt = new Date(order.placedAt);

  const lineElements: React.ReactElement[] = [];
  for (const item of order.items) {
    lineElements.push(
      <tr key={item.productId} className="border-b border-gray-200">
        <td className="py-2">{item.productName}</td>
        <td className="py-2">{item.quantity}</td>
        <td className="py-2">Rs {formatRupees(item.unitPrice)}</td>
        <td className="py-2">Rs {formatRupees(item.lineTotal)}</td>
      </tr>,
    );
  }

  let cashElements = null;
  if (order.amountTendered !== null && order.changeDue !== null) {
    cashElements = (
      <div>
        <p>Amount tendered: Rs {formatRupees(order.amountTendered)}</p>
        <p>Change: Rs {formatRupees(order.changeDue)}</p>
      </div>
    );
  }

  // An order placed before GST existed carries none (requirement 61), so its receipt stays exactly as it was printed.
  let gstElements = null;
  if (order.gstPercentage > 0) {
    gstElements = (
      <>
        <p>Subtotal: Rs {formatRupees(order.subtotal)}</p>
        <p>
          GST ({order.gstPercentage}%): Rs {formatRupees(order.gstAmount)}
        </p>
      </>
    );
  }

  return (
    <div className="max-w-xl">
      <h1 className="mb-1 text-2xl font-bold">Order {order.id}</h1>
      <p>Placed {placedAt.toLocaleString()}</p>
      <p>Status: {order.status}</p>
      <p>Payment: {order.paymentMethod}</p>
      {cashElements}
      <table className="mt-4 w-full border-collapse">
        <thead>
          <tr className="border-b border-gray-300 text-left">
            <th className="py-2">Product</th>
            <th className="py-2">Quantity</th>
            <th className="py-2">Unit price</th>
            <th className="py-2">Line total</th>
          </tr>
        </thead>
        <tbody>{lineElements}</tbody>
      </table>
      <div className="mt-4 flex flex-col gap-1">
        {gstElements}
        <p className="text-lg font-bold">
          Total: Rs {formatRupees(order.total)}
        </p>
      </div>
    </div>
  );
}
