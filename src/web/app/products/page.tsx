import { callPosApi } from "@/lib/callPosApi";
import { requireLoginToken } from "@/lib/requireLoginToken";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";
import type { ProductResponse } from "@/lib/types/ProductResponse";
import { ProductGrid } from "@/app/components/ProductGrid";

// The two fetches do not depend on each other, so they run at the same time and the page waits for both.
export default async function ProductsPage() {
  const token = await requireLoginToken();

  const categoriesPromise = callPosApi("/pos/categories", {
    method: "GET",
    token: token,
    body: null,
  });
  const productsPromise = callPosApi("/pos/products", {
    method: "GET",
    token: token,
    body: null,
  });
  const [categoriesResponse, productsResponse] = await Promise.all([
    categoriesPromise,
    productsPromise,
  ]);

  const categories: CategoryResponse[] = await categoriesResponse.json();
  const products: ProductResponse[] = await productsResponse.json();

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">Products</h1>
      <ProductGrid products={products} categories={categories} />
    </div>
  );
}
