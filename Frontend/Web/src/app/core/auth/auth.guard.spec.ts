import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree, provideRouter } from '@angular/router';

import { authGuard } from './auth.guard';
import { AuthService } from './auth.service';
import { CrmRoles } from './roles';

describe('authGuard', () => {
  let auth: { isAuthenticated: jasmine.Spy; hasAnyRole: jasmine.Spy; login: jasmine.Spy };

  function runGuard(url: string, roles?: string[]) {
    const route = { data: roles ? { roles } : {} } as unknown as ActivatedRouteSnapshot;
    const state = { url } as RouterStateSnapshot;
    return TestBed.runInInjectionContext(() => authGuard(route, state));
  }

  beforeEach(() => {
    auth = jasmine.createSpyObj('AuthService', ['isAuthenticated', 'hasAnyRole', 'login']);
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: auth }],
    });
  });

  it('should start the SSO login for the requested url when the user is not authenticated', () => {
    auth.isAuthenticated.and.returnValue(false);

    const result = runGuard('/leads/new', [CrmRoles.Salesperson]);

    expect(result).toBeFalse();
    expect(auth.login).toHaveBeenCalledOnceWith('/leads/new');
  });

  it('should redirect to /forbidden when the user has none of the required roles', () => {
    auth.isAuthenticated.and.returnValue(true);
    auth.hasAnyRole.and.returnValue(false);

    const result = runGuard('/leads', [CrmRoles.Salesperson, CrmRoles.Admin]);

    expect(result instanceof UrlTree).toBeTrue();
    expect(TestBed.inject(Router).serializeUrl(result as UrlTree)).toBe('/forbidden');
    expect(auth.hasAnyRole).toHaveBeenCalledOnceWith([CrmRoles.Salesperson, CrmRoles.Admin]);
  });

  it('should allow navigation when the user has one of the required roles', () => {
    auth.isAuthenticated.and.returnValue(true);
    auth.hasAnyRole.and.returnValue(true);

    const result = runGuard('/leads', [CrmRoles.Salesperson]);

    expect(result).toBeTrue();
    expect(auth.login).not.toHaveBeenCalled();
  });

  it('should allow any authenticated user when the route declares no roles', () => {
    auth.isAuthenticated.and.returnValue(true);

    const result = runGuard('/forbidden');

    expect(result).toBeTrue();
    expect(auth.hasAnyRole).not.toHaveBeenCalled();
  });
});

