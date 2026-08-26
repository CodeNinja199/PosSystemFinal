import { redirect } from "next/navigation";
import { callPosApi } from "@/lib/callPosApi";
import { readLoginTokenFromCookies } from "@/lib/readLoginTokenFromCookies";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";
import type { ProductResponse } from "@/lib/types/ProductResponse";

// The two fetches do not depend on each other, so they run at the same time and the page waits for both.
export default async function ProductsPage() {
  const token = await readLoginTokenFromCookies();
  if (token === null) {
    redirect("/login");
  }

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

  const categoryNamesById = new Map<number, string>();
  for (const category of categories) {
    categoryNamesById.set(category.id, category.name);
  }

  const productElements: React.ReactElement[] = [];
  for (const product of products) {
    let categoryName = categoryNamesById.get(product.categoryId);
    if (categoryName === undefined) {
      categoryName = "";
    }
    productElements.push(
      <li key={product.id} className="border border-gray-300 p-4">
        <p className="font-bold">{product.name}</p>
        <p>{categoryName}</p>
        <p>Rs {product.price}</p>
        <p>{product.stockQuantity} in stock</p>
      </li>,
    );
  }

  let content = <p>There are no products yet.</p>;
  if (productElements.length > 0) {
    content = (
      <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {productElements}
      </ul>
    );
  }

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">Products</h1>
      {content}
    </div>
  );
}
