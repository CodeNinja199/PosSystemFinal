"use client";

import { clearLoginSession, readLoginToken } from "@/lib/loginTokenStorage";

// The browser's way of reaching the APIs, now that the token is in localStorage.
//
// It still never calls the gateway directly: every request goes to this web app's own
// /api/... routes, which forward it on. The only change from before is that the token is
// read out of localStorage here and sent in the Authorization header, instead of the
// server pulling it out of a cookie.
export type CallWebApiOptions = {
  method: string;
  body: object | null;
};

export async function callWebApi(
  path: string,
  options: CallWebApiOptions,
): Promise<Response> {
  const token = readLoginToken();

  const headers: Record<string, string> = {};
  headers["Content-Type"] = "application/json";
  if (token !== null) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  let bodyText: string | undefined = undefined;
  if (options.body !== null) {
    bodyText = JSON.stringify(options.body);
  }

  const response = await fetch(path, {
    method: options.method,
    headers: headers,
    body: bodyText,
    cache: "no-store",
  });

  // A 401 means the stored token is missing, expired, or refused, and no screen can recover from
  // that on its own, so the login is thrown away here. Nothing has to navigate: every protected page
  // watches the stored login through useRequireLogin, so dropping it is what sends the user to the
  // login page - the same thing that used to happen when the httpOnly cookie expired.
  if (response.status === 401) {
    clearLoginSession();
  }

  return response;
}

// Reads through the forwarding route. The path is the API path as the gateway knows it,
// for example "/pos/products" or "/notifications/notifications".
export async function readFromApi<T>(apiPath: string): Promise<T> {
  const response = await callWebApi(`/api/gateway${apiPath}`, {
    method: "GET",
    body: null,
  });
  if (response.ok === false) {
    throw new Error(`GET ${apiPath} answered ${response.status}`);
  }

  return (await response.json()) as T;
}
