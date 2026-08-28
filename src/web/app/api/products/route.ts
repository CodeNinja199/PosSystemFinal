import { NextResponse } from "next/server";
import { callPosApi } from "@/lib/callPosApi";
import { readLoginTokenFromCookies } from "@/lib/readLoginTokenFromCookies";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { ProductFormData } from "@/lib/types/ProductFormData";
import type { ProductResponse } from "@/lib/types/ProductResponse";

// Called by ProductForm on the admin products page to add a product.
export async function POST(request: Request) {
  const token = await readLoginTokenFromCookies();
  if (token === null) {
    return NextResponse.json(
      { message: "Please log in first." },
      { status: 401 },
    );
  }

  const productFormData: ProductFormData = await request.json();

  const apiResponse = await callPosApi("/api/products", {
    method: "POST",
    token: token,
    body: productFormData,
  });
  if (apiResponse.ok === false) {
    const errorBody: ApiErrorResponse = await apiResponse.json();
    return NextResponse.json(
      { message: errorBody.message },
      { status: apiResponse.status },
    );
  }

  const createdProduct: ProductResponse = await apiResponse.json();

  return NextResponse.json(createdProduct, { status: 201 });
}
