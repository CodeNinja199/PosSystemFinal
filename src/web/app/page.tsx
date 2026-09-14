import { redirect } from "next/navigation";
import { homePathForRole } from "@/lib/homePathForRole";
import { readCurrentUserFromCookies } from "@/lib/readCurrentUserFromCookies";

// The home address has no screen of its own: it sends each role to its first page, and a logged-out visitor to the products page, which sends them on to log in.
export default async function HomePage() {
  const currentUser = await readCurrentUserFromCookies();
  if (currentUser === null) {
    redirect("/products");
  }

  redirect(homePathForRole(currentUser.role));
}
