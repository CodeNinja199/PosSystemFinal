"use client";

import { useEffect, useState } from "react";
import { readFromApi } from "@/lib/callWebApi";
import { useRequireLogin } from "@/lib/useRequireLogin";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";
import type { ProductResponse } from "@/lib/types/ProductResponse";
import { ProductGrid } from "@/app/components/ProductGrid";

// A client component because the token lives in localStorage, which only the browser can read.
// The two fetches do not depend on each other, so they run at the same time and the page waits for both.
export default function ProductsPage() {
  const { isReady } = useRequireLogin(null);
  const [products, setProducts] = useState<ProductResponse[] | null>(null);
  const [categories, setCategories] = useState<CategoryResponse[] | null>(null);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(
    function loadProductsAndCategories() {
      if (isReady === false) {
        return;
      }

      Promise.all([
        readFromApi<CategoryResponse[]>("/pos/categories"),
        readFromApi<ProductResponse[]>("/pos/products"),
      ])
        .then(function showThem([loadedCategories, loadedProducts]) {
          setCategories(loadedCategories);
          setProducts(loadedProducts);
        })
        .catch(function showTheProblem() {
          setErrorMessage("The products could not be loaded.");
        });
    },
    [isReady],
  );

  // Nothing of this page is drawn once the stored login has gone: the redirect from useRequireLogin
  // is already on its way, and what was fetched belonged to whoever was signed in a moment ago.
  if (isReady === false) {
    return <p>Loading…</p>;
  }

  if (errorMessage !== "") {
    return <p className="text-red-700">{errorMessage}</p>;
  }

  if (products === null || categories === null) {
    return <p>Loading…</p>;
  }

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">Products</h1>
      <ProductGrid products={products} categories={categories} />
    </div>
  );
}
