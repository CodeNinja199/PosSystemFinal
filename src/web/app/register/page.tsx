import { callPosApi } from "@/lib/callPosApi";
import type { StoreResponse } from "@/lib/types/StoreResponse";
import { RegisterForm } from "@/app/components/RegisterForm";

// Rendered when it is asked for, never while the Docker image is being built: the store list comes
// from the API, and during the build there is no gateway to ask. Every page used to be like this
// because the navigation bar read a cookie on the server; now that the signed-in user comes from
// localStorage, this is the only page left that fetches anything on the server.
// Learned from: https://nextjs.org/docs/app/api-reference/file-conventions/route-segment-config#dynamic
export const dynamic = "force-dynamic";

// A server component: it loads the store list on the server and hands it to the form as props.
export default async function RegisterPage() {
  const storesResponse = await callPosApi("/pos/stores", {
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
