import Link from "next/link";
import { callPosApi } from "@/lib/callPosApi";
import { requireLoginToken } from "@/lib/requireLoginToken";
import type { OrderResponse } from "@/lib/types/OrderResponse";

export default async function MyOrdersPage() {
  const token = await requireLoginToken();

  const ordersResponse = await callPosApi("/pos/orders/mine", {
    method: "GET",
    token: token,
    body: null,
  });
  const orders: OrderResponse[] = await ordersResponse.json();

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
