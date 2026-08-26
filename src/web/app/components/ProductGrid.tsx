"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";
import type { ProductResponse } from "@/lib/types/ProductResponse";
import { ProductCard } from "@/app/components/ProductCard";

type ProductGridProps = {
  products: ProductResponse[];
  categories: CategoryResponse[];
};

// The interactive part of the products page: a search box and the cards that match it. Fed by props from the server page.
export function ProductGrid(props: ProductGridProps) {
  const products = props.products;
  const categories = props.categories;
  const [searchText, setSearchText] = useState("");
  const searchInputRef = useRef<HTMLInputElement>(null);

  useEffect(function focusSearchBoxWhenPageOpens() {
    if (searchInputRef.current !== null) {
      searchInputRef.current.focus();
    }
  }, []);

  const filteredProducts = useMemo(
    function filterProductsBySearchText() {
      const searchTextToMatch = searchText.trim().toLowerCase();
      if (searchTextToMatch === "") {
        return products;
      }

      const matchingProducts: ProductResponse[] = [];
      for (const product of products) {
        const isMatch = product.name.toLowerCase().includes(searchTextToMatch);
        if (isMatch) {
          matchingProducts.push(product);
        }
      }

      return matchingProducts;
    },
    [products, searchText],
  );

  function handleSearchTextChange(event: React.ChangeEvent<HTMLInputElement>) {
    setSearchText(event.target.value);
  }

  const categoryNamesById = new Map<number, string>();
  for (const category of categories) {
    categoryNamesById.set(category.id, category.name);
  }

  const cardElements: React.ReactElement[] = [];
  for (const product of filteredProducts) {
    let categoryName = categoryNamesById.get(product.categoryId);
    if (categoryName === undefined) {
      categoryName = "";
    }
    cardElements.push(
      <ProductCard
        key={product.id}
        product={product}
        categoryName={categoryName}
      />,
    );
  }

  let gridElement = <p>No products match your search.</p>;
  if (cardElements.length > 0) {
    gridElement = (
      <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {cardElements}
      </ul>
    );
  }

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-col gap-1">
        <label htmlFor="search">Search products</label>
        <input
          id="search"
          type="search"
          ref={searchInputRef}
          value={searchText}
          onChange={handleSearchTextChange}
          className="max-w-sm border border-gray-400 px-2 py-1"
        />
      </div>
      {gridElement}
    </div>
  );
}
