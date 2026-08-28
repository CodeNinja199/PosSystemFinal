import { NextResponse } from "next/server";
import { callPosApi } from "@/lib/callPosApi";
import { readLoginTokenFromCookies } from "@/lib/readLoginTokenFromCookies";
import type { ApiErrorResponse } from "@/lib/types/ApiErrorResponse";
import type { CategoryFormData } from "@/lib/types/CategoryFormData";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";

// Called by the admin categories page to rename a category.
export async function PUT(
  request: Request,
  context: RouteContext<"/api/categories/[id]">,
) {
  const token = await readLoginTokenFromCookies();
  if (token === null) {
    return NextResponse.json(
      { message: "Please log in first." },
      { status: 401 },
    );
  }

  const parameters = await context.params;
  const categoryFormData: CategoryFormData = await request.json();

  const apiResponse = await callPosApi(`/api/categories/${parameters.id}`, {
    method: "PUT",
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

  const updatedCategory: CategoryResponse = await apiResponse.json();

  return NextResponse.json(updatedCategory);
}

// Called by the admin categories page to delete a category; the API answers 409 when it still has products.
export async function DELETE(
  request: Request,
  context: RouteContext<"/api/categories/[id]">,
) {
  const token = await readLoginTokenFromCookies();
  if (token === null) {
    return NextResponse.json(
      { message: "Please log in first." },
      { status: 401 },
    );
  }

  const parameters = await context.params;

  const apiResponse = await callPosApi(`/api/categories/${parameters.id}`, {
    method: "DELETE",
    token: token,
    body: null,
  });
  if (apiResponse.ok === false) {
    const errorBody: ApiErrorResponse = await apiResponse.json();
    return NextResponse.json(
      { message: errorBody.message },
      { status: apiResponse.status },
    );
  }

  return new NextResponse(null, { status: 204 });
}
