"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { readFromApi } from "@/lib/callWebApi";
import { useRequireLogin } from "@/lib/useRequireLogin";
import type { NotificationResponse } from "@/lib/types/NotificationResponse";
import type { SalesSummaryResponse } from "@/lib/types/SalesSummaryResponse";

// The admin summary: real numbers from the POS API and the unread low-stock notices from the Notification API, fetched together.
// A client component because the token lives in localStorage, which only the browser can read.
// The two fetches do not depend on each other, so they run at the same time and the page waits for both.
export default function AdminSummaryPage() {
  const { isReady } = useRequireLogin(["Admin"]);
  const [summary, setSummary] = useState<SalesSummaryResponse | null>(null);
  const [notifications, setNotifications] = useState<
    NotificationResponse[] | null
  >(null);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(
    function loadSummaryAndNotifications() {
      if (isReady === false) {
        return;
      }

      Promise.all([
        readFromApi<SalesSummaryResponse>("/pos/reports/sales-summary"),
        readFromApi<NotificationResponse[]>("/notifications/notifications"),
      ])
        .then(function showThem([loadedSummary, loadedNotifications]) {
          setSummary(loadedSummary);
          setNotifications(loadedNotifications);
        })
        .catch(function showTheProblem() {
          setErrorMessage("The summary could not be loaded.");
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

  if (summary === null || notifications === null) {
    return <p>Loading…</p>;
  }

  const dayText = summary.dayStartUtc.slice(0, 10);

  const lowStockElements: React.ReactElement[] = [];
  for (const notification of notifications) {
    const isUnreadLowStock =
      notification.type === "stock.low" && notification.isRead === false;
    if (isUnreadLowStock) {
      lowStockElements.push(
        <li key={notification.id}>{notification.message}</li>,
      );
    }
  }

  let lowStockContent = <p>No products are low on stock.</p>;
  if (lowStockElements.length > 0) {
    lowStockContent = <ul className="list-disc pl-5">{lowStockElements}</ul>;
  }

  return (
    <div className="max-w-xl">
      <h1 className="mb-4 text-2xl font-bold">Summary</h1>
      <p>Sales for {dayText} (UTC day)</p>
      <table className="mt-2 w-full border-collapse">
        <tbody>
          <tr className="border-b border-gray-200">
            <td className="py-2">Total sales</td>
            <td className="py-2">Rs {summary.totalSales}</td>
          </tr>
          <tr className="border-b border-gray-200">
            <td className="py-2">Orders placed</td>
            <td className="py-2">{summary.orderCount}</td>
          </tr>
        </tbody>
      </table>
      <h2 className="mt-6 mb-2 text-xl font-bold">Low stock</h2>
      {lowStockContent}
      <p className="mt-2">
        <Link href="/notifications" className="text-accent hover:underline">
          All notifications
        </Link>
      </p>
    </div>
  );
}
