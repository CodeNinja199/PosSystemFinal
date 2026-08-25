import { NextResponse } from "next/server";
import { CurrentUserCookieName, LoginTokenCookieName } from "@/lib/cookieNames";

// Called by the logout form in NavBar. Deleting the two cookies is all a logout is: the API keeps no session.
export async function POST(request: Request) {
  const loginPageUrl = new URL("/login", request.url);

  const response = NextResponse.redirect(loginPageUrl, 303);
  response.cookies.delete(LoginTokenCookieName);
  response.cookies.delete(CurrentUserCookieName);

  return response;
}
