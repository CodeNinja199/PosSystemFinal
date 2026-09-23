"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { readFromApi } from "@/lib/callWebApi";
import { useRequireLogin } from "@/lib/useRequireLogin";
import type { OrderResponse } from "@/lib/types/OrderResponse";

// A client component because the token lives in localStorage, which only the browser can read.
// The staff redirect therefore happens in an effect as well, once the stored user is known, rather
// than on the server before anything renders.
export default function MyOrdersPage() {
  const router = useRouter();
  const { isReady, currentUser } = useRequireLogin(null);
  const [orders, setOrders] = useState<OrderResponse[] | null>(null);
  const [errorMessage, setErrorMessage] = useState("");

  // Staff have no orders of their own: the sales they ring up are in the store's orders.
  const isStaff = currentUser !== null && currentUser.role !== "Customer";

  useEffect(
    function sendStaffToTheStoreOrders() {
      if (isStaff === false) {
        return;
      }

      router.replace("/admin/orders");
    },
    [isStaff, router],
  );

  useEffect(
    function loadMyOrders() {
      // Nothing is fetched for staff: they are on their way to the store's orders instead.
      if (isReady === false || isStaff) {
        return;
      }

      readFromApi<OrderResponse[]>("/pos/orders/mine")
        .then(function showThem(loadedOrders) {
          setOrders(loadedOrders);
        })
        .catch(function showTheProblem() {
          setErrorMessage("Your orders could not be loaded.");
        });
    },
    [isReady, isStaff],
  );

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
      <tr key={order.id} className="border-b border-gray-200">
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
      </tr>,
    );
  }

  let content = <p>You have not placed any orders yet.</p>;
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
          </tr>
        </thead>
        <tbody>{rowElements}</tbody>
      </table>
    );
  }

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">My orders</h1>
      {content}
    </div>
  );
}
