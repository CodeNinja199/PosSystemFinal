import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "POS system",
  description:
    "Point of sale for the store: products, orders, and notifications.",
};

export default function RootLayout(props: LayoutProps<"/">) {
  const children = props.children;

  return (
    <html lang="en" className="h-full">
      <body className="min-h-full flex flex-col">{children}</body>
    </html>
  );
}
