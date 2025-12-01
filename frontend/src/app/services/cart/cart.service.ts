import { Injectable } from '@angular/core';
import { BehaviorSubject, tap } from 'rxjs';
import { CartDto } from '../../models/cart/cart.models';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/enviroment';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private cartSubject = new BehaviorSubject<CartDto | null>(null);
  cart$ = this.cartSubject.asObservable();
  private apiUrl = environment.apiUrl; 

  constructor(private http: HttpClient) {}

  // Hämta kundvagn från backend
  loadCart() {
    this.http.get<CartDto>(`${this.apiUrl}/cart`).pipe(
      tap(cart => this.cartSubject.next(cart))
    ).subscribe();
  }

  // Lägg till produkt (eller öka qty)
  addItem(productId: string, qty: number = 1) {
    const body = { productId, qty };
    return this.http.post<void>(`${this.apiUrl}/cart/items`, body).pipe(
      tap(() => this.loadCart()) 
    ).subscribe();
  }

  // Sätt nytt antal på en rad
  updateItem(productId: string, qty: number) {
    const body = { qty };
    return this.http.put<void>(`${this.apiUrl}/cart/items/${productId}`, body).pipe(
      tap(() => this.loadCart()) 
    ).subscribe();
  }

  // Ta bort rad från kundvagn
  removeItem(productId: string) {
    return this.http.delete<void>(`${this.apiUrl}/cart/items/${productId}`).pipe(
      tap(() => this.loadCart()) 
    ).subscribe();
  }

  //Töm hela kundvagnen
  clearCart() {
    return this.http.delete<void>(`${this.apiUrl}/cart`).pipe(
      tap(() => this.loadCart()) 
    ).subscribe();
  }
}
