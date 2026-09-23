"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useDispatch } from "react-redux";
import { clearCart } from "@/lib/cartSlice";
import type { AppDispatch } from "@/lib/store";
import { homePathForRole } from "@/lib/homePathForRole";
import { saveLoginSession } from "@/lib/loginTokenStorage";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { LoginFormData } from "@/lib/types/LoginFormData";
import type { LoginResponse } from "@/lib/types/LoginResponse";

export default function LoginPage() {
  const router = useRouter();
  const dispatch = useDispatch<AppDispatch>();
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

      // The API hands back the token; this page is what puts it into localStorage.
      const loginResponse: LoginResponse = await response.json();
      saveLoginSession(loginResponse.token, {
        fullName: loginResponse.fullName,
        role: loginResponse.role,
      });

      // A session always starts with an empty cart. The cart used to be thrown away by the full page
      // load that logging out caused; now that every move between pages is a client-side navigation
      // the store lives as long as the tab, so whoever logs in next must not find the last person's
      // lines waiting - however that last session ended: log out, an expired token, or a 401.
      dispatch(clearCart());
      // No router.refresh() any more: it existed to re-render the navigation bar on the server,
      // which now reads the stored login in the browser and updates itself when it changes.
      router.push(homePathForRole(loginResponse.role));
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
