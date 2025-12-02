import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product } from '../models/product/product.model';
import { environment } from '../../environments/enviroment';


export interface ProductPayload {
  title: string;
  description?: string;
  price: number;
  imageUrl: string;
}

@Injectable({ providedIn: 'root' })
export class ProductService {
  private apiUrl = environment.apiUrl; 

  constructor(private http : HttpClient) {}

  // Hjälpmetod för att få med Authorization header om token finns
  private getAuthHeaders(): HttpHeaders | undefined {
      const token = localStorage.getItem('token');
      if (!token) {
        return undefined;
      }
      return new HttpHeaders({
        Authorization: `Bearer ${token}`
    });
  }

  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.apiUrl}/products`);
  }

  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/products` + `/${id}`);
  }

  create(payload: ProductPayload): Observable<Product> {
    const headers = this.getAuthHeaders();
    return this.http.post<Product>(`${this.apiUrl}/products`, payload, {
      headers
    });
  }

  update(id: string, payload: ProductPayload): Observable<Product> {
    const headers = this.getAuthHeaders();
    return this.http.put<Product>(`${this.apiUrl}/products` + `/${id}`, payload, {
      headers
    });
  }

  delete(id: string): Observable<void> {
    const headers = this.getAuthHeaders();
    return this.http.delete<void>(`${this.apiUrl}/products` + `/${id}`, {
      headers
    });
  }

}