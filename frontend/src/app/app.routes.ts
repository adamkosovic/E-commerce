import { Routes, RouterModule } from '@angular/router';
import { NgModel } from '@angular/forms';
import { App } from './app';
import { Home } from './pages/home/home'; // Corrected import path
import { About } from './pages/about/about'; // Example additional component
import { Contact } from './pages/contact/contact';


export const routes: Routes = [
  { path: '', component: Home, title: 'Home'},
  { path: 'about', component: About, title: 'About' },
  { path: 'contact', component: Contact, title: 'Contact' },
  { path: '**', redirectTo: '' } // Wildcard route for a 404 page
];


