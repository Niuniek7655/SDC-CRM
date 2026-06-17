import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

/**
 * Functional HTTP interceptor: centralised logging of failed requests.
 * Errors are re-thrown so feature components can show contextual messages.
 */
export const errorInterceptor: HttpInterceptorFn = (request, next) =>
  next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      console.error(`[HTTP ${error.status}] ${request.method} ${request.url}`, error.error);
      return throwError(() => error);
    }),
  );
