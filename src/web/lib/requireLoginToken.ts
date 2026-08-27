import { redirect } from "next/navigation";
import { readLoginTokenFromCookies } from "@/lib/readLoginTokenFromCookies";

// Called at the top of every protected page: returns the token, or sends the browser to the login page and never returns.
export async function requireLoginToken(): Promise<string> {
  const token = await readLoginTokenFromCookies();
  if (token === null) {
    redirect("/login");
  }

  return token;
}
