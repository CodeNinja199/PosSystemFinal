"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { CategoryFormData } from "@/lib/types/CategoryFormData";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";

type CategoryManagerProps = {
  categories: CategoryResponse[];
};

// The admin's category table: an add form above, a rename form and a delete button per row. Every change refreshes the server page.
export function CategoryManager(props: CategoryManagerProps) {
  const categories = props.categories;
  const router = useRouter();
  const [newCategoryName, setNewCategoryName] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function sendCategoryRequest(
    path: string,
    method: string,
    body: CategoryFormData | null,
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

      setNewCategoryName("");
      setIsSubmitting(false);
      router.refresh();
    } catch {
      setErrorMessage("The server could not be reached.");
      setIsSubmitting(false);
    }
  }

  async function handleAddCategoryFormSubmit(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();
    await sendCategoryRequest("/api/categories", "POST", {
      name: newCategoryName,
    });
  }

  function handleNewCategoryNameChange(
    event: React.ChangeEvent<HTMLInputElement>,
  ) {
    setNewCategoryName(event.target.value);
  }

  const rowElements: React.ReactElement[] = [];
  for (const category of categories) {
    rowElements.push(
      <CategoryRow
        key={category.id}
        category={category}
        isSubmitting={isSubmitting}
        sendCategoryRequest={sendCategoryRequest}
      />,
    );
  }

  let errorElement = null;
  if (errorMessage !== "") {
    errorElement = <p className="text-red-700">{errorMessage}</p>;
  }

  let buttonText = "Add category";
  if (isSubmitting) {
    buttonText = "Saving…";
  }

  return (
    <div className="flex flex-col gap-4">
      <form
        onSubmit={handleAddCategoryFormSubmit}
        className="flex items-end gap-2"
      >
        <div className="flex flex-col gap-1">
          <label htmlFor="newCategoryName">New category</label>
          <input
            id="newCategoryName"
            type="text"
            value={newCategoryName}
            onChange={handleNewCategoryNameChange}
            required
            className="border border-gray-400 px-2 py-1"
          />
        </div>
        <button
          type="submit"
          disabled={isSubmitting}
          className="bg-accent px-3 py-1 text-white disabled:opacity-60"
        >
          {buttonText}
        </button>
      </form>
      {errorElement}
      <table className="w-full border-collapse">
        <thead>
          <tr className="border-b border-gray-300 text-left">
            <th className="py-2">Id</th>
            <th className="py-2">Name</th>
            <th className="py-2"></th>
          </tr>
        </thead>
        <tbody>{rowElements}</tbody>
      </table>
    </div>
  );
}

type CategoryRowProps = {
  category: CategoryResponse;
  isSubmitting: boolean;
  sendCategoryRequest: (
    path: string,
    method: string,
    body: CategoryFormData | null,
  ) => Promise<void>;
};

function CategoryRow(props: CategoryRowProps) {
  const category = props.category;
  const isSubmitting = props.isSubmitting;
  const sendCategoryRequest = props.sendCategoryRequest;
  const [name, setName] = useState(category.name);

  function handleNameChange(event: React.ChangeEvent<HTMLInputElement>) {
    setName(event.target.value);
  }

  async function handleRenameFormSubmit(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();
    await sendCategoryRequest(`/api/categories/${category.id}`, "PUT", {
      name: name,
    });
  }

  async function handleDeleteButtonClick() {
    await sendCategoryRequest(`/api/categories/${category.id}`, "DELETE", null);
  }

  return (
    <tr className="border-b border-gray-200">
      <td className="py-2">{category.id}</td>
      <td className="py-2">
        <form onSubmit={handleRenameFormSubmit} className="flex gap-2">
          <label htmlFor={`categoryName${category.id}`} className="sr-only">
            Name of category {category.id}
          </label>
          <input
            id={`categoryName${category.id}`}
            type="text"
            value={name}
            onChange={handleNameChange}
            required
            className="border border-gray-400 px-2 py-1"
          />
          <button
            type="submit"
            disabled={isSubmitting}
            className="border border-gray-400 px-2 py-1 hover:bg-gray-100 disabled:opacity-60"
          >
            Rename
          </button>
        </form>
      </td>
      <td className="py-2">
        <button
          type="button"
          onClick={handleDeleteButtonClick}
          disabled={isSubmitting}
          className="border border-gray-400 px-2 py-1 hover:bg-gray-100 disabled:opacity-60"
        >
          Delete
        </button>
      </td>
    </tr>
  );
}
