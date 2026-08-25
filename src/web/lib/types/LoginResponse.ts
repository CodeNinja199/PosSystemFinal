// What POST api/auth/login returns.
export type LoginResponse = {
  token: string;
  fullName: string;
  role: string;
};
