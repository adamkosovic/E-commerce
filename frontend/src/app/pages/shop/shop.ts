import { Component } from '@angular/core';
import { ProductListComponent } from '../../components/product-list/product-list';


@Component({
  selector: 'app-shop',
  standalone: true,
  imports: [ProductListComponent],
  templateUrl: './shop.html',
  styleUrls: ['./shop.css']
})
export class Shop {
}
