import { NextResponse } from "next/server";
import { callPosApi } from "@/lib/callPosApi";
import { readBearerToken } from "@/lib/readBearerToken";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { NotificationResponse } from "@/lib/types/NotificationResponse";

// Called by NotificationList every 30 seconds. The gateway forwards /notifications to the Notification API.
// The request is now a parameter because the token arrives in its Authorization header.
export async function GET(request: Request) {
  const token = readBearerToken(request);
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
