import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { Product } from '../../../../models/product/product.model';
import { ProductService } from '../../../../product.service';
import { CommonModule, AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-admin-product-list',
  standalone: true,
  imports: [CommonModule , AsyncPipe],
  templateUrl: './admin-product-list.html',
  styleUrl: './admin-product-list.css'
})
export class AdminProductList {
  products$!: Observable<Product[]>;

  constructor(private productService: ProductService) {}

  ngOnInit(): void {
    this.products$ = this.productService.getAll();
  }
}
