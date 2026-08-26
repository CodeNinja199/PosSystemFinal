import { callPosApi } from "@/lib/callPosApi";
import type { StoreResponse } from "@/lib/types/StoreResponse";
import { RegisterForm } from "@/app/components/RegisterForm";

// A server component: it loads the store list on the server and hands it to the form as props.
export default async function RegisterPage() {
  const storesResponse = await callPosApi("/api/stores", {
    method: "GET",
    token: null,
    body: null,
  });
  const stores: StoreResponse[] = await storesResponse.json();

  return (
    <div className="max-w-sm">
      <h1 className="mb-4 text-2xl font-bold">Register</h1>
      <RegisterForm stores={stores} />
    </div>
  );
}
