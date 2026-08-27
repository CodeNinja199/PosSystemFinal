import { redirect } from "next/navigation";
import { readLoginTokenFromCookies } from "@/lib/readLoginTokenFromCookies";
import { CheckoutForm } from "@/app/components/CheckoutForm";

export default async function CheckoutPage() {
  const token = await readLoginTokenFromCookies();
  if (token === null) {
    redirect("/login");
  }

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">Checkout</h1>
      <CheckoutForm />
    </div>
  );
}
