import { NextResponse } from "next/server";
import { callPosApi } from "@/lib/callPosApi";
import { CurrentUserCookieName, LoginTokenCookieName } from "@/lib/cookieNames";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { CurrentUser } from "@/lib/types/CurrentUser";
import type { LoginFormData } from "@/lib/types/LoginFormData";
import type { LoginResponse } from "@/lib/types/LoginResponse";

const OneDayInSeconds = 60 * 60 * 24;

// Called by the login page. The token never reaches the browser's JavaScript: it goes into an httpOnly cookie here.
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

  const loginResponse: LoginResponse = await apiResponse.json();
  const currentUser: CurrentUser = {
    fullName: loginResponse.fullName,
    role: loginResponse.role,
  };
  const isProduction = process.env.NODE_ENV === "production";

  const response = NextResponse.json(currentUser);
  response.cookies.set(LoginTokenCookieName, loginResponse.token, {
    httpOnly: true,
    sameSite: "lax",
    secure: isProduction,
    path: "/",
    maxAge: OneDayInSeconds,
  });
  response.cookies.set(CurrentUserCookieName, JSON.stringify(currentUser), {
    httpOnly: false,
    sameSite: "lax",
    secure: isProduction,
    path: "/",
    maxAge: OneDayInSeconds,
  });

  return response;
}
