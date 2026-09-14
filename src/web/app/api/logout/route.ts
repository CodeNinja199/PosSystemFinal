import { NextResponse } from "next/server";
import { CurrentUserCookieName, LoginTokenCookieName } from "@/lib/cookieNames";

// Called by the logout form in NavBar. Deleting the two cookies is all a logout is: the API keeps no session.
// The Location header is a relative path, which RFC 7231 allows: in Docker the server is bound to 0.0.0.0 and
// cannot know the address the browser used, so the browser resolves "/login" against that address itself.
export async function POST() {
  const response = new NextResponse(null, {
    status: 303,
    headers: {
      Location: "/login",
    },
  });

  response.cookies.delete(LoginTokenCookieName);
  response.cookies.delete(CurrentUserCookieName);

  return response;
}
