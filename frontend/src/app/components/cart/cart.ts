import { Component, OnInit } from '@angular/core';
import { CartService } from '../../services/cart/cart.service';
import { CartDto, CartItemDto } from '../../models/cart/cart.models';
import { Observable } from 'rxjs';
import { CommonModule, } from '@angular/common';



@Component({
  selector: 'app-cart',
  imports: [CommonModule],
  templateUrl: './cart.html',
  styleUrl: './cart.css'
})
export class Cart implements OnInit {
  cart$! : Observable<CartDto | null>;

  constructor(private cartService: CartService) {}

  ngOnInit(): void {
    this.cart$ = this.cartService.cart$;
    this.cartService.loadCart();
  }

  increase(item: CartItemDto) {
    this.cartService.updateItem(item.productId, item.qty + 1);
  }

  decrease(item: CartItemDto) {
    const newQty = item.qty - 1;
    if (newQty <= 0) {
      this.cartService.removeItem(item.productId);
    } else {
      this.cartService.updateItem(item.productId, newQty);
    }
  }

  remove(item: CartItemDto) {
    this.cartService.removeItem(item.productId);
  }

  clear() {
    this.cartService.clearCart();
  }

  toPrice(totalMinor: number): string {
    return (totalMinor / 100).toFixed(2) + ' kr';
  }

}
