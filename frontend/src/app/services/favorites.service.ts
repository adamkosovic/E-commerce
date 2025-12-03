import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class FavoritesService {
  private apiUrl = environment.apiUrl;
  private storageKey = 'guest_favorites';

  private favoritesSubject = new BehaviorSubject<string[]>([]);
  favorites$ = this.favoritesSubject.asObservable();

  constructor(private http: HttpClient) {}

  private isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  loadFavorites() {
    if (!this.isLoggedIn()) {
      const json = localStorage.getItem(this.storageKey);
      const ids = json ? JSON.parse(json) : [];
      this.favoritesSubject.next(ids);
      return;
    }

    this.http.get<string[]>(`${this.apiUrl}/favorites`)
      .subscribe(ids => this.favoritesSubject.next(ids));
  }

  toggleFavorite(productId: string) {
    if (!this.isLoggedIn()) {
      // Gäst → bara localStorage
      this.toggleLocal(productId);
      return;
    }
  
    // Inloggad → optimistisk uppdatering
    if (this.isFavorite(productId)) {
      // ta bort lokalt direkt
      this.removeFromState(productId);
  
      this.http.delete(`${this.apiUrl}/favorites/${productId}`)
        .subscribe({
          error: () => {
            // om API:et failar → lägg tillbaka
            this.addToState(productId);
          }
        });
    } else {
      // lägg till lokalt direkt
      this.addToState(productId);
  
      this.http.post(`${this.apiUrl}/favorites/${productId}`, {})
        .subscribe({
          error: () => {
            // om API:et failar → ta bort igen
            this.removeFromState(productId);
          }
        });
    }
  }

  isFavorite(productId: string): boolean {
    return this.favoritesSubject.value.includes(productId);
  }

  private toggleLocal(productId: string) {
    const current = [...this.favoritesSubject.value];
    let updated;

    if (current.includes(productId)) {
      updated = current.filter(id => id !== productId);
    } else {
      updated = [...current, productId];
    }

    this.favoritesSubject.next(updated);
    localStorage.setItem(this.storageKey, JSON.stringify(updated));
  }

  private addToState(productId: string) {
    const updated = [...this.favoritesSubject.value, productId];
    this.favoritesSubject.next(updated);
  }

  private removeFromState(productId: string) {
    const updated = this.favoritesSubject.value.filter(id => id !== productId);
    this.favoritesSubject.next(updated);
  }

  // Call this after login
  mergeGuestFavorites() {
    if (this.isLoggedIn()) {
      const json = localStorage.getItem(this.storageKey);
      const ids = json ? JSON.parse(json) : [];

      if (ids.length > 0) {
        this.http.post(`${this.apiUrl}/favorites/merge`, ids)
          .subscribe(() => {
            localStorage.removeItem(this.storageKey);
            this.loadFavorites();
          });
      }
    }
  }

  getFavoriteIds(): string[] {
    return this.favoritesSubject.value;
  }
}
