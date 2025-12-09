import { HttpInterceptorFn } from '@angular/common/http';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { inject } from '@angular/core';
import { Router } from '@angular/router';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((error) => {
    if (error.status === 401) {
        router.navigate(['/login']);
    }
    debugger
    const standardError = {
        status: error.status || 500,
        statusText: error.statusText || 'Error',
        message: error.error?.reason || error.error?.reason || 'Unknown error'
    };

    return throwError(() => standardError);
    })
  );
};