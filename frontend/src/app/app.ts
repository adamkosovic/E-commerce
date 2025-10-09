
import { RouterOutlet } from '@angular/router';
import { Navbar } from "./components/navbar/navbar";
import { Component } from '@angular/core'; 
import 'tslib'; 
import { Footer } from './components/footer/footer';
import { CategoryMenu } from './components/category-menu/category-menu';

@Component({
  selector: 'app-root',
  standalone: true, 
  imports: [RouterOutlet, Navbar, Footer],
  templateUrl: './app.html',
  styleUrls: ['./app.css'] 
})

export class App {
  protected readonly title = ('frontend');
}
