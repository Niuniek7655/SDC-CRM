import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';

import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

/**
 * Attaches the bearer access token to API requests only.
 * Requests to the identity provider (discovery, token) are left untouched.
 */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const apiPrefix = `${environment.apiBaseUrl}/api`;
  const isApiCall = request.url.startsWith(apiPrefix) || request.url.startsWith('/api');

  if (!isApiCall) {
    return next(request);
  }

  const token = inject(AuthService).getAccessToken();
  if (!token) {
    return next(request);
  }

  return next(
    request.clone({
      setHeaders: { Authorization: `Bearer ${token}` },
    }),
  );
};
