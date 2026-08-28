// One customer as GET api/customers returns it. There is no password hash here and never will be.
export type UserResponse = {
  id: number;
  fullName: string;
  email: string;
  role: string;
  registeredAt: string;
};
