import Link from "next/link";
import { callPosApi } from "@/lib/callPosApi";
import { requireLoginToken } from "@/lib/requireLoginToken";
import { requireRole } from "@/lib/requireRole";
import type { NotificationResponse } from "@/lib/types/NotificationResponse";
import type { SalesSummaryResponse } from "@/lib/types/SalesSummaryResponse";

// The admin summary: real numbers from the POS API and the unread low-stock notices from the Notification API, fetched together.
export default async function AdminSummaryPage() {
  const token = await requireLoginToken();
  await requireRole(["Admin"]);

  const summaryPromise = callPosApi("/pos/reports/sales-summary", {
    method: "GET",
    token: token,
    body: null,
  });
  const notificationsPromise = callPosApi("/notifications/notifications", {
    method: "GET",
    token: token,
    body: null,
  });
  const [summaryResponse, notificationsResponse] = await Promise.all([
    summaryPromise,
    notificationsPromise,
  ]);

  const summary: SalesSummaryResponse = await summaryResponse.json();
  const notifications: NotificationResponse[] =
    await notificationsResponse.json();
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
