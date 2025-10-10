import { Component,  OnInit} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../services/products';
import { Product } from '../../models/product/product.model';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-product-detail',
  imports: [CommonModule],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.css'
})
export class ProductDetail implements OnInit {
  product?: Product;

  constructor( 
    private route: ActivatedRoute,
    private router: Router,
    private productservice: ProductService,
    ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/shop']);
      return;
    }

    const found = this.productservice.getById(id);
    if (!found) {
      this.router.navigate(['/shop']);
      return;
    }

    this.product = found;
  }
}
