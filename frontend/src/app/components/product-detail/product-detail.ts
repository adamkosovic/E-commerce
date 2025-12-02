import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError, switchMap, map, tap } from 'rxjs/operators';
import { ProductService } from '../../product.service';
import { Product } from '../../models/product/product.model';
import { CartService } from '../../services/cart/cart.service';


@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-detail.html',
  styleUrls: ['./product-detail.css']
})
export class ProductDetailComponent {
  // Use a single Product for a detail page
  product$: Observable<Product | null>;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService,
    private cartService: CartService
  ) {
    this.product$ = this.route.paramMap.pipe(
      map(params => params.get('id')),
      switchMap(id => {
        if (!id) {
          this.router.navigate(['/shop']);
          return of(null);
        }
        return this.productService.getById(id).pipe(
          catchError(() => {
            this.router.navigate(['/shop']);
            return of(null);
          })
        );
      })
    );
  }

  addToCart(product: Product) {
    this.cartService.addItem(product, 1);
  }

  goToCart() {
    this.router.navigate(['/cart']);
  }
}
