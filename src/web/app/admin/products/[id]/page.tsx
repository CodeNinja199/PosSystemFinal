"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { callWebApi, readFromApi } from "@/lib/callWebApi";
import { useRequireLogin } from "@/lib/useRequireLogin";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";
import type { ProductResponse } from "@/lib/types/ProductResponse";
import { ProductForm } from "@/app/components/ProductForm";

// A client component because the token lives in localStorage, which only the browser can read.
// The role check is the same convenience it was on the server: the API enforces Admin regardless.
//
// The id used to come from the awaited params of a server page; in the browser it comes from useParams
// instead, which is the same [id] folder segment read on the client side.
export default function EditProductPage() {
  const { isReady } = useRequireLogin(["Admin"]);
  const router = useRouter();
  const parameters = useParams<{ id: string }>();
  const productId = parameters.id;
  const [product, setProduct] = useState<ProductResponse | null>(null);
  const [categories, setCategories] = useState<CategoryResponse[] | null>(null);
  const [wasNotFound, setWasNotFound] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(
    function loadProductAndCategories() {
      if (isReady === false) {
        return;
      }

      // The product goes through callWebApi rather than readFromApi because a 404 has to be told apart
      // from a real failure, and readFromApi turns every non-OK status into the same thrown error.
      async function loadThem() {
        const [productResponse, loadedCategories] = await Promise.all([
          callWebApi(`/api/gateway/pos/products/${productId}`, {
            method: "GET",
            body: null,
          }),
          readFromApi<CategoryResponse[]>("/pos/categories"),
        ]);

        // notFound() only works while rendering on the server, so a product that is not there - or
        // belongs to another store, which the API also answers 404 - is shown as a message instead.
        if (productResponse.status === 404) {
          setWasNotFound(true);
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

  // The form used to leave this page itself. It now says when the product has been saved and the
  // page decides, which is what lets the products page reload its list instead of navigating.
  function handleProductSaved() {
    router.push("/admin/products");
  }

  // Nothing of this page is drawn once the stored login has gone: the redirect from useRequireLogin
  // is already on its way, and what was fetched belonged to whoever was signed in a moment ago.
  if (isReady === false) {
    return <p>Loading…</p>;
  }

  if (errorMessage !== "") {
    return <p className="text-red-700">{errorMessage}</p>;
  }

  if (wasNotFound) {
    return <p>That product could not be found.</p>;
  }

  if (product === null || categories === null) {
    return <p>Loading…</p>;
  }

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">Edit {product.name}</h1>
      <ProductForm
        categories={categories}
        product={product}
        onSaved={handleProductSaved}
      />
    </div>
  );
}
