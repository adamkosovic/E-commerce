import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../auth/auth.service';
import { Router } from "@angular/router";
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  imports: [FormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  email: string = '';
  password: string = '';
  errorMessage: string | null = null;
  loading = false; 

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit() {
    this.errorMessage = null;
    this.loading = true;

    this.authService.login(this.email, this.password).subscribe({
      next: (res) => {
        console.log('Login successful:', res);
        this.loading = false;
        this.router.navigate(['/']);
    }, 
      error: (err) => {
        this.loading = false;
        this.errorMessage = 'Login failed. Please check your credentials and try again.';
      }
    });
  }

  toRegister() {
    this.router.navigate(['/register']);
  }
}
