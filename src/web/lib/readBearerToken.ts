// Called by every route handler. The browser now sends the token in the Authorization header,
// read out of localStorage, instead of the server reading it from an httpOnly cookie.
// Returns null when the header is missing or malformed, so the handler can answer 401 itself.
export function readBearerToken(request: Request): string | null {
  const header = request.headers.get("Authorization");
  if (header === null) {
    return null;
  }

  const prefix = "Bearer ";
  if (header.startsWith(prefix) === false) {
    return null;
  }

  const token = header.slice(prefix.length).trim();
  if (token.length === 0) {
    return null;
  }

  return token;
}
