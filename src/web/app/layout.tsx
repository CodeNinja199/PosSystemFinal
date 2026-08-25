import type { Metadata } from "next";
import "./globals.css";
import { NavBar } from "@/app/components/NavBar";

export const metadata: Metadata = {
  title: "POS system",
  description:
    "Point of sale for the store: products, orders, and notifications.",
};

export default function RootLayout(props: LayoutProps<"/">) {
  const children = props.children;

  return (
    <html lang="en" className="h-full">
      <body className="min-h-full flex flex-col">
        <NavBar />
        <main className="mx-auto w-full max-w-5xl px-6 py-6">{children}</main>
      </body>
    </html>
  );
}
