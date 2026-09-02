import { NextResponse } from "next/server";
import { callPosApi } from "@/lib/callPosApi";
import { readLoginTokenFromCookies } from "@/lib/readLoginTokenFromCookies";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";

// Called by the Mark as read button. PATCH changes one thing: the read flag.
export async function PATCH(
  request: Request,
  context: RouteContext<"/api/notifications/[id]/read">,
) {
  const token = await readLoginTokenFromCookies();
  if (token === null) {
    return NextResponse.json(
      { message: "Please log in first." },
      { status: 401 },
    );
  }

  const parameters = await context.params;

  const apiResponse = await callPosApi(
    `/notifications/notifications/${parameters.id}/read`,
    { method: "PATCH", token: token, body: null },
  );
  if (apiResponse.ok === false) {
    const errorBody: ApiErrorResponse = await apiResponse.json();
    return NextResponse.json(
      { message: errorBody.message },
      { status: apiResponse.status },
    );
  }

  return new NextResponse(null, { status: 204 });
}
