import { Component } from '@angular/core';
import { FavoriteProducts } from "../../components/favorite-products/favorite-products";
import { PopularProductsComponent } from "../../components/popular-products/popular-products.component";
import { ClothesNewsComponent } from "../../components/clothes-news/clothes-news.component";

@Component({
  selector: 'app-home',
  imports: [FavoriteProducts, PopularProductsComponent, ClothesNewsComponent],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home {

  shop() {
    window.location.href = '/shop';
  }
}
