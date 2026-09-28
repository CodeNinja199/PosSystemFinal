import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

// How the Content Security Policy works here (requirement 65):
// 1. Every page request passes through this proxy first. It makes a fresh random nonce - a one-time
//    password - and sends the browser a policy that says "only run scripts carrying this password".
// 2. The same policy is also put on the request, because that is where Next.js looks while it
//    renders the page: it reads the nonce out of the policy and stamps it on every script of its own.
// 3. A script that reaches the page any other way - hidden in a product name, say - cannot know the
//    password, which is different on every request, so the browser refuses to run it. That matters
//    now that the login token lives in localStorage, where any script on the page could read it.
// 4. A page built once in advance would carry no password at all, so the root layout waits for the
//    request (connection()) and every page is rendered when it is asked for.
// Learned from: https://nextjs.org/docs/app/guides/content-security-policy
export function proxy(request: NextRequest): NextResponse {
  const nonce = Buffer.from(crypto.randomUUID()).toString("base64");
  const policy = buildContentSecurityPolicy(nonce);

  const requestHeaders = new Headers(request.headers);
  requestHeaders.set("Content-Security-Policy", policy);

  const response = NextResponse.next({
    request: { headers: requestHeaders },
  });
  response.headers.set("Content-Security-Policy", policy);

  return response;
}

function buildContentSecurityPolicy(nonce: string): string {
  // React uses eval in development only, to rebuild server error stacks in the browser; the Docker
  // image runs in production, where nothing needs it.
  const isDevelopment = process.env.NODE_ENV === "development";
  let developmentOnlyScriptSource = "";
  if (isDevelopment) {
    developmentOnlyScriptSource = " 'unsafe-eval'";
  }

  const directives: string[] = [
    // Anything not named below may only come from this site.
    "default-src 'self'",
    // Only scripts carrying this request's nonce run. 'strict-dynamic' lets those trusted scripts load
    // the page's other chunks, so the nonce does not have to be written onto every one of them.
    `script-src 'self' 'nonce-${nonce}' 'strict-dynamic'${developmentOnlyScriptSource}`,
    // next/image puts a small inline style on every picture (color: transparent). A nonce cannot
    // cover a style attribute, so inline styles are allowed; a style cannot read the token.
    "style-src 'self' 'unsafe-inline'",
    // A product image is a link to any site an admin chose, and the API accepts http as well as https
    // links, so pictures may come from any web address as well as from here. Loading a picture cannot
    // run a script, so this does not weaken script-src.
    "img-src 'self' http: https: data: blob:",
    "font-src 'self'",
    // The browser only ever calls this web app's own /api routes, so fetch and XHR are limited to
    // this site. This is not what keeps the token safe: a script that did run could still send it
    // away inside a picture's address, or by moving the page to another site. script-src is the
    // real protection, by making sure no such script ever runs.
    "connect-src 'self'",
    "object-src 'none'",
    "base-uri 'self'",
    "form-action 'self'",
    // No other site may show these pages inside a frame, which stops clickjacking.
    "frame-ancestors 'none'",
  ];

  // upgrade-insecure-requests is left out on purpose: the Docker site is served over plain http on
  // localhost, and upgrading its own requests to https would break it. It belongs with HTTPS when
  // this is deployed.
  return directives.join("; ");
}

// The built files under _next and link prefetches do not need the policy. /api is deliberately NOT
// skipped: the route handlers there answer JSON, where the header is harmless, but an unknown address
// such as /api/nonsense answers with the app's full HTML 404 page, and that page needs the policy too.
export const config = {
  matcher: [
    {
      source: "/((?!_next/static|_next/image|favicon.ico).*)",
      missing: [
        { type: "header", key: "next-router-prefetch" },
        { type: "header", key: "purpose", value: "prefetch" },
      ],
    },
  ],
};
