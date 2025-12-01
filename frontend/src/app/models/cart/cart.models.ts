export interface CartItemDto {
  id: string;
  productId: string; 
  title: string;
  price: number;
  qty: number;
}

export interface CartDto {
  id: string;
  items: CartItemDto[];
  totalMinor: number;
}