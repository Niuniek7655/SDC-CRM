import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router, RouterStateSnapshot } from '@angular/router';

import { AuthService } from './auth.service';
import { CrmRole } from './roles';

/**
 * Requires an authenticated user. When missing, starts the login redirect and
 * remembers the target URL. Optionally enforces roles declared on the route via
 * `data: { roles: [...] }`.
 */
export const authGuard: CanActivateFn = (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    auth.login(state.url);
    return false;
  }

  const requiredRoles = (route.data?.['roles'] as CrmRole[] | undefined) ?? [];
  if (requiredRoles.length > 0 && !auth.hasAnyRole(requiredRoles)) {
    return router.parseUrl('/forbidden');
  }

  return true;
};
