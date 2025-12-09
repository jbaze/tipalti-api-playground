import { Component, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';

/**
 * Login component with mock JWT authentication
 *
 * Features:
 * - Email/password form
 * - Validates against hardcoded admin credentials
 * - Generates realistic JWT token on success
 * - Displays token preview
 * - Redirects to invoices page after login
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  email = '';
  password = '';
  errorMessage = signal<string>('');
  isLoading = signal<boolean>(false);

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    // If already logged in, redirect to invoices
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/invoices']);
    }
  }

  onLogin(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    setTimeout(() => {
      const result = this.authService.login(this.email, this.password);
      if (result.success) {
        // Login successful - navigate to invoices page
        this.router.navigate(['/invoices']);
      } else {
        // Login failed - show error
        this.errorMessage.set(result.error || 'Login failed');
        this.isLoading.set(false);
      }
    }, 500);
  }
}
