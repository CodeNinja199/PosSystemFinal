"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { readFromApi } from "@/lib/callWebApi";
import { useRequireLogin } from "@/lib/useRequireLogin";
import type { OrderResponse } from "@/lib/types/OrderResponse";
import { OrderStatusButtons } from "@/app/components/OrderStatusButtons";

// The store's orders for cashiers and admins. The API answers 403 to anyone else even if they reach the page.
//
// A client component because the token lives in localStorage, which only the browser can read.
export default function StoreOrdersPage() {
  const { isReady } = useRequireLogin(["Cashier", "Admin"]);
  const [orders, setOrders] = useState<OrderResponse[] | null>(null);
  const [errorMessage, setErrorMessage] = useState("");

  // Counted up by handleStatusChanged below. A completed or cancelled order used to be shown by
  // asking Next to render the page on the server again; the fetch now happens here in the browser,
  // so a change has to ask for that fetch to run again instead.
  const [reloadCount, setReloadCount] = useState(0);

  useEffect(
    function loadOrders() {
      if (isReady === false) {
        return;
      }

      readFromApi<OrderResponse[]>("/pos/orders")
        .then(function showThem(loadedOrders) {
          setOrders(loadedOrders);
        })
        .catch(function showTheProblem() {
          setErrorMessage("The orders could not be loaded.");
        });
    },
    [isReady, reloadCount],
  );

  function handleStatusChanged() {
    setReloadCount(function countOneMore(currentCount) {
      return currentCount + 1;
    });
  }

  // Nothing of this page is drawn once the stored login has gone: the redirect from useRequireLogin
  // is already on its way, and what was fetched belonged to whoever was signed in a moment ago.
  if (isReady === false) {
    return <p>Loading…</p>;
  }

  if (errorMessage !== "") {
    return <p className="text-red-700">{errorMessage}</p>;
  }

  if (orders === null) {
    return <p>Loading…</p>;
  }

  const rowElements: React.ReactElement[] = [];
  for (const order of orders) {
    const placedAt = new Date(order.placedAt);
    rowElements.push(
      <tr key={order.id} className="border-b border-gray-200 align-top">
        <td className="py-2">
          <Link
            href={`/orders/${order.id}`}
            className="text-accent hover:underline"
          >
            Order {order.id}
          </Link>
        </td>
        <td className="py-2">{placedAt.toLocaleString()}</td>
        <td className="py-2">{order.status}</td>
        <td className="py-2">{order.paymentMethod}</td>
        <td className="py-2">Rs {order.total}</td>
        <td className="py-2">
          <OrderStatusButtons
            orderId={order.id}
            status={order.status}
            onStatusChanged={handleStatusChanged}
          />
        </td>
      </tr>,
    );
  }

  let content = <p>There are no orders yet.</p>;
  if (rowElements.length > 0) {
    content = (
      <table className="w-full border-collapse">
        <thead>
          <tr className="border-b border-gray-300 text-left">
            <th className="py-2">Order</th>
            <th className="py-2">Placed</th>
            <th className="py-2">Status</th>
            <th className="py-2">Payment</th>
            <th className="py-2">Total</th>
            <th className="py-2"></th>
          </tr>
        </thead>
        <tbody>{rowElements}</tbody>
      </table>
    );
  }

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">Store orders</h1>
      {content}
    </div>
  );
}
