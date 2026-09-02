import { callPosApi } from "@/lib/callPosApi";
import { requireLoginToken } from "@/lib/requireLoginToken";
import { requireRole } from "@/lib/requireRole";
import type { CategoryResponse } from "@/lib/types/CategoryResponse";
import { CategoryManager } from "@/app/components/CategoryManager";

export default async function AdminCategoriesPage() {
  const token = await requireLoginToken();
  await requireRole(["Admin"]);

  const categoriesResponse = await callPosApi("/pos/categories", {
    method: "GET",
    token: token,
    body: null,
  });
  const categories: CategoryResponse[] = await categoriesResponse.json();

  return (
    <div>
      <h1 className="mb-4 text-2xl font-bold">Categories</h1>
      <CategoryManager categories={categories} />
    </div>
  );
}
