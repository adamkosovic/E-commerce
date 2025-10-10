import { Component, OnInit } from '@angular/core';
import { Product } from '../../models/product/product.model';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ProductService } from '../../services/products';



@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-list.html',
  styleUrl: './product-list.css'
})
export class ProductList implements OnInit {
  products: Product[] = [];

  constructor (private productService: ProductService, private router: Router) {}

  
  ngOnInit() {
    this.products = this.productService.getAll();
  }
  
  goToProduct(id: string) {
    this.router.navigate(['/shop', id]);
  }
  

  trackByProductId(index: number, product: Product): string {
    return product.id;
  }
}