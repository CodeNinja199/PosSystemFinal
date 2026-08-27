import { notFound, redirect } from "next/navigation";
import { callPosApi } from "@/lib/callPosApi";
import { readLoginTokenFromCookies } from "@/lib/readLoginTokenFromCookies";
import type { OrderResponse } from "@/lib/types/OrderResponse";

// The receipt: one order with its lines, in a single column. A customer may only open their own; staff may open any order of the store.
export default async function OrderReceiptPage(
  props: PageProps<"/orders/[id]">,
) {
  const token = await readLoginTokenFromCookies();
  if (token === null) {
    redirect("/login");
  }

  const parameters = await props.params;
  const orderId = parameters.id;

  const orderResponse = await callPosApi(`/api/orders/${orderId}`, {
    method: "GET",
    token: token,
    body: null,
  });
  if (orderResponse.status === 404) {
    notFound();
  }
  if (orderResponse.status === 403) {
    return <p>This order belongs to another customer.</p>;
  }

  const order: OrderResponse = await orderResponse.json();
  const placedAt = new Date(order.placedAt);

  const lineElements: React.ReactElement[] = [];
  for (const item of order.items) {
    lineElements.push(
      <tr key={item.productId} className="border-b border-gray-200">
        <td className="py-2">{item.productName}</td>
        <td className="py-2">{item.quantity}</td>
        <td className="py-2">Rs {item.unitPrice}</td>
        <td className="py-2">Rs {item.lineTotal}</td>
      </tr>,
    );
  }

  return (
    <div className="max-w-xl">
      <h1 className="mb-1 text-2xl font-bold">Order {order.id}</h1>
      <p>Placed {placedAt.toLocaleString()}</p>
      <p>Status: {order.status}</p>
      <p>Payment: {order.paymentMethod}</p>
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
      <p className="mt-4 text-lg font-bold">Total: Rs {order.total}</p>
    </div>
  );
}
