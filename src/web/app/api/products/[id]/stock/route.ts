import { NextResponse } from "next/server";
import { callPosApi } from "@/lib/callPosApi";
import { readLoginTokenFromCookies } from "@/lib/readLoginTokenFromCookies";
import type { AdjustStockFormData } from "@/lib/types/AdjustStockFormData";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { ProductResponse } from "@/lib/types/ProductResponse";

// Called by the stock form on the admin products page. PATCH changes one thing: the stock quantity.
export async function PATCH(
  request: Request,
  context: RouteContext<"/api/products/[id]/stock">,
) {
  const token = await readLoginTokenFromCookies();
  if (token === null) {
    return NextResponse.json(
      { message: "Please log in first." },
      { status: 401 },
    );
  }

  const parameters = await context.params;
  const adjustStockFormData: AdjustStockFormData = await request.json();

  const apiResponse = await callPosApi(`/api/products/${parameters.id}/stock`, {
    method: "PATCH",
    token: token,
    body: adjustStockFormData,
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
