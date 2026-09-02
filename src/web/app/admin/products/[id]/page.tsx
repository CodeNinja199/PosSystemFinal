import { notFound } from "next/navigation";
import { callPosApi } from "@/lib/callPosApi";
import { requireLoginToken } from "@/lib/requireLoginToken";
import { requireRole } from "@/lib/requireRole";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";
import type { ProductResponse } from "@/lib/types/ProductResponse";
import { ProductForm } from "@/app/components/ProductForm";

export default async function EditProductPage(
  props: PageProps<"/admin/products/[id]">,
) {
  const token = await requireLoginToken();
  await requireRole(["Admin"]);

  const parameters = await props.params;

  const productPromise = callPosApi(`/pos/products/${parameters.id}`, {
    method: "GET",
    token: token,
    body: null,
  });
  const categoriesPromise = callPosApi("/pos/categories", {
    method: "GET",
    token: token,
    body: null,
  });
  const [productResponse, categoriesResponse] = await Promise.all([
    productPromise,
    categoriesPromise,
  ]);

  if (productResponse.status === 404) {
    notFound();
  }

  const product: ProductResponse = await productResponse.json();
  const categories: CategoryResponse[] = await categoriesResponse.json();

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">Edit {product.name}</h1>
      <ProductForm categories={categories} product={product} />
    </div>
  );
}
