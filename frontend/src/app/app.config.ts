import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { httpHeaderInterceptor } from './interceptors/http-header.interceptor';
import { errorInterceptor } from './interceptors/http-error.interceptor';

/**
 * Application configuration for Angular 18+
 *
 * Providers:
 * - Zone change detection with event coalescing for better performance
 * - Router with defined routes
 * - HttpClient with authentication interceptor
 */
export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(
      withInterceptors([httpHeaderInterceptor, errorInterceptor ]) // Automatically adds Bearer token to requests
    )
  ]
};
