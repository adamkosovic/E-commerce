import { Component, OnInit } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product/product.model';

@Component({
  selector: 'app-popular-products',
  standalone: true,
  imports: [NgFor, NgIf, RouterLink],
  templateUrl: './popular-products.component.html',
  styleUrl: './popular-products.component.css'
})
export class PopularProductsComponent implements OnInit{
  products: Product[] = [];
  loading = true;

  constructor(private productService: ProductService) {}

  ngOnInit(): void {
    this.productService.getAll().subscribe({
      next: (products) => {
        this.products = products.slice(0, 5);
        this.loading = false;
      },
      error: (err) => {
        console.error('Kunde inte hämta produkter:', err);
        this.loading = false;
      }
    });
  }
}
