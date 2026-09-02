import { NextResponse } from "next/server";
import { callPosApi } from "@/lib/callPosApi";
import { readLoginTokenFromCookies } from "@/lib/readLoginTokenFromCookies";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { OrderResponse } from "@/lib/types/OrderResponse";
import type { PlaceOrderRequest } from "@/lib/types/PlaceOrderRequest";

// Called by the checkout form. Adds the token from the cookie and forwards the order to POST api/orders.
export async function POST(request: Request) {
  const token = await readLoginTokenFromCookies();
  if (token === null) {
    return NextResponse.json(
      { message: "Please log in first." },
      { status: 401 },
    );
  }

  const placeOrderRequest: PlaceOrderRequest = await request.json();

  const apiResponse = await callPosApi("/pos/orders", {
    method: "POST",
    token: token,
    body: placeOrderRequest,
  });

  if (apiResponse.ok === false) {
    const errorBody: ApiErrorResponse = await apiResponse.json();
    return NextResponse.json(
      { message: errorBody.message },
      { status: apiResponse.status },
    );
  }

  const placedOrder: OrderResponse = await apiResponse.json();

  return NextResponse.json(placedOrder, { status: 201 });
}
