"use client";

import { useRequireLogin } from "@/lib/useRequireLogin";
import { CheckoutForm } from "@/app/components/CheckoutForm";

// A customer places an order here; staff complete a walk-in sale on the same form, which then also asks for the cash handed over.
//
// A client component because the token lives in localStorage, which only the browser can read.
// Who is logged in therefore also comes from the browser, which is what decides between the two forms.
export default function CheckoutPage() {
  const { isReady, currentUser } = useRequireLogin(null);

  if (isReady === false) {
    return <p>Loading…</p>;
  }

  let isWalkInSale = false;
  if (currentUser !== null && currentUser.role !== "Customer") {
    isWalkInSale = true;
  }

  let heading = "Checkout";
  if (isWalkInSale) {
    heading = "Complete sale";
  }

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">{heading}</h1>
      <CheckoutForm isWalkInSale={isWalkInSale} />
    </div>
  );
}
