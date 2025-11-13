/**
 * Represents a product in the application.
 */

export interface Product {
  id: string;
  title: string;
  description?: string;
  price: number;
  imageUrl: string;
}