// src/app/interceptors/auth.interceptor.ts
import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent
} from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Hämta token från localStorage (eller där du sparar den)
    const token = localStorage.getItem('token');

    // Om ingen token finns -> skicka requesten som vanligt
    if (!token) {
      return next.handle(req);
    }

    // Klona requesten och lägg till Authorization-headern
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });

    // Skicka vidare den modifierade requesten
    return next.handle(authReq);
  }
}
