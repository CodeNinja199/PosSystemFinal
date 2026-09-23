"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { homePathForRole } from "@/lib/homePathForRole";
import { readCurrentUser } from "@/lib/loginTokenStorage";

// The home address has no screen of its own: it sends each role to its first page, and a logged-out visitor to the products page, which sends them on to log in.
// A client component because the stored user now lives in localStorage, which the Next.js server cannot read, so the decision has to wait for an effect in the browser.
export default function HomePage() {
  const router = useRouter();

  useEffect(
    function sendTheVisitorOnwards() {
      const currentUser = readCurrentUser();
      if (currentUser === null) {
        router.replace("/products");
        return;
      }

      router.replace(homePathForRole(currentUser.role));
    },
    [router],
  );

  // This address never shows anything of its own, so there is nothing to draw while the effect decides where to send the visitor.
  return null;
}
