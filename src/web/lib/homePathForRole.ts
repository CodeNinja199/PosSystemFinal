// Called after login, by the home page, and by useRequireLogin: the first page of each role. Admins manage; everyone else sells or buys.
export function homePathForRole(role: string): string {
  if (role === "Admin") {
    return "/admin";
  }

  return "/products";
}
