import Link from "next/link";
import { callPosApi } from "@/lib/callPosApi";
import { requireLoginToken } from "@/lib/requireLoginToken";
import { requireRole } from "@/lib/requireRole";
import type { OrderResponse } from "@/lib/types/OrderResponse";
import { OrderStatusButtons } from "@/app/components/OrderStatusButtons";

// The store's orders for cashiers and admins. The API answers 403 to anyone else even if they reach the page.
export default async function StoreOrdersPage() {
  const token = await requireLoginToken();
  await requireRole(["Cashier", "Admin"]);

  const ordersResponse = await callPosApi("/pos/orders", {
    method: "GET",
    token: token,
    body: null,
  });
  const orders: OrderResponse[] = await ordersResponse.json();

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
          <OrderStatusButtons orderId={order.id} status={order.status} />
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
