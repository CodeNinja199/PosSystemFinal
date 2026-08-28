import { callPosApi } from "@/lib/callPosApi";
import { requireLoginToken } from "@/lib/requireLoginToken";
import { requireRole } from "@/lib/requireRole";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";
import type { ProductResponse } from "@/lib/types/ProductResponse";
import { ProductForm } from "@/app/components/ProductForm";
import { ProductTable } from "@/app/components/ProductTable";

export default async function AdminProductsPage() {
  const token = await requireLoginToken();
  await requireRole(["Admin"]);

  const categoriesPromise = callPosApi("/api/categories", {
    method: "GET",
    token: token,
    body: null,
  });
  const productsPromise = callPosApi("/api/products", {
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
    <div className="flex flex-col gap-8">
      <div>
        <h1 className="mb-4 text-2xl font-bold">Manage products</h1>
        <ProductTable products={products} categories={categories} />
      </div>
      <div>
        <h2 className="mb-4 text-xl font-bold">Add a product</h2>
        <ProductForm categories={categories} product={null} />
      </div>
    </div>
  );
}
