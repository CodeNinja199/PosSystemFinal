import { readCurrentUserFromCookies } from "@/lib/readCurrentUserFromCookies";
import { requireLoginToken } from "@/lib/requireLoginToken";
import { CheckoutForm } from "@/app/components/CheckoutForm";

// A customer places an order here; staff complete a walk-in sale on the same form, which then also asks for the cash handed over.
export default async function CheckoutPage() {
  await requireLoginToken();
  const currentUser = await readCurrentUserFromCookies();

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
