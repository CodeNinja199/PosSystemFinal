import { NextResponse } from "next/server";
import { callPosApi } from "@/lib/callPosApi";
import { readBearerToken } from "@/lib/readBearerToken";

// Why this route exists:
// 1. The token lives in localStorage now, so the pages read it in the browser and must fetch from the browser too.
// 2. The browser still must not call the gateway directly (requirement 46), so every read comes through
//    here and this server forwards it. That is a rule this app keeps, not something the network enforces:
//    compose publishes the gateway on :5000 for Swagger and for curl, so nothing stops a page breaking it.
// 3. It forwards the caller's own token and nothing else, so it grants no access of its own: the API still
//    checks the signature, the role and the store on every single request.
// Only GET, and only the two paths the gateway actually serves, so this cannot be used to reach anything else.
const AllowedPathPrefixes = ["pos", "notifications"];

export async function GET(
  request: Request,
  context: { params: Promise<{ path: string[] }> },
) {
  const parameters = await context.params;
  const pathSegments = parameters.path;

  if (
    pathSegments.length === 0 ||
    AllowedPathPrefixes.includes(pathSegments[0]) === false
  ) {
    return NextResponse.json({ message: "Not found." }, { status: 404 });
  }

  const token = readBearerToken(request);
  if (token === null) {
    return NextResponse.json(
      { message: "A valid login token is required." },
      { status: 401 },
    );
  }

  // The query string is carried through as well, so a filter like ?categoryId=3 keeps working.
  const queryString = new URL(request.url).search;
  const apiPath = `/${pathSegments.join("/")}${queryString}`;

  const apiResponse = await callPosApi(apiPath, {
    method: "GET",
    token: token,
    body: null,
  });

  // The API's status and body are passed back untouched, so the { message } shape of every error survives.
  const bodyText = await apiResponse.text();
  return new NextResponse(bodyText, {
    status: apiResponse.status,
    headers: { "Content-Type": "application/json" },
  });
}
