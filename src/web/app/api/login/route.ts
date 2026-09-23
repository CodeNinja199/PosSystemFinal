import { NextResponse } from "next/server";
import { callPosApi } from "@/lib/callPosApi";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { LoginFormData } from "@/lib/types/LoginFormData";
import type { LoginResponse } from "@/lib/types/LoginResponse";

// Called by the login page. It no longer sets any cookie: the token is handed back in the body and the
// login page puts it into localStorage itself. This route stays in place so the browser still never has
// to know the gateway's address.
export async function POST(request: Request) {
  const loginFormData: LoginFormData = await request.json();

  const apiResponse = await callPosApi("/pos/auth/login", {
    method: "POST",
    token: null,
    body: loginFormData,
  });

  if (apiResponse.ok === false) {
    const errorBody: ApiErrorResponse = await apiResponse.json();
    return NextResponse.json(
      { message: errorBody.message },
      { status: apiResponse.status },
    );
  }

  // The token, the full name and the role, exactly as the API returned them.
  const loginResponse: LoginResponse = await apiResponse.json();

  return NextResponse.json(loginResponse);
}
