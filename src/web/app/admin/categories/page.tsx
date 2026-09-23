"use client";

import { useEffect, useState } from "react";
import { readFromApi } from "@/lib/callWebApi";
import { useRequireLogin } from "@/lib/useRequireLogin";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";
import { CategoryManager } from "@/app/components/CategoryManager";

// A client component because the token lives in localStorage, which only the browser can read.
// The role check is the same convenience it was on the server: the API enforces Admin regardless.
export default function AdminCategoriesPage() {
  const { isReady } = useRequireLogin(["Admin"]);
  const [categories, setCategories] = useState<CategoryResponse[] | null>(null);
  const [errorMessage, setErrorMessage] = useState("");

  // Counted up by handleCategoryChanged below. A change used to be shown by asking Next to render
  // the page on the server again; the fetch now happens here in the browser, so a change has to ask
  // for that fetch to run again instead.
  const [reloadCount, setReloadCount] = useState(0);

  useEffect(
    function loadCategories() {
      if (isReady === false) {
        return;
      }

      readFromApi<CategoryResponse[]>("/pos/categories")
        .then(function showThem(loadedCategories) {
          setCategories(loadedCategories);
        })
        .catch(function showTheProblem() {
          setErrorMessage("The categories could not be loaded.");
        });
    },
    [isReady, reloadCount],
  );

  function handleCategoryChanged() {
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

  if (categories === null) {
    return <p>Loading…</p>;
  }

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">Categories</h1>
      <CategoryManager
        categories={categories}
        onCategoryChanged={handleCategoryChanged}
      />
    </div>
  );
}
