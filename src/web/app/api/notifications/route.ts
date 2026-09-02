import { NextResponse } from "next/server";
import { callPosApi } from "@/lib/callPosApi";
import { readLoginTokenFromCookies } from "@/lib/readLoginTokenFromCookies";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { NotificationResponse } from "@/lib/types/NotificationResponse";

// Called by NotificationList every 30 seconds. The gateway forwards /notifications to the Notification API.
export async function GET() {
  const token = await readLoginTokenFromCookies();
  if (token === null) {
    return NextResponse.json(
      { message: "Please log in first." },
      { status: 401 },
    );
  }

  const apiResponse = await callPosApi("/notifications/notifications", {
    method: "GET",
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

  const notifications: NotificationResponse[] = await apiResponse.json();

  return NextResponse.json(notifications);
}
