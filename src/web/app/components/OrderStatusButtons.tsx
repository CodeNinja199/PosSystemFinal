"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { UpdateOrderStatusRequest } from "@/lib/types/UpdateOrderStatusRequest";

type OrderStatusButtonsProps = {
  orderId: number;
  status: string;
};

// Two buttons per Placed order on the store orders page. A finished order shows no buttons: nothing can change it.
export function OrderStatusButtons(props: OrderStatusButtonsProps) {
  const orderId = props.orderId;
  const status = props.status;
  const router = useRouter();
  const [errorMessage, setErrorMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function changeStatus(newStatus: string) {
    setErrorMessage("");
    setIsSubmitting(true);

    const updateOrderStatusRequest: UpdateOrderStatusRequest = {
      status: newStatus,
    };

    try {
      const response = await fetch(`/api/orders/${orderId}/status`, {
        method: "PATCH",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(updateOrderStatusRequest),
      });

      if (response.ok === false) {
        const errorBody: ApiErrorResponse = await response.json();
        setErrorMessage(errorBody.message);
        setIsSubmitting(false);
        return;
      }

      router.refresh();
    } catch {
      setErrorMessage("The server could not be reached.");
      setIsSubmitting(false);
    }
  }

  async function handleCompleteButtonClick() {
    await changeStatus("Completed");
  }

  async function handleCancelButtonClick() {
    await changeStatus("Cancelled");
  }

  if (status !== "Placed") {
    return null;
  }

  let errorElement = null;
  if (errorMessage !== "") {
    errorElement = <p className="text-red-700">{errorMessage}</p>;
  }

  return (
    <div className="flex flex-col gap-1">
      <div className="flex gap-2">
        <button
          type="button"
          onClick={handleCompleteButtonClick}
          disabled={isSubmitting}
          className="bg-accent px-2 py-1 text-white disabled:opacity-60"
        >
          Complete
        </button>
        <button
          type="button"
          onClick={handleCancelButtonClick}
          disabled={isSubmitting}
          className="border border-gray-400 px-2 py-1 hover:bg-gray-100 disabled:opacity-60"
        >
          Cancel
        </button>
      </div>
      {errorElement}
    </div>
  );
}
