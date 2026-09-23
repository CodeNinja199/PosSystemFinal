"use client";

import { useRequireLogin } from "@/lib/useRequireLogin";
import { CartTable } from "@/app/components/CartTable";

// The cart itself lives in the browser's store; this page only checks the login and renders the table.
// A client component because the token now lives in localStorage, which only the browser can read.
// There is no data to fetch, so the page waits for nothing but that check.
export default function CartPage() {
  const { isReady } = useRequireLogin(null);

  if (isReady === false) {
    return null;
  }

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">Cart</h1>
      <CartTable />
    </div>
  );
}
