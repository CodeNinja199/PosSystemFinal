// One product as GET api/products returns it.
export type ProductResponse = {
  id: number;
  name: string;
  price: number;
  stockQuantity: number;
  lowStockThreshold: number;
  imageUrl: string | null;
  categoryId: number;
};
