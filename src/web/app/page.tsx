import { redirect } from "next/navigation";

// The home address has no screen of its own: it sends everyone to the products page, which sends a logged-out visitor on to the login page.
export default function HomePage() {
  redirect("/products");
}
