// The only file that knows where the POS API is. Every page and route handler talks to the API through it.
// POS_API_URL comes from .env.local, which is never committed; in Phase 9 it becomes GATEWAY_URL and points at the gateway.
export type CallPosApiOptions = {
  method: string;
  token: string | null;
  body: object | null;
};

export async function callPosApi(
  path: string,
  options: CallPosApiOptions,
): Promise<Response> {
  const baseUrl = process.env.POS_API_URL;
  if (baseUrl === undefined) {
    throw new Error("POS_API_URL is not configured.");
  }

  const headers: Record<string, string> = {};
  headers["Content-Type"] = "application/json";
  if (options.token !== null) {
    headers["Authorization"] = `Bearer ${options.token}`;
  }

  let bodyText: string | undefined = undefined;
  if (options.body !== null) {
    bodyText = JSON.stringify(options.body);
  }

  const response = await fetch(`${baseUrl}${path}`, {
    method: options.method,
    headers: headers,
    body: bodyText,
    cache: "no-store",
  });

  return response;
}
