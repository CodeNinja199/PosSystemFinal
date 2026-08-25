import Link from "next/link";
import { readCurrentUserFromCookies } from "@/lib/readCurrentUserFromCookies";

type NavLink = {
  href: string;
  text: string;
};

// The UI hides what a role cannot use; the API refuses the call anyway if the cookie is edited.
export async function NavBar() {
  const currentUser = await readCurrentUserFromCookies();

  const links: NavLink[] = [];
  if (currentUser === null) {
    links.push({ href: "/login", text: "Log in" });
    links.push({ href: "/register", text: "Register" });
  } else {
    links.push({ href: "/products", text: "Products" });
    links.push({ href: "/cart", text: "Cart" });
    links.push({ href: "/orders", text: "My orders" });
    if (currentUser.role === "Cashier" || currentUser.role === "Admin") {
      links.push({ href: "/admin/orders", text: "Store orders" });
    }
    if (currentUser.role === "Admin") {
      links.push({ href: "/admin", text: "Summary" });
      links.push({ href: "/admin/categories", text: "Categories" });
      links.push({ href: "/admin/products", text: "Manage products" });
      links.push({ href: "/admin/customers", text: "Customers" });
    }
  }

  const linkElements = links.map(renderNavLink);

  let userArea = null;
  if (currentUser !== null) {
    userArea = (
      <form
        action="/api/logout"
        method="post"
        className="flex items-center gap-3"
      >
        <span>{currentUser.fullName}</span>
        <button
          type="submit"
          className="border border-gray-400 px-3 py-1 hover:bg-gray-100"
        >
          Log out
        </button>
      </form>
    );
  }

  return (
    <nav className="flex items-center justify-between border-b border-gray-300 px-6 py-3">
      <div className="flex items-center gap-5">
        <Link href="/" className="font-bold text-accent">
          POS
        </Link>
        {linkElements}
      </div>
      {userArea}
    </nav>
  );
}

function renderNavLink(link: NavLink) {
  return (
    <Link key={link.href} href={link.href} className="hover:text-accent">
      {link.text}
    </Link>
  );
}
