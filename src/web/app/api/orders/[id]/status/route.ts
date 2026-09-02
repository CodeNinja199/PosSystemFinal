import { NextResponse } from "next/server";
import { callPosApi } from "@/lib/callPosApi";
import { readLoginTokenFromCookies } from "@/lib/readLoginTokenFromCookies";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { OrderResponse } from "@/lib/types/OrderResponse";
import type { UpdateOrderStatusRequest } from "@/lib/types/UpdateOrderStatusRequest";

// Called by OrderStatusButtons on the store orders page. Forwards the new status with the token; the API checks the role.
export async function PATCH(
  request: Request,
  context: RouteContext<"/api/orders/[id]/status">,
) {
  const token = await readLoginTokenFromCookies();
  if (token === null) {
    return NextResponse.json(
      { message: "Please log in first." },
      { status: 401 },
    );
  }

  const parameters = await context.params;
  const orderId = parameters.id;
  const updateOrderStatusRequest: UpdateOrderStatusRequest =
    await request.json();

  const apiResponse = await callPosApi(`/pos/orders/${orderId}/status`, {
    method: "PATCH",
    token: token,
    body: updateOrderStatusRequest,
  });

  if (apiResponse.ok === false) {
    const errorBody: ApiErrorResponse = await apiResponse.json();
    return NextResponse.json(
      { message: errorBody.message },
      { status: apiResponse.status },
    );
  }

  const updatedOrder: OrderResponse = await apiResponse.json();

  return NextResponse.json(updatedOrder);
}
