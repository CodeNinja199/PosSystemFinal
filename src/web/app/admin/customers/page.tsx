"use client";

import { useEffect, useState } from "react";
import { readFromApi } from "@/lib/callWebApi";
import { useRequireLogin } from "@/lib/useRequireLogin";
import type { UserResponse } from "@/lib/types/UserResponse";

// A client component because the token lives in localStorage, which only the browser can read.
export default function AdminCustomersPage() {
  const { isReady } = useRequireLogin(["Admin"]);
  const [customers, setCustomers] = useState<UserResponse[] | null>(null);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(
    function loadCustomers() {
      if (isReady === false) {
        return;
      }

      readFromApi<UserResponse[]>("/pos/customers")
        .then(function showThem(loadedCustomers) {
          setCustomers(loadedCustomers);
        })
        .catch(function showTheProblem() {
          setErrorMessage("The customers could not be loaded.");
        });
    },
    [isReady],
  );

  // Nothing of this page is drawn once the stored login has gone: the redirect from useRequireLogin
  // is already on its way, and what was fetched belonged to whoever was signed in a moment ago.
  if (isReady === false) {
    return <p>Loading…</p>;
  }

  if (errorMessage !== "") {
    return <p className="text-red-700">{errorMessage}</p>;
  }

  if (customers === null) {
    return <p>Loading…</p>;
  }

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
