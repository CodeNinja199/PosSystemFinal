import { redirect } from "next/navigation";
import { readCurrentUserFromCookies } from "@/lib/readCurrentUserFromCookies";
import type { CurrentUser } from "@/lib/types/CurrentUser";

// Called at the top of the staff and admin pages after requireLoginToken. The API enforces the role on every call anyway;
// this only keeps a customer from seeing a page whose every button would fail.
export async function requireRole(
  allowedRoles: string[],
): Promise<CurrentUser> {
  const currentUser = await readCurrentUserFromCookies();
  if (currentUser === null) {
    redirect("/login");
  }

  const isAllowed = allowedRoles.includes(currentUser.role);
  if (isAllowed === false) {
    redirect("/products");
  }

  return currentUser;
}
