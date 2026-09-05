"use client";

import Link from "next/link";
import { useState } from "react";
import { useRouter } from "next/navigation";
import type { AdjustStockFormData } from "@/lib/types/AdjustStockFormData";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";
import type { ProductResponse } from "@/lib/types/ProductResponse";

type ProductTableProps = {
  products: ProductResponse[];
  categories: CategoryResponse[];
};

// The admin's product table: a stock form and a delete button per row, an Edit link to the edit page, one error line.
export function ProductTable(props: ProductTableProps) {
  const products = props.products;
  const categories = props.categories;
  const router = useRouter();
  const [errorMessage, setErrorMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function sendProductRequest(
    path: string,
    method: string,
    body: AdjustStockFormData | null,
  ) {
    setErrorMessage("");
    setIsSubmitting(true);

    let bodyText: string | undefined = undefined;
    if (body !== null) {
      bodyText = JSON.stringify(body);
    }

    try {
      const response = await fetch(path, {
        method: method,
        headers: { "Content-Type": "application/json" },
        body: bodyText,
      });

      if (response.ok === false) {
        const errorBody: ApiErrorResponse = await response.json();
        setErrorMessage(errorBody.message);
        setIsSubmitting(false);
        return;
      }

      setIsSubmitting(false);
      router.refresh();
    } catch {
      setErrorMessage("The server could not be reached.");
      setIsSubmitting(false);
    }
  }

  const categoryNamesById = new Map<number, string>();
  for (const category of categories) {
    categoryNamesById.set(category.id, category.name);
  }

  const rowElements: React.ReactElement[] = [];
  for (const product of products) {
    let categoryName = categoryNamesById.get(product.categoryId);
    if (categoryName === undefined) {
      categoryName = "";
    }
    rowElements.push(
      <ProductRow
        key={product.id}
        product={product}
        categoryName={categoryName}
        isSubmitting={isSubmitting}
        sendProductRequest={sendProductRequest}
      />,
    );
  }

  let errorElement = null;
  if (errorMessage !== "") {
    errorElement = <p className="text-red-700">{errorMessage}</p>;
  }

  return (
    <div className="flex flex-col gap-2">
      <table className="w-full border-collapse">
        <thead>
          <tr className="border-b border-gray-300 text-left">
            <th className="py-2">Name</th>
            <th className="py-2">Category</th>
            <th className="py-2">Price</th>
            <th className="py-2">Stock</th>
            <th className="py-2">Threshold</th>
            <th className="py-2">Adjust stock</th>
            <th className="py-2"></th>
          </tr>
        </thead>
        <tbody>{rowElements}</tbody>
      </table>
      {errorElement}
    </div>
  );
}

type ProductRowProps = {
  product: ProductResponse;
  categoryName: string;
  isSubmitting: boolean;
  sendProductRequest: (
    path: string,
    method: string,
    body: AdjustStockFormData | null,
  ) => Promise<void>;
};

function ProductRow(props: ProductRowProps) {
  const product = props.product;
  const categoryName = props.categoryName;
  const isSubmitting = props.isSubmitting;
  const sendProductRequest = props.sendProductRequest;
  const [change, setChange] = useState("");

  function handleChangeInputChange(event: React.ChangeEvent<HTMLInputElement>) {
    setChange(event.target.value);
  }

  async function handleStockFormSubmit(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();
    await sendProductRequest(`/api/products/${product.id}/stock`, "PATCH", {
      change: Number(change),
    });
    setChange("");
  }

  async function handleDeleteButtonClick() {
    await sendProductRequest(`/api/products/${product.id}`, "DELETE", null);
  }

  return (
    <tr className="border-b border-gray-200 align-top">
      <td className="py-2">{product.name}</td>
      <td className="py-2">{categoryName}</td>
      <td className="py-2">Rs {product.price}</td>
      <td className="py-2">{product.stockQuantity}</td>
      <td className="py-2">{product.lowStockThreshold}</td>
      <td className="py-2">
        <form onSubmit={handleStockFormSubmit} className="flex gap-2">
          <label htmlFor={`change${product.id}`} className="self-center">
            Change
          </label>
          <input
            id={`change${product.id}`}
            type="number"
            step="1"
            value={change}
            onChange={handleChangeInputChange}
            required
            className="w-20 border border-gray-400 px-2 py-1"
          />
          <button
            type="submit"
            disabled={isSubmitting}
            className="border border-gray-400 px-2 py-1 hover:bg-gray-100 disabled:opacity-60"
          >
            Adjust
          </button>
        </form>
      </td>
      <td className="py-2">
        <div className="flex gap-2">
          <Link
            href={`/admin/products/${product.id}`}
            className="border border-gray-400 px-2 py-1 hover:bg-gray-100"
          >
            Edit
          </Link>
          <button
            type="button"
            onClick={handleDeleteButtonClick}
            disabled={isSubmitting}
            className="border border-gray-400 px-2 py-1 hover:bg-gray-100 disabled:opacity-60"
          >
            Delete
          </button>
        </div>
      </td>
    </tr>
  );
}
