"use client";

import { useEffect, useSyncExternalStore } from "react";
import { useRouter } from "next/navigation";
import { homePathForRole } from "@/lib/homePathForRole";
import { isLoginTokenExpired } from "@/lib/isLoginTokenExpired";
import {
  clearLoginSession,
  parseCurrentUser,
  readLoginToken,
  readNothingWhilePrerendering,
  readStoredCurrentUserText,
  subscribeToLoginSession,
} from "@/lib/loginTokenStorage";
import type { CurrentUser } from "@/lib/types/CurrentUser";

export type RequireLoginResult = {
  // False until the browser has had a chance to look in localStorage. A protected page renders
  // nothing until this is true, so a logged-out visitor never sees the contents for a moment.
  isReady: boolean;
  currentUser: CurrentUser | null;
};

// Called at the top of every protected page. It replaces requireLoginToken and requireRole, which
// read cookies on the server; localStorage can only be read in the browser, so the check has to
// wait until the browser has taken the page over.
//
// This is a convenience only, exactly as the server-side version was: the API enforces the role on
// every call regardless of what the browser believes.
export function useRequireLogin(
  allowedRoles: string[] | null,
): RequireLoginResult {
  const router = useRouter();

  // Both of these are undefined while the page is being prerendered and while React is hydrating
  // it, and become the stored text - or null - once the browser is running the page. They also
  // change when another tab logs in or out, so a page cannot sit there with a login that is gone.
  const storedToken = useSyncExternalStore(
    subscribeToLoginSession,
    readLoginToken,
    readNothingWhilePrerendering,
  );
  const storedUserText = useSyncExternalStore(
    subscribeToLoginSession,
    readStoredCurrentUserText,
    readNothingWhilePrerendering,
  );

  const hasReadTheBrowsersStorage =
    storedToken !== undefined && storedUserText !== undefined;
  const currentUser = parseCurrentUser(storedUserText);

  // The cookie carried the token's own 24 hours and vanished with it; localStorage keeps whatever it
  // is given, so the browser has to see the expiry for itself - otherwise a page renders as though
  // somebody were signed in and then fails every read with a 401.
  const hasExpiredToken = isLoginTokenExpired(storedToken);

  // Worked out during the render, so that the effect below has one plain string to watch. Watching
  // the user instead would re-run it on every render, because JSON.parse makes a new object each
  // time even when the stored text has not changed.
  let redirectPath: string | null = null;
  if (hasReadTheBrowsersStorage) {
    if (storedToken === null || currentUser === null || hasExpiredToken) {
      redirectPath = "/login";
    } else if (
      allowedRoles !== null &&
      allowedRoles.includes(currentUser.role) === false
    ) {
      redirectPath = homePathForRole(currentUser.role);
    }
  }

  useEffect(
    function sendTheVisitorAway() {
      if (redirectPath === null) {
        return;
      }

      // A token the browser can see is past its time is thrown away, so the navigation bar stops
      // showing a name and no further request carries a token the API would only refuse.
      if (hasExpiredToken) {
        clearLoginSession();
      }

      router.replace(redirectPath);
    },
    [redirectPath, hasExpiredToken, router],
  );

  const isReady =
    hasReadTheBrowsersStorage && redirectPath === null && currentUser !== null;

  // The page is only told who is signed in once the login has passed every check, so a page can
  // never draw itself for a visitor who is on their way to somewhere else.
  let allowedUser: CurrentUser | null = null;
  if (isReady) {
    allowedUser = currentUser;
  }

  return { isReady: isReady, currentUser: allowedUser };
}
