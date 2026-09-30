// Always two decimal places, because with GST a total is rarely a whole number of rupees, and "Rs 377.6" reads as a
// mistake where "Rs 377.60" does not.
export function formatRupees(amount: number): string {
  return amount.toFixed(2);
}
