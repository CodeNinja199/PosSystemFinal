import { cookies } from "next/headers";
import { LoginTokenCookieName } from "@/lib/cookieNames";

// Called by every protected page and route handler. Returns null when there is no login cookie.
export async function readLoginTokenFromCookies(): Promise<string | null> {
  const cookieStore = await cookies();
  const loginTokenCookie = cookieStore.get(LoginTokenCookieName);
  if (loginTokenCookie === undefined) {
    return null;
  }

  return loginTokenCookie.value;
}
