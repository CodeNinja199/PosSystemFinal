"use client";

import { useSyncExternalStore } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useDispatch } from "react-redux";
import { clearCart } from "@/lib/cartSlice";
import type { AppDispatch } from "@/lib/store";
import {
  clearLoginSession,
  parseCurrentUser,
  readNothingWhilePrerendering,
  readStoredCurrentUserText,
  subscribeToLoginSession,
} from "@/lib/loginTokenStorage";

type NavLink = {
  href: string;
  text: string;
};

// Each role gets only the links it uses. The UI hides what a role cannot use; the API refuses the call anyway.
//
// This is a client component because the signed-in user now lives in localStorage, which only the browser can
// read. The bar therefore draws the logged-out links first and the role's links a moment later, once the
// browser has taken the page over.
export function NavBar() {
  const router = useRouter();
  const dispatch = useDispatch<AppDispatch>();

  // Reads localStorage and then listens for changes to it, so logging in or out - here or in
  // another tab - updates the bar without a full page load.
  const storedUserText = useSyncExternalStore(
    subscribeToLoginSession,
    readStoredCurrentUserText,
    readNothingWhilePrerendering,
  );
  const currentUser = parseCurrentUser(storedUserText);

  function handleLogOutButtonClick() {
    // Clearing the stored login is the whole of logging out now that there is no cookie for the
    // server to expire. It announces the change, which is what empties this bar.
    clearLoginSession();

    // Logging out used to be a form post that reloaded the document, and the cart went with the rest
    // of the page. A client-side navigation keeps the store alive, so the cart is emptied by hand: on
    // a shared till the next cashier must not inherit the last one's lines.
    dispatch(clearCart());
    router.push("/login");
  }

  const links: NavLink[] = [];
  if (currentUser === null) {
    links.push({ href: "/login", text: "Log in" });
    links.push({ href: "/register", text: "Register" });
  } else if (currentUser.role === "Customer") {
    links.push({ href: "/products", text: "Products" });
    links.push({ href: "/cart", text: "Cart" });
    links.push({ href: "/orders", text: "My orders" });
    links.push({ href: "/notifications", text: "Notifications" });
  } else if (currentUser.role === "Cashier") {
    links.push({ href: "/products", text: "New sale" });
    links.push({ href: "/cart", text: "Cart" });
    links.push({ href: "/admin/orders", text: "Store orders" });
  } else if (currentUser.role === "Admin") {
    links.push({ href: "/admin", text: "Summary" });
    links.push({ href: "/admin/orders", text: "Store orders" });
    links.push({ href: "/admin/products", text: "Manage products" });
    links.push({ href: "/admin/categories", text: "Categories" });
    links.push({ href: "/admin/customers", text: "Customers" });
    links.push({ href: "/notifications", text: "Notifications" });
  }

  const linkElements = links.map(renderNavLink);

  let userArea = null;
  if (currentUser !== null) {
    userArea = (
      <div className="flex items-center gap-3">
        <span>{currentUser.fullName}</span>
        <button
          type="button"
          onClick={handleLogOutButtonClick}
          className="border border-gray-400 px-3 py-1 hover:bg-gray-100"
        >
          Log out
        </button>
      </div>
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
