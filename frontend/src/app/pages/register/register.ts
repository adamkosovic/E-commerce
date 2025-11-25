import { Component } from '@angular/core';
import { tap } from 'rxjs';
import { AuthService } from '../../auth/auth.service';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  email: string = '';
  password: string = '';
  loading = false;
  errorMessage: string | null = null;


  constructor(
    private auth: AuthService, 
    private router: Router
    ) {}

    register() {
      this.loading = true;
      this.errorMessage = null;

      this.auth.register(this.email, this.password)
        .pipe(
          tap({
            next: (res) => {
              console.log('Registration successful:', res);
              this.loading = false;
              this.router.navigate(['/']);
            },
            error: (err) => {
              this.loading = false;
              this.errorMessage = 'Registration failed. Please try again.';
            }
          })
        )
        .subscribe();
    }
}