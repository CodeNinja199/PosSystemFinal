import { NextResponse } from "next/server";
import { callPosApi } from "@/lib/callPosApi";
import { readLoginTokenFromCookies } from "@/lib/readLoginTokenFromCookies";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { CategoryFormData } from "@/lib/types/CategoryFormData";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";

// Called by the admin categories page to add a category.
export async function POST(request: Request) {
  const token = await readLoginTokenFromCookies();
  if (token === null) {
    return NextResponse.json(
      { message: "Please log in first." },
      { status: 401 },
    );
  }

  const categoryFormData: CategoryFormData = await request.json();

  const apiResponse = await callPosApi("/api/categories", {
    method: "POST",
    token: token,
    body: categoryFormData,
  });
  if (apiResponse.ok === false) {
    const errorBody: ApiErrorResponse = await apiResponse.json();
    return NextResponse.json(
      { message: errorBody.message },
      { status: apiResponse.status },
    );
  }

  const createdCategory: CategoryResponse = await apiResponse.json();

  return NextResponse.json(createdCategory, { status: 201 });
}
