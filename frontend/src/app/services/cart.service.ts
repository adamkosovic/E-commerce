import { Injectable } from '@angular/core';
import { BehaviorSubject, tap } from 'rxjs';
import { CartDto } from '../models/cart/cart.models';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/enviroment';
import { Product } from '../models/product/product.model';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private cartSubject = new BehaviorSubject<CartDto> (this.createEmptyCart());
  cart$ = this.cartSubject.asObservable();
  private apiUrl = environment.apiUrl; 

  //key för sessionstorage för gästers kundvagn
  private storageKey = 'guest_cart';

  constructor(private http: HttpClient) {}

  private isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  // Hämta kundvagn från backend
  loadCart() {
    if (!this.isLoggedIn()) {
      //  👤 Gäst → hämta från localStorage
      const cart = this.getGuestCart();
      this.cartSubject.next(cart);
      return;
    }

    //  🔐 Inloggad → hämta från backend
    this.http.get<CartDto>(`${this.apiUrl}/cart`).pipe(
      tap(cart => this.cartSubject.next(cart))
    ).subscribe();
  }

  private getGuestCart(): CartDto {
    const json = sessionStorage.getItem(this.storageKey);
    return json ? JSON.parse(json) as CartDto : this.createEmptyCart();
  }

  private saveGuestCart(cart: CartDto): void {
    sessionStorage.setItem(this.storageKey, JSON.stringify(cart));
  }
  
  private createEmptyCart(): CartDto {
    return {
      id: 'guest',
      items: [],
      totalMinor: 0
    };
  }


  private calculateTotalMinor(cart: CartDto): number {
    return cart.items.reduce((sum, item) => 
    sum + item.price * 100 * item.qty, 
    0
    );
  }


  // Lägg till produkt (eller öka qty)
  addItem(product: Product, qty: number = 1) {
    if(!this.isLoggedIn()) {
      const cart = this.getGuestCart();
      const existing = cart.items.find(i => i.productId === product.id);

      if (existing) {
        existing.qty += qty;
      } else {
        cart.items.push({
          id: product.id,          
          productId: product.id,
          title: product.title,
          price: product.price,
          qty
        });
      }

      cart.totalMinor = this.calculateTotalMinor(cart);
      this.saveGuestCart(cart);
      this.cartSubject.next(cart);
      return;
    }

    //  🔐 Inloggad → spara i backend
    const body = { productId: product.id, qty };
    return this.http.post<void>(`${this.apiUrl}/cart/items`, body).pipe(
      tap(() => this.loadCart()) 
    ).subscribe();
  }

  // Sätt nytt antal på en rad
  updateItem(productId: string, qty: number) {
    if(!this.isLoggedIn()) {
      const cart = this.getGuestCart();
      const existing = cart.items.find(i => i.productId === productId);

      if (existing) {
        existing.qty = qty;
        cart.totalMinor = this.calculateTotalMinor(cart);
        this.saveGuestCart(cart);
        this.cartSubject.next(cart);
      }
      return;
    }


    const body = { qty };
    return this.http.put<void>(`${this.apiUrl}/cart/items/${productId}`, body).pipe(
      tap(() => this.loadCart()) 
    ).subscribe();
  }

  // Ta bort rad från kundvagn
  removeItem(productId: string) {
    if(!this.isLoggedIn()) {
      const cart = this.getGuestCart();
      cart.items = cart.items.filter(i => i.productId !== productId);
      cart.totalMinor = this.calculateTotalMinor(cart);
      this.saveGuestCart(cart);
      this.cartSubject.next(cart);
      return;
    }


    return this.http.delete<void>(`${this.apiUrl}/cart/items/${productId}`).pipe(
      tap(() => this.loadCart()) 
    ).subscribe();
  }

  //Töm hela kundvagnen
  clearCart() {
    if(!this.isLoggedIn()) {
      const emptyCart = this.createEmptyCart();
      this.saveGuestCart(emptyCart);
      this.cartSubject.next(emptyCart);
      return;
    }


    return this.http.delete<void>(`${this.apiUrl}/cart`).pipe(
      tap(() => this.loadCart()) 
    ).subscribe();
  }
}
