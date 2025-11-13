import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product } from './models/product/product.model';


@Injectable({ providedIn: 'root' })
export class ProductService {
  private baseUrl = '/api/products';  //proxyn skickar vidare till backend

  constructor(private http : HttpClient) {}

  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(this.baseUrl);
  }

  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`/api/products/${id}`);
  }

}