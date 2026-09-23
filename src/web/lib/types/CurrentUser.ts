// What the pos_user entry in localStorage holds: enough to greet the user and decide which links to
// show. The API enforces on its own. It is deliberately separate from the token, which is the only
// thing the API will accept as proof of anything.
export type CurrentUser = {
  fullName: string;
  role: string;
};
