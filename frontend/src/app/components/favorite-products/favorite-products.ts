import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Observable, combineLatest, map } from 'rxjs';
import { FavoritesService } from '../../services/favorites.service';
import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product/product.model';

@Component({
  selector: 'app-favorite-products',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './favorite-products.html',
  styleUrls: ['./favorite-products.css']
})
export class FavoriteProducts implements OnInit {
  favoriteProducts$!: Observable<Product[]>;

  constructor(
    private favoritesService: FavoritesService,
    private productsService: ProductService,
  ) {}

  ngOnInit() {
    // starta laddning av favoriter (om din service behöver det)
    this.favoritesService.loadFavorites();

    this.favoriteProducts$ = combineLatest([
      this.productsService.getAll(),          // Observable<Product[]>
      this.favoritesService.favorites$       // Observable<string[]> eller Guid[]
    ]).pipe(
      map(([allProducts, favIds]) =>
        allProducts.filter(p => favIds.includes(p.id))
      )
    );
  }

  removeFavorite(id: string) {
    this.favoritesService.toggleFavorite(id);
  }

  // om du vill använda trackBy i HTML
  trackById(index: number, product: Product) {
    return product.id;
  }
}
