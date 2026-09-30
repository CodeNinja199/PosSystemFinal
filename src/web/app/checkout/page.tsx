"use client";

import { useEffect, useState } from "react";
import { readFromApi } from "@/lib/callWebApi";
import { useRequireLogin } from "@/lib/useRequireLogin";
import type { TaxRateResponse } from "@/lib/types/TaxRateResponse";
import { CheckoutForm } from "@/app/components/CheckoutForm";

// A customer places an order here; staff complete a walk-in sale on the same form, which then also asks for the cash handed over.
//
// A client component because the token lives in localStorage, which only the browser can read.
// Who is logged in therefore also comes from the browser, which is what decides between the two forms.
export default function CheckoutPage() {
  const { isReady, currentUser } = useRequireLogin(null);
  const [gstPercentage, setGstPercentage] = useState<number | null>(null);
  const [errorMessage, setErrorMessage] = useState("");

  // The rate comes from the API rather than being written into the web app, so the screen can never show a different
  // GST from the one the API charges; the API still works the real figure out when the order is placed.
  useEffect(
    function loadTheGstRate() {
      if (isReady === false) {
        return;
      }

      readFromApi<TaxRateResponse>("/pos/tax")
        .then(function keepTheRate(taxRate) {
          setGstPercentage(taxRate.gstPercentage);
        })
        .catch(function showTheProblem() {
          setErrorMessage("The GST rate could not be loaded.");
        });
    },
    [isReady],
  );

  if (isReady === false) {
    return <p>Loading…</p>;
  }

  if (errorMessage !== "") {
    return <p className="text-red-700">{errorMessage}</p>;
  }

  if (gstPercentage === null) {
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
      <CheckoutForm isWalkInSale={isWalkInSale} gstPercentage={gstPercentage} />
    </div>
  );
}
