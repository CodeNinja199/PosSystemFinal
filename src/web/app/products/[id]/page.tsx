import Image from "next/image";
import { notFound, redirect } from "next/navigation";
import { callPosApi } from "@/lib/callPosApi";
import { readLoginTokenFromCookies } from "@/lib/readLoginTokenFromCookies";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";
import type { ProductResponse } from "@/lib/types/ProductResponse";

// The id comes from the folder name [id]; in Next.js 16 params is a promise, so it is awaited.
export default async function ProductDetailPage(
  props: PageProps<"/products/[id]">,
) {
  const token = await readLoginTokenFromCookies();
  if (token === null) {
    redirect("/login");
  }

  const parameters = await props.params;
  const productId = parameters.id;

  const productPromise = callPosApi(`/api/products/${productId}`, {
    method: "GET",
    token: token,
    body: null,
  });
  const categoriesPromise = callPosApi("/api/categories", {
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

  let categoryName = "";
  for (const category of categories) {
    if (category.id === product.categoryId) {
      categoryName = category.name;
    }
  }

  let imageElement = (
    <div className="flex h-64 items-center justify-center bg-gray-100 text-gray-500">
      No image
    </div>
  );
  if (product.imageUrl !== null) {
    imageElement = (
      <Image
        src={product.imageUrl}
        alt={product.name}
        width={640}
        height={256}
        unoptimized
        className="h-64 w-full object-cover"
      />
    );
  }

  let stockText = `${product.stockQuantity} in stock`;
  if (product.stockQuantity === 0) {
    stockText = "Out of stock";
  }

  return (
    <div className="max-w-xl">
      {imageElement}
      <h1 className="mt-4 text-2xl font-bold">{product.name}</h1>
      <p className="text-gray-700">{categoryName}</p>
      <p className="mt-2 text-xl">Rs {product.price}</p>
      <p>{stockText}</p>
    </div>
  );
}
