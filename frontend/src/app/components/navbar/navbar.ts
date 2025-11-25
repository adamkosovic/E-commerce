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

  constructor(public auth: AuthService) {}

  onLogout () {
    localStorage.clear();
    window.location.reload();
  }


  // Login () {
  //   window.location.href = '/login';
  // }

  // Register () {
  //   window.location.href = '/register';
  // }
}
