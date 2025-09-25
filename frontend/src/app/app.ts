
import { RouterOutlet } from '@angular/router';
import { Navbar } from "./components/navbar/navbar";
import { Component } from '@angular/core'; 
import 'tslib'; 

@Component({
  selector: 'app-root',
  standalone: true, 
  imports: [RouterOutlet, Navbar],
  templateUrl: './app.html',
  styleUrls: ['./app.css'] 
})

export class App {
  protected readonly title = ('frontend');
}
