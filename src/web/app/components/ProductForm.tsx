"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";
import type { ProductFormData } from "@/lib/types/ProductFormData";
import type { ProductResponse } from "@/lib/types/ProductResponse";

type ProductFormProps = {
  categories: CategoryResponse[];
  product: ProductResponse | null;
};

// Used by the admin products page (product is null: POST) and the edit page (product is set: PUT).
export function ProductForm(props: ProductFormProps) {
  const categories = props.categories;
  const product = props.product;
  const router = useRouter();

  let initialName = "";
  let initialPrice = "";
  let initialStockQuantity = "0";
  let initialLowStockThreshold = "0";
  let initialImageUrl = "";
  let initialCategoryId = "";
  if (product !== null) {
    initialName = product.name;
    initialPrice = String(product.price);
    initialStockQuantity = String(product.stockQuantity);
    initialLowStockThreshold = String(product.lowStockThreshold);
    if (product.imageUrl !== null) {
      initialImageUrl = product.imageUrl;
    }
    initialCategoryId = String(product.categoryId);
  }

  const [name, setName] = useState(initialName);
  const [price, setPrice] = useState(initialPrice);
  const [stockQuantity, setStockQuantity] = useState(initialStockQuantity);
  const [lowStockThreshold, setLowStockThreshold] = useState(
    initialLowStockThreshold,
  );
  const [imageUrl, setImageUrl] = useState(initialImageUrl);
  const [categoryId, setCategoryId] = useState(initialCategoryId);
  const [errorMessage, setErrorMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleProductFormSubmit(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();
    setErrorMessage("");
    setIsSubmitting(true);

    let imageUrlToSend: string | null = null;
    if (imageUrl.trim() !== "") {
      imageUrlToSend = imageUrl.trim();
    }
    const productFormData: ProductFormData = {
      name: name,
      price: Number(price),
      stockQuantity: Number(stockQuantity),
      lowStockThreshold: Number(lowStockThreshold),
      imageUrl: imageUrlToSend,
      categoryId: Number(categoryId),
    };

    let path = "/api/products";
    let method = "POST";
    if (product !== null) {
      path = `/api/products/${product.id}`;
      method = "PUT";
    }

    try {
      const response = await fetch(path, {
        method: method,
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(productFormData),
      });

      if (response.ok === false) {
        const errorBody: ApiErrorResponse = await response.json();
        setErrorMessage(errorBody.message);
        setIsSubmitting(false);
        return;
      }

      router.push("/admin/products");
      router.refresh();
    } catch {
      setErrorMessage("The server could not be reached.");
      setIsSubmitting(false);
    }
  }

  function handleNameChange(event: React.ChangeEvent<HTMLInputElement>) {
    setName(event.target.value);
  }

  function handlePriceChange(event: React.ChangeEvent<HTMLInputElement>) {
    setPrice(event.target.value);
  }

  function handleStockQuantityChange(
    event: React.ChangeEvent<HTMLInputElement>,
  ) {
    setStockQuantity(event.target.value);
  }

  function handleLowStockThresholdChange(
    event: React.ChangeEvent<HTMLInputElement>,
  ) {
    setLowStockThreshold(event.target.value);
  }

  function handleImageUrlChange(event: React.ChangeEvent<HTMLInputElement>) {
    setImageUrl(event.target.value);
  }

  function handleCategoryChange(event: React.ChangeEvent<HTMLSelectElement>) {
    setCategoryId(event.target.value);
  }

  const categoryOptions = categories.map(renderCategoryOption);

  let buttonText = "Save product";
  if (isSubmitting) {
    buttonText = "Saving…";
  }

  let errorElement = null;
  if (errorMessage !== "") {
    errorElement = <p className="text-red-700">{errorMessage}</p>;
  }

  return (
    <form
      onSubmit={handleProductFormSubmit}
      className="flex max-w-md flex-col gap-3"
    >
      <label htmlFor="name">Name</label>
      <input
        id="name"
        type="text"
        value={name}
        onChange={handleNameChange}
        required
        className="border border-gray-400 px-2 py-1"
      />
      <label htmlFor="price">Price (Rs)</label>
      <input
        id="price"
        type="number"
        min="0.01"
        step="0.01"
        value={price}
        onChange={handlePriceChange}
        required
        className="border border-gray-400 px-2 py-1"
      />
      <label htmlFor="stockQuantity">Stock quantity</label>
      <input
        id="stockQuantity"
        type="number"
        min="0"
        step="1"
        value={stockQuantity}
        onChange={handleStockQuantityChange}
        required
        className="border border-gray-400 px-2 py-1"
      />
      <label htmlFor="lowStockThreshold">Low-stock threshold</label>
      <input
        id="lowStockThreshold"
        type="number"
        min="0"
        step="1"
        value={lowStockThreshold}
        onChange={handleLowStockThresholdChange}
        required
        className="border border-gray-400 px-2 py-1"
      />
      <label htmlFor="imageUrl">Image URL (optional)</label>
      <input
        id="imageUrl"
        type="url"
        value={imageUrl}
        onChange={handleImageUrlChange}
        className="border border-gray-400 px-2 py-1"
      />
      <label htmlFor="categoryId">Category</label>
      <select
        id="categoryId"
        value={categoryId}
        onChange={handleCategoryChange}
        required
        className="border border-gray-400 px-2 py-1"
      >
        <option value="">Choose a category</option>
        {categoryOptions}
      </select>
      {errorElement}
      <button
        type="submit"
        disabled={isSubmitting}
        className="w-fit bg-accent px-4 py-2 text-white disabled:opacity-60"
      >
        {buttonText}
      </button>
    </form>
  );
}

function renderCategoryOption(category: CategoryResponse) {
  return (
    <option key={category.id} value={category.id}>
      {category.name}
    </option>
  );
}
