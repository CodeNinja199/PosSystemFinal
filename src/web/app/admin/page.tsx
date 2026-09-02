import { callPosApi } from "@/lib/callPosApi";
import { requireLoginToken } from "@/lib/requireLoginToken";
import { requireRole } from "@/lib/requireRole";
import type { SalesSummaryResponse } from "@/lib/types/SalesSummaryResponse";

// The admin summary: real numbers from the API in plain text, not a dashboard of widgets.
export default async function AdminSummaryPage() {
  const token = await requireLoginToken();
  await requireRole(["Admin"]);

  const summaryResponse = await callPosApi("/pos/reports/sales-summary", {
    method: "GET",
    token: token,
    body: null,
  });
  const summary: SalesSummaryResponse = await summaryResponse.json();
  const dayText = summary.dayStartUtc.slice(0, 10);

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
    </div>
  );
}
