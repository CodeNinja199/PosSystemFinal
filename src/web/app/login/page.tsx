"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { LoginFormData } from "@/lib/types/LoginFormData";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleLoginFormSubmit(
    event: React.FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault();
    setErrorMessage("");
    setIsSubmitting(true);

    const loginFormData: LoginFormData = { email: email, password: password };

    try {
      const response = await fetch("/api/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(loginFormData),
      });

      if (response.ok === false) {
        const errorBody: ApiErrorResponse = await response.json();
        setErrorMessage(errorBody.message);
        setIsSubmitting(false);
        return;
      }

      router.push("/products");
      router.refresh();
    } catch {
      setErrorMessage("The server could not be reached.");
      setIsSubmitting(false);
    }
  }

  function handleEmailChange(event: React.ChangeEvent<HTMLInputElement>) {
    setEmail(event.target.value);
  }

  function handlePasswordChange(event: React.ChangeEvent<HTMLInputElement>) {
    setPassword(event.target.value);
  }

  let buttonText = "Log in";
  if (isSubmitting) {
    buttonText = "Logging in…";
  }

  let errorElement = null;
  if (errorMessage !== "") {
    errorElement = <p className="text-red-700">{errorMessage}</p>;
  }

  return (
    <div className="max-w-sm">
      <h1 className="mb-4 text-2xl font-bold">Log in</h1>
      <form onSubmit={handleLoginFormSubmit} className="flex flex-col gap-3">
        <label htmlFor="email">Email</label>
        <input
          id="email"
          type="email"
          value={email}
          onChange={handleEmailChange}
          required
          className="border border-gray-400 px-2 py-1"
        />
        <label htmlFor="password">Password</label>
        <input
          id="password"
          type="password"
          value={password}
          onChange={handlePasswordChange}
          required
          className="border border-gray-400 px-2 py-1"
        />
        {errorElement}
        <button
          type="submit"
          disabled={isSubmitting}
          className="bg-accent px-4 py-2 text-white disabled:opacity-60"
        >
          {buttonText}
        </button>
      </form>
    </div>
  );
}
