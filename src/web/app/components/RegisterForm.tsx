"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { RegisterFormData } from "@/lib/types/RegisterFormData";
import type { StoreResponse } from "@/lib/types/StoreResponse";

type RegisterFormProps = {
  stores: StoreResponse[];
};

export function RegisterForm(props: RegisterFormProps) {
  const stores = props.stores;
  const router = useRouter();
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [storeId, setStoreId] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleRegisterFormSubmit(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();
    setErrorMessage("");
    setIsSubmitting(true);

    const registerFormData: RegisterFormData = {
      fullName: fullName,
      email: email,
      password: password,
      storeId: Number(storeId),
    };

    try {
      const response = await fetch("/api/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(registerFormData),
      });

      if (response.ok === false) {
        const errorBody: ApiErrorResponse = await response.json();
        setErrorMessage(errorBody.message);
        setIsSubmitting(false);
        return;
      }

      router.push("/login");
    } catch {
      setErrorMessage("The server could not be reached.");
      setIsSubmitting(false);
    }
  }

  function handleFullNameChange(event: React.ChangeEvent<HTMLInputElement>) {
    setFullName(event.target.value);
  }

  function handleEmailChange(event: React.ChangeEvent<HTMLInputElement>) {
    setEmail(event.target.value);
  }

  function handlePasswordChange(event: React.ChangeEvent<HTMLInputElement>) {
    setPassword(event.target.value);
  }

  function handleStoreChange(event: React.ChangeEvent<HTMLSelectElement>) {
    setStoreId(event.target.value);
  }

  const storeOptions = stores.map(renderStoreOption);

  let buttonText = "Create account";
  if (isSubmitting) {
    buttonText = "Creating account…";
  }

  let errorElement = null;
  if (errorMessage !== "") {
    errorElement = <p className="text-red-700">{errorMessage}</p>;
  }

  return (
    <form onSubmit={handleRegisterFormSubmit} className="flex flex-col gap-3">
      <label htmlFor="fullName">Full name</label>
      <input
        id="fullName"
        type="text"
        value={fullName}
        onChange={handleFullNameChange}
        required
        className="border border-gray-400 px-2 py-1"
      />
      <label htmlFor="email">Email</label>
      <input
        id="email"
        type="email"
        value={email}
        onChange={handleEmailChange}
        required
        className="border border-gray-400 px-2 py-1"
      />
      <label htmlFor="password">Password (at least 8 characters)</label>
      <input
        id="password"
        type="password"
        value={password}
        onChange={handlePasswordChange}
        required
        minLength={8}
        className="border border-gray-400 px-2 py-1"
      />
      <label htmlFor="storeId">Store</label>
      <select
        id="storeId"
        value={storeId}
        onChange={handleStoreChange}
        required
        className="border border-gray-400 px-2 py-1"
      >
        <option value="">Choose a store</option>
        {storeOptions}
      </select>
      {errorElement}
      <button
        type="submit"
        disabled={isSubmitting}
        className="bg-accent px-4 py-2 text-white disabled:opacity-60"
      >
        {buttonText}
      </button>
    </form>
  );
}

function renderStoreOption(store: StoreResponse) {
  return (
    <option key={store.id} value={store.id}>
      {store.name}
    </option>
  );
}
