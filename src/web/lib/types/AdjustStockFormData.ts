// What the stock form posts to app/api/products/[id]/stock: a positive or negative change.
export type AdjustStockFormData = {
  change: number;
};
