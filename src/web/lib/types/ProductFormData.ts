// What the product form posts: the same shape for adding (POST) and editing (PUT).
export type ProductFormData = {
  name: string;
  price: number;
  stockQuantity: number;
  lowStockThreshold: number;
  imageUrl: string | null;
  categoryId: number;
};
