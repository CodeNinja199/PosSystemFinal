"use client";

import type { CurrentUser } from "@/lib/types/CurrentUser";

// The token now lives in the browser's localStorage instead of an httpOnly cookie.
// Everything here therefore runs in the browser only: the Next.js server cannot read localStorage,
// which is why every page that needs the token is a client component.
//
// How reading the stored login works here:
// 1. saveLoginSession and clearLoginSession write it, and then announce that it changed.
// 2. subscribeToLoginSession lets a component listen for that announcement.
// 3. readLoginToken and readStoredCurrentUserText hand back the stored text as it is now.
// Steps 2 and 3 are the two halves React's useSyncExternalStore asks for, which is how the
// navigation bar and useRequireLogin read this without keeping a copy of it in their own state.
// Learned from: https://react.dev/reference/react/useSyncExternalStore
export const LoginTokenStorageKey = "pos_token";

export const CurrentUserStorageKey = "pos_user";

// The browser's own "storage" event only fires in the OTHER tabs, never in the tab that did the
// writing, so logging in or out needs an announcement of its own for this tab to notice.
const LoginSessionChangedEventName = "pos-login-session-changed";

// localStorage throws in a few real situations - a private window with site data blocked, or
// storage disabled by policy - so every read and write is guarded and a failure is treated as
// "nobody is logged in" rather than allowed to break the page.
export function saveLoginSession(
  token: string,
  currentUser: CurrentUser,
): void {
  try {
    window.localStorage.setItem(LoginTokenStorageKey, token);
    window.localStorage.setItem(
      CurrentUserStorageKey,
      JSON.stringify(currentUser),
    );
    window.dispatchEvent(new Event(LoginSessionChangedEventName));
  } catch {
    // Nothing useful to do here: the next read returns null and the user is sent to log in again.
  }
}

export function clearLoginSession(): void {
  try {
    window.localStorage.removeItem(LoginTokenStorageKey);
    window.localStorage.removeItem(CurrentUserStorageKey);
    window.dispatchEvent(new Event(LoginSessionChangedEventName));
  } catch {
    // Same as above: a failure here only means the browser was not storing anything anyway.
  }
}

export function readLoginToken(): string | null {
  try {
    return window.localStorage.getItem(LoginTokenStorageKey);
  } catch {
    return null;
  }
}

export function readCurrentUser(): CurrentUser | null {
  return parseCurrentUser(readStoredCurrentUserText());
}

// The stored text rather than a parsed object, because useSyncExternalStore compares what it is
// given with what it had last time: a fresh object out of JSON.parse would never look equal and
// React would re-render for ever. Two identical strings do look equal.
export function readStoredCurrentUserText(): string | null {
  try {
    return window.localStorage.getItem(CurrentUserStorageKey);
  } catch {
    return null;
  }
}

// Handed to useSyncExternalStore as the value to use while the page is being rendered on the
// server and while React is hydrating it. undefined, rather than null, so a component can tell
// "the browser has not been asked yet" apart from "the browser has nothing stored".
export function readNothingWhilePrerendering(): undefined {
  return undefined;
}

export function parseCurrentUser(
  storedText: string | null | undefined,
): CurrentUser | null {
  if (storedText === null || storedText === undefined) {
    return null;
  }

  try {
    const currentUser: CurrentUser = JSON.parse(storedText);
    return currentUser;
  } catch {
    return null;
  }
}

export function subscribeToLoginSession(onChange: () => void): () => void {
  window.addEventListener(LoginSessionChangedEventName, onChange);
  window.addEventListener("storage", onChange);

  return function unsubscribe() {
    window.removeEventListener(LoginSessionChangedEventName, onChange);
    window.removeEventListener("storage", onChange);
  };
}
