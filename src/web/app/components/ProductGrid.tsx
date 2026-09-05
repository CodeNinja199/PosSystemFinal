"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";
import type { ProductResponse } from "@/lib/types/ProductResponse";
import { ProductCard } from "@/app/components/ProductCard";

type ProductGridProps = {
  products: ProductResponse[];
  categories: CategoryResponse[];
};

// The interactive part of the products page: a category list, a search box, and the cards that match both. Fed by props from the server page.
export function ProductGrid(props: ProductGridProps) {
  const products = props.products;
  const categories = props.categories;
  const [selectedCategoryId, setSelectedCategoryId] = useState("");
  const [searchText, setSearchText] = useState("");
  const searchInputRef = useRef<HTMLInputElement>(null);

  useEffect(function focusSearchBoxWhenPageOpens() {
    if (searchInputRef.current !== null) {
      searchInputRef.current.focus();
    }
  }, []);

  const filteredProducts = useMemo(
    function filterProductsByCategoryAndSearchText() {
      const searchTextToMatch = searchText.trim().toLowerCase();

      const matchingProducts: ProductResponse[] = [];
      for (const product of products) {
        const isInSelectedCategory =
          selectedCategoryId === "" ||
          String(product.categoryId) === selectedCategoryId;
        const isSearchMatch =
          searchTextToMatch === "" ||
          product.name.toLowerCase().includes(searchTextToMatch);
        if (isInSelectedCategory && isSearchMatch) {
          matchingProducts.push(product);
        }
      }

      return matchingProducts;
    },
    [products, selectedCategoryId, searchText],
  );

  function handleCategoryChange(event: React.ChangeEvent<HTMLSelectElement>) {
    setSelectedCategoryId(event.target.value);
  }

  function handleSearchTextChange(event: React.ChangeEvent<HTMLInputElement>) {
    setSearchText(event.target.value);
  }

  const categoryNamesById = new Map<number, string>();
  const categoryOptionElements: React.ReactElement[] = [];
  for (const category of categories) {
    categoryNamesById.set(category.id, category.name);
    categoryOptionElements.push(
      <option key={category.id} value={String(category.id)}>
        {category.name}
      </option>,
    );
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
      <div className="flex flex-wrap gap-4">
        <div className="flex flex-col gap-1">
          <label htmlFor="category">Category</label>
          <select
            id="category"
            value={selectedCategoryId}
            onChange={handleCategoryChange}
            className="border border-gray-400 px-2 py-1"
          >
            <option value="">All categories</option>
            {categoryOptionElements}
          </select>
        </div>
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
      </div>
      {gridElement}
    </div>
  );
}
