// What GET api/reports/sales-summary returns for the admin's store.
export type SalesSummaryResponse = {
  dayStartUtc: string;
  totalSales: number;
  gstCollected: number;
  orderCount: number;
};
