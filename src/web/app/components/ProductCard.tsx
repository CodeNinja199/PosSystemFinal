"use client";

import Image from "next/image";
import Link from "next/link";
import type { ProductResponse } from "@/lib/types/ProductResponse";
import { AddToCartButton } from "@/app/components/AddToCartButton";

type ProductCardProps = {
  product: ProductResponse;
  categoryName: string;
};

// One card in the product grid. Rendered by ProductGrid for every product that matches the search.
export function ProductCard(props: ProductCardProps) {
  const product = props.product;
  const categoryName = props.categoryName;

  let imageElement = (
    <div className="flex h-40 items-center justify-center bg-gray-100 text-gray-500">
      No image
    </div>
  );
  if (product.imageUrl !== null) {
    imageElement = (
      <Image
        src={product.imageUrl}
        alt={product.name}
        width={320}
        height={160}
        unoptimized
        className="h-40 w-full object-cover"
      />
    );
  }

  let stockText = `${product.stockQuantity} in stock`;
  if (product.stockQuantity === 0) {
    stockText = "Out of stock";
  }

  return (
    <li className="flex flex-col gap-1 border border-gray-300 p-4">
      {imageElement}
      <Link
        href={`/products/${product.id}`}
        className="font-bold hover:text-accent"
      >
        {product.name}
      </Link>
      <p className="text-gray-700">{categoryName}</p>
      <p>Rs {product.price}</p>
      <p>{stockText}</p>
      <AddToCartButton product={product} />
    </li>
  );
}
