import { callPosApi } from "@/lib/callPosApi";
import { requireLoginToken } from "@/lib/requireLoginToken";
import { requireRole } from "@/lib/requireRole";
import type { UserResponse } from "@/lib/types/UserResponse";

export default async function AdminCustomersPage() {
  const token = await requireLoginToken();
  await requireRole(["Admin"]);

  const customersResponse = await callPosApi("/api/customers", {
    method: "GET",
    token: token,
    body: null,
  });
  const customers: UserResponse[] = await customersResponse.json();

  const rowElements: React.ReactElement[] = [];
  for (const customer of customers) {
    const registeredAt = new Date(customer.registeredAt);
    rowElements.push(
      <tr key={customer.id} className="border-b border-gray-200">
        <td className="py-2">{customer.id}</td>
        <td className="py-2">{customer.fullName}</td>
        <td className="py-2">{customer.email}</td>
        <td className="py-2">{registeredAt.toLocaleDateString()}</td>
      </tr>,
    );
  }

  let content = <p>No customers have registered yet.</p>;
  if (rowElements.length > 0) {
    content = (
      <table className="w-full border-collapse">
        <thead>
          <tr className="border-b border-gray-300 text-left">
            <th className="py-2">Id</th>
            <th className="py-2">Name</th>
            <th className="py-2">Email</th>
            <th className="py-2">Registered</th>
          </tr>
        </thead>
        <tbody>{rowElements}</tbody>
      </table>
    );
  }

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">Customers</h1>
      {content}
    </div>
  );
}
