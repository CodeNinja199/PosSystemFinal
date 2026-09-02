// The only file that knows where the POS API is. Every page and route handler talks to the API through it.
// GATEWAY_URL comes from .env.local, which is never committed. Paths start with /pos or /notifications: the gateway picks the API.
export type CallPosApiOptions = {
  method: string;
  token: string | null;
  body: object | null;
};

export async function callPosApi(
  path: string,
  options: CallPosApiOptions,
): Promise<Response> {
  const baseUrl = process.env.GATEWAY_URL;
  if (baseUrl === undefined) {
    throw new Error("GATEWAY_URL is not configured.");
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
