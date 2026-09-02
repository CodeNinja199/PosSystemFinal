import { NextResponse } from "next/server";
import { callPosApi } from "@/lib/callPosApi";
import { readLoginTokenFromCookies } from "@/lib/readLoginTokenFromCookies";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { ProductFormData } from "@/lib/types/ProductFormData";
import type { ProductResponse } from "@/lib/types/ProductResponse";

// Called by ProductForm on the edit page to replace a product's details.
export async function PUT(
  request: Request,
  context: RouteContext<"/api/products/[id]">,
) {
  const token = await readLoginTokenFromCookies();
  if (token === null) {
    return NextResponse.json(
      { message: "Please log in first." },
      { status: 401 },
    );
  }

  const parameters = await context.params;
  const productFormData: ProductFormData = await request.json();

  const apiResponse = await callPosApi(`/pos/products/${parameters.id}`, {
    method: "PUT",
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

  const updatedProduct: ProductResponse = await apiResponse.json();

  return NextResponse.json(updatedProduct);
}

// Called by the admin products page; the API answers 409 when the product appears in an order.
export async function DELETE(
  request: Request,
  context: RouteContext<"/api/products/[id]">,
) {
  const token = await readLoginTokenFromCookies();
  if (token === null) {
    return NextResponse.json(
      { message: "Please log in first." },
      { status: 401 },
    );
  }

  const parameters = await context.params;

  const apiResponse = await callPosApi(`/pos/products/${parameters.id}`, {
    method: "DELETE",
    token: token,
    body: null,
  });
  if (apiResponse.ok === false) {
    const errorBody: ApiErrorResponse = await apiResponse.json();
    return NextResponse.json(
      { message: errorBody.message },
      { status: apiResponse.status },
    );
  }

  return new NextResponse(null, { status: 204 });
}
