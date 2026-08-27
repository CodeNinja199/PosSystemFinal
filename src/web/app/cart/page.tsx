import { requireLoginToken } from "@/lib/requireLoginToken";
import { CartTable } from "@/app/components/CartTable";

// The cart itself lives in the browser's store; the server page only checks the login and renders the table.
export default async function CartPage() {
  const token = await requireLoginToken();

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">Cart</h1>
      <CartTable />
    </div>
  );
}
