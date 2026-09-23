"use client";

import { useEffect, useState } from "react";
import { readFromApi } from "@/lib/callWebApi";
import { useRequireLogin } from "@/lib/useRequireLogin";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";
import type { ProductResponse } from "@/lib/types/ProductResponse";
import { ProductForm } from "@/app/components/ProductForm";
import { ProductTable } from "@/app/components/ProductTable";

// A client component because the token lives in localStorage, which only the browser can read.
// The role check is the same convenience it was on the server: the API enforces Admin regardless.
// The two fetches do not depend on each other, so they run at the same time and the page waits for both.
export default function AdminProductsPage() {
  const { isReady } = useRequireLogin(["Admin"]);
  const [categories, setCategories] = useState<CategoryResponse[] | null>(null);
  const [products, setProducts] = useState<ProductResponse[] | null>(null);
  const [errorMessage, setErrorMessage] = useState("");

  // Counted up by handleProductChanged below. A change used to be shown by asking Next to render the
  // page on the server again; the fetch now happens here in the browser, so a change has to ask for
  // that fetch to run again instead.
  const [reloadCount, setReloadCount] = useState(0);

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
    [isReady, reloadCount],
  );

  function handleProductChanged() {
    setReloadCount(function countOneMore(currentCount) {
      return currentCount + 1;
    });
  }

  // Nothing of this page is drawn once the stored login has gone: the redirect from useRequireLogin
  // is already on its way, and what was fetched belonged to whoever was signed in a moment ago.
  if (isReady === false) {
    return <p>Loading…</p>;
  }

  if (errorMessage !== "") {
    return <p className="text-red-700">{errorMessage}</p>;
  }

  if (categories === null || products === null) {
    return <p>Loading…</p>;
  }

  return (
    <div className="flex flex-col gap-8">
      <div>
        <h1 className="mb-4 text-2xl font-bold">Manage products</h1>
        <ProductTable
          products={products}
          categories={categories}
          onProductChanged={handleProductChanged}
        />
      </div>
      <div>
        <h2 className="mb-4 text-xl font-bold">Add a product</h2>
        <ProductForm
          categories={categories}
          product={null}
          onSaved={handleProductChanged}
        />
      </div>
    </div>
  );
}
