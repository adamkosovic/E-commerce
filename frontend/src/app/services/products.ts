import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private products = [
    {
      id: '1',
      name: 'Eco Tee',
      price: 99,
      description: 'A trendy eco-friendly t-shirt made from organic cotton. Perfect for casual wear and promoting sustainability.',
      imageUrl: 'https://picsum.photos/seed/eco-tee/400/300',
      categories: ['men' , 'sale']
    },
    {
      id: '2',
      name: 'Classic Shirt',
      price: 149,
      description: 'A trendy eco-friendly t-shirt made from organic cotton. Perfect for casual wear and promoting sustainability.',
      imageUrl: 'https://picsum.photos/seed/classic-shirt/400/300',
      categories: ['men', 'sale']
    },
    {
      id: '3',
      name: 'Classic Shirt',
      price: 149,
      description: 'A trendy eco-friendly t-shirt made from organic cotton. Perfect for casual wear and promoting sustainability.',
      imageUrl: 'https://picsum.photos/seed/linen-dress/400/300',
      categories: ['men', 'sale']
    },
    {
      id: '4',
      name: 'Classic Shirt',
      price: 149,
      description: 'Revamp your style with the latest designer trends in men’s clothing or achieve a perfectly curated wardrobe thanks to our line-up of timeless pieces.',
      imageUrl: 'https://picsum.photos/seed/kids-hoodie/400/300',
      categories: ['men', 'sale']
    },
    {
      id: '5',
      name: 'Classic Shirt',
      price: 149,
      description: 'Revamp your style with the latest designer trends in men’s clothing or achieve a perfectly curated wardrobe thanks to our line-up of timeless pieces. ',
      imageUrl: 'https://picsum.photos/seed/kids-hoodie/400/300',
      categories: ['men', 'sale']
    },
    {
      id: '6',
      name: 'Classic Shirt',
      price: 149,
      description: 'Revamp your style with the latest designer trends in men’s clothing or achieve a perfectly curated wardrobe thanks to our line-up of timeless pieces. ',
      imageUrl: 'https://picsum.photos/seed/kids-hoodie/400/300',
      categories: ['men', 'sale']
    },
    {
      id: '7',
      name: 'Classic Shirt',
      price: 149,
      description: 'Revamp your style with the latest designer trends in men’s clothing or achieve a perfectly curated wardrobe thanks to our line-up of timeless pieces. ',
      imageUrl: 'https://picsum.photos/seed/kids-hoodie/400/300',
      categories: ['men', 'sale']
    },
    {
      id: '8',
      name: 'Classic Shirt',
      price: 149,
      description: 'Revamp your style with the latest designer trends in men’s clothing or achieve a perfectly curated wardrobe thanks to our line-up of timeless pieces. ',
      imageUrl: 'https://picsum.photos/seed/kids-hoodie/400/300',
      categories: ['men', 'sale']
    },
  ]



  getAll () {
    return this.products;
  }

  getById (id: string) {
    return this.products.find(product => product.id === id);
  }
}
