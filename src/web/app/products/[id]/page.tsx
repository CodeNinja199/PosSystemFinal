"use client";

import { useEffect, useState } from "react";
import Image from "next/image";
import { useParams } from "next/navigation";
import { callWebApi, readFromApi } from "@/lib/callWebApi";
import { useRequireLogin } from "@/lib/useRequireLogin";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";
import type { ProductResponse } from "@/lib/types/ProductResponse";
import { AddToCartButton } from "@/app/components/AddToCartButton";

// A client component because the token lives in localStorage, which only the browser can read.
// That is also why the id from the folder name [id] comes from useParams: a client component is
// not handed the route's params, and there is no promise to await.
//
// notFound() is a server-side call, so a product the store does not have is reported with a
// message of its own instead. It still has to be told apart from a request that simply failed,
// and only the raw response carries the status, so the product is read with callWebApi while
// the categories go through readFromApi.
export default function ProductDetailPage() {
  const { isReady } = useRequireLogin(null);
  const routeParameters = useParams<{ id: string }>();
  const productId = routeParameters.id;
  const [product, setProduct] = useState<ProductResponse | null>(null);
  const [categories, setCategories] = useState<CategoryResponse[] | null>(null);
  const [errorMessage, setErrorMessage] = useState("");

  // The two fetches do not depend on each other, so they run at the same time and the page waits for both.
  useEffect(
    function loadTheProductAndCategories() {
      if (isReady === false) {
        return;
      }

      async function loadThem() {
        const [productResponse, loadedCategories] = await Promise.all([
          callWebApi(`/api/gateway/pos/products/${productId}`, {
            method: "GET",
            body: null,
          }),
          readFromApi<CategoryResponse[]>("/pos/categories"),
        ]);

        const wasNotFound = productResponse.status === 404;
        if (wasNotFound) {
          setErrorMessage("That product could not be found.");
          return;
        }

        if (productResponse.ok === false) {
          throw new Error(
            `GET /pos/products/${productId} answered ${productResponse.status}`,
          );
        }

        const loadedProduct: ProductResponse = await productResponse.json();
        setProduct(loadedProduct);
        setCategories(loadedCategories);
      }

      loadThem().catch(function showTheProblem() {
        setErrorMessage("The product could not be loaded.");
      });
    },
    [isReady, productId],
  );

  // Nothing of this page is drawn once the stored login has gone: the redirect from useRequireLogin
  // is already on its way, and what was fetched belonged to whoever was signed in a moment ago.
  if (isReady === false) {
    return <p>Loading…</p>;
  }

  if (errorMessage !== "") {
    return <p className="text-red-700">{errorMessage}</p>;
  }

  if (product === null || categories === null) {
    return <p>Loading…</p>;
  }

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
      <div className="mt-4">
        <AddToCartButton product={product} />
      </div>
    </div>
  );
}
