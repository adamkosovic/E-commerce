import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product/product.model';
import { FavoritesService } from '../../services/favorites.service';



@Component({
  selector: 'app-product-list-component',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './product-list.html',
  styleUrls: ['./product-list.css']
})
export class ProductListComponent implements OnInit{
  products$: Observable<Product[]>;
  errorMessage: string | null = null;

  constructor(
    private productService: ProductService,
    private favoritesService: FavoritesService
    ){
    this.products$ = this.productService.getAll().pipe(
      tap(() => {
        console.log('Products loaded successfully');
        this.errorMessage = null;
      }),
      catchError((error) => {
        console.error('Error loading products:', error);
        this.errorMessage = `Failed to load products: ${error.message || 'Unknown error'}`;
        // Return empty array so the UI shows "Inga produkter hittades" instead of crashing
        return of([]);
      })
    );
  }

  ngOnInit() {
    this.favoritesService.loadFavorites();  // 👈 viktigt
  }

  trackByProductId(index: number, product: Product): string {
    return product.id;
  }


  toggleFavorite(productId: string) {
    this.favoritesService.toggleFavorite(productId);
  }

  isFavorite(productId: string): boolean {
    return this.favoritesService.isFavorite(productId);
  }
}
