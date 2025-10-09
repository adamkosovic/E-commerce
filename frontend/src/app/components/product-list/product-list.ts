import { Component } from '@angular/core';
import { Product } from '../../models/product/product.model';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';


@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-list.html',
  styleUrl: './product-list.css'
})
export class ProductList {

  constructor(private router: Router) {}

  goToProductDetail(id: string) {
    this.router.navigate(['/shop', id]);
  }

  trackByProduct = (index: number, product: Product ) => product.id;


  products: Product[] = [
    {
      id: '1',
      name: 'Eco Tee',
      price: 99,
      imageUrl: 'https://picsum.photos/seed/eco-tee/400/300',
      categories: ['men' , 'sale']
    },
    {
      id: '2',
      name: 'Classic Shirt',
      price: 149,
      imageUrl: 'https://picsum.photos/seed/classic-shirt/400/300',
      categories: ['men', 'sale']
    },
    {
      id: '3',
      name: 'Classic Shirt',
      price: 149,
      imageUrl: 'https://picsum.photos/seed/linen-dress/400/300',
      categories: ['men', 'sale']
    },
    {
      id: '4',
      name: 'Classic Shirt',
      price: 149,
      imageUrl: 'https://picsum.photos/seed/kids-hoodie/400/300',
      categories: ['men', 'sale']
    },
    {
      id: '5',
      name: 'Classic Shirt',
      price: 149,
      imageUrl: 'https://picsum.photos/seed/kids-hoodie/400/300',
      categories: ['men', 'sale']
    },
    {
      id: '6',
      name: 'Classic Shirt',
      price: 149,
      imageUrl: 'https://picsum.photos/seed/kids-hoodie/400/300',
      categories: ['men', 'sale']
    },
    {
      id: '7',
      name: 'Classic Shirt',
      price: 149,
      imageUrl: 'https://picsum.photos/seed/kids-hoodie/400/300',
      categories: ['men', 'sale']
    },
    {
      id: '8',
      name: 'Classic Shirt',
      price: 149,
      imageUrl: 'https://picsum.photos/seed/kids-hoodie/400/300',
      categories: ['men', 'sale']
    },
  ]
}
