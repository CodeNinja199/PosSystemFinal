import { cookies } from "next/headers";
import { CurrentUserCookieName } from "@/lib/cookieNames";
import type { CurrentUser } from "@/lib/types/CurrentUser";

// Called by NavBar and by pages that show something only to one role. Returns null when nobody is logged in.
export async function readCurrentUserFromCookies(): Promise<CurrentUser | null> {
  const cookieStore = await cookies();
  const currentUserCookie = cookieStore.get(CurrentUserCookieName);
  if (currentUserCookie === undefined) {
    return null;
  }

  try {
    const currentUser: CurrentUser = JSON.parse(currentUserCookie.value);
    return currentUser;
  } catch {
    return null;
  }
}
