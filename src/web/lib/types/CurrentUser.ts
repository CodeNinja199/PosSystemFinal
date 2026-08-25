// What the readable pos_user cookie holds: enough to greet the user and decide which links to show. The API enforces on its own.
export type CurrentUser = {
  fullName: string;
  role: string;
};
