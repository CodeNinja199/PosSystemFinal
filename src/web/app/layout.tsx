import type { Metadata } from "next";
import { connection } from "next/server";
import "./globals.css";
import { CartToast } from "@/app/components/CartToast";
import { NavBar } from "@/app/components/NavBar";
import { StoreProvider } from "@/app/StoreProvider";

export const metadata: Metadata = {
  title: "POS system",
  description:
    "Point of sale for the store: products, orders, and notifications.",
};

export default async function RootLayout(props: LayoutProps<"/">) {
  const children = props.children;

  // proxy.ts makes a fresh nonce for every request, and a page built once in advance would carry
  // none, so the browser would refuse all of its scripts. Waiting for the request here makes every
  // page below this layout render when it is asked for (requirement 65).
  await connection();

  return (
    <html lang="en" className="h-full">
      <body className="min-h-full flex flex-col">
        <StoreProvider>
          <NavBar />
          <main className="mx-auto w-full max-w-5xl px-6 py-6">{children}</main>
          <CartToast />
        </StoreProvider>
      </body>
    </html>
  );
}
