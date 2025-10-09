import { Component } from '@angular/core';
import { ProductList } from "../../components/product-list/product-list";
import { SortBy } from "../../components/sort-by/sort-by";
import { CategoryMenu } from "../../components/category-menu/category-menu";
import { Filters } from "../../components/filters/filters";


@Component({
  selector: 'app-shop',
  imports: [ProductList, SortBy, CategoryMenu, Filters],
  templateUrl: './shop.html',
  styleUrl: './shop.css'
})
export class Shop {
}
