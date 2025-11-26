import { Component, Inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from "@angular/router";
import { AuthService } from '../../auth/auth.service';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLinkActive, RouterLink, NgIf],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})

export class Navbar {

  menuOpen = false;

  constructor(public auth: AuthService) {}

  
  toggleMenu() {
    this.menuOpen = !this.menuOpen;
  }
  
  closeMenu() {
    this.menuOpen = false;
  }
  
  onLogout () {
    localStorage.clear();
    window.location.reload();
  }
}
