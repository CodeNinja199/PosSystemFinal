import { requireLoginToken } from "@/lib/requireLoginToken";
import { CheckoutForm } from "@/app/components/CheckoutForm";

export default async function CheckoutPage() {
  const token = await requireLoginToken();

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">Checkout</h1>
      <CheckoutForm />
    </div>
  );
}
