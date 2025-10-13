import { Routes, RouterModule } from '@angular/router';
import { NgModel } from '@angular/forms';
import { App } from './app';
import { Home } from './pages/home/home'; // Corrected import path
import { About } from './pages/about/about'; // Example additional component
import { Shop } from './pages/shop/shop'; // Example additional component
import { Stories } from './pages/stories/stories'; // Example additional component
import { ProductDetail } from './components/product-detail/product-detail';
import { Login } from './pages/login/login';



export const routes: Routes = [
  { path: '', component: Home, title: 'Home'},
  { path: 'shop', component: Shop, title: 'Shop' },
  { path: 'shop/:id', component: ProductDetail, title: 'Product Details' },
  { path: 'stories', component: Stories, title: 'Stories' },
  { path: 'about', component: About, title: 'About' },
  { path: 'login', component: Login, title: 'Login' },
  { path: 'yourCart', loadChildren: () => import('./pages/payment/your-cart/your-cart').then(m => m.YourCart), title: 'Your Cart' }, 
  { path: '**', redirectTo: '' } // Wildcard route for a 404 page
];


