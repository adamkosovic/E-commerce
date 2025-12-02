import { Routes, RouterModule } from '@angular/router';
import { NgModel } from '@angular/forms';
import { App } from './app';
import { Home } from './pages/home/home'; // Corrected import path
import { Shop } from './pages/shop/shop'; // Example additional component
import { ProductDetailComponent } from './components/product-detail/product-detail';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { AdminGuard } from './guards/admin.guard';    
import { AdminProductList } from './pages/admin/products/admin-product-list/admin-product-list';
import { Cart } from './components/cart/cart';
import { FavoriteProducts } from './components/favorite-products/favorite-products';



export const routes: Routes = [
  { path: '', component: Home, title: 'Home'},
  { path: 'shop', component: Shop, title: 'Shop' },
  { path: 'shop/:id', component: ProductDetailComponent, title: 'Product Details' },
  { path: 'login', component: Login, title: 'Login' },
  { path: 'register', component: Register, title: 'Register' },
  { path: 'admin/products', component: AdminProductList, canActivate: [AdminGuard] , title: 'Admin Products' },
  { path: 'cart', component: Cart , title: 'Shopping Cart' },
  { path: 'favorites', component: FavoriteProducts },
  { path: 'yourCart', loadChildren: () => import('./pages/payment/your-cart/your-cart').then(m => m.YourCart), title: 'Your Cart' }, 
  { path: '**', redirectTo: '' } // Wildcard route for a 404 page
];


