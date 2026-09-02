import { NextResponse } from "next/server";
import { callPosApi } from "@/lib/callPosApi";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { RegisterFormData } from "@/lib/types/RegisterFormData";

// Called by the register page. It forwards the form to the API; the user logs in afterwards, so no cookie is set here.
export async function POST(request: Request) {
  const registerFormData: RegisterFormData = await request.json();

  const apiResponse = await callPosApi("/pos/auth/register", {
    method: "POST",
    token: null,
    body: registerFormData,
  });

  if (apiResponse.ok === false) {
    const errorBody: ApiErrorResponse = await apiResponse.json();
    return NextResponse.json(
      { message: errorBody.message },
      { status: apiResponse.status },
    );
  }

  const registeredUser = await apiResponse.json();

  return NextResponse.json(registeredUser, { status: 201 });
}
