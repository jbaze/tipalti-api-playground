import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

/**
 * HTTP Interceptor for automatic Bearer token injection
 *
 * Purpose:
 * - Automatically adds Authorization header to all outgoing HTTP requests
 * - Retrieves token from AuthService
 * - Follows Angular 18 functional interceptor pattern (no class-based interceptors)
 *
 * How it works:
 * 1. Intercepts every HTTP request before it's sent
 * 2. Checks if user is logged in (has token)
 * 3. If token exists, clones request and adds "Authorization: Bearer {token}" header
 * 4. If no token, passes request through unchanged
 *
 * Usage:
 * - Automatically applied via app.config.ts providers
 * - No need to manually add headers in service methods
 *
 * Example headers added:
 * ```
 * Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbmlzdH...
 * ```
 */
export const httpHeaderInterceptor: HttpInterceptorFn = (req, next) => {
  // Inject AuthService to access current token
  const authService = inject(AuthService);

  // Get current token from signal
  const token = authService.token();

  // If token exists, clone request and add Authorization header
  if (token) {
    const clonedRequest = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });

    // Log for debugging (optional - can remove in production)
    console.log(`[AuthInterceptor] Added Bearer token to ${req.method} ${req.url}`);

    return next(clonedRequest);
  }

  // No token - pass request through unchanged
  return next(req);
};
