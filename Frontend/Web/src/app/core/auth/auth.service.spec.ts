import { TestBed } from '@angular/core/testing';
import { OAuthEvent, OAuthSuccessEvent, OAuthService } from 'angular-oauth2-oidc';
import { Subject } from 'rxjs';
import { AuthService } from './auth.service';

describe('AuthService session state', () => {
  let oauth: jasmine.SpyObj<OAuthService>;
  let events: Subject<OAuthEvent>;
  let auth: AuthService;
  const jwt = (claims: object) => 'header.' + btoa(JSON.stringify(claims)).replace(/=/g, '').replace(/\+/g, '-').replace(/\//g, '_') + '.signature';

  beforeEach(() => {
    events = new Subject<OAuthEvent>();
    oauth = jasmine.createSpyObj<OAuthService>('OAuthService', ['configure', 'setupAutomaticSilentRefresh', 'loadDiscoveryDocumentAndTryLogin', 'hasValidAccessToken', 'getAccessToken', 'getIdentityClaims', 'initLoginFlow', 'logOut'], { events });
    oauth.loadDiscoveryDocumentAndTryLogin.and.resolveTo(true);
    oauth.hasValidAccessToken.and.returnValue(true);
    oauth.getAccessToken.and.returnValue(jwt({ role: 'Salesperson' }));
    oauth.getIdentityClaims.and.returnValue({ name: 'Jan' });
    TestBed.configureTestingModule({ providers: [{ provide: OAuthService, useValue: oauth }] });
    auth = TestBed.inject(AuthService);
  });

  it('initializes identity and the single role from the access token', async () => {
    await auth.init();
    expect(auth.isAuthenticated()).toBeTrue();
    expect(auth.roles()).toEqual(['Salesperson']);
    expect(auth.userName()).toBe('Jan');
    expect(oauth.setupAutomaticSilentRefresh).toHaveBeenCalledTimes(1);
  });

  it('replaces roles, name and outgoing token when a refresh event arrives', async () => {
    await auth.init();
    const refreshed = jwt({ role: ['SalesManager', 'Admin'] });
    oauth.getAccessToken.and.returnValue(refreshed);
    oauth.getIdentityClaims.and.returnValue({ preferred_username: 'manager' });
    events.next(new OAuthSuccessEvent('token_received'));
    expect(auth.roles()).toEqual(['SalesManager', 'Admin']);
    expect(auth.isAdmin()).toBeTrue();
    expect(auth.userName()).toBe('manager');
    expect(auth.getAccessToken()).toBe(refreshed);
  });

  it('clears the displayed identity when refresh failure or logout invalidates the token', async () => {
    await auth.init();
    oauth.hasValidAccessToken.and.returnValue(false);
    events.next(new OAuthSuccessEvent('logout'));
    expect(auth.isAuthenticated()).toBeFalse();
    expect(auth.roles()).toEqual([]);
    expect(auth.userName()).toBeNull();
  });

  ['malformed', 'header.@@@.signature', jwt({}), jwt({ role: 123 })].forEach(token => {
    it('does not grant a role for malformed or absent role claims: ' + token, async () => {
      oauth.getAccessToken.and.returnValue(token);
      await auth.init();
      expect(auth.roles()).toEqual([]);
      expect(auth.hasAnyRole(['Salesperson', 'Admin'])).toBeFalse();
    });
  });

  it('supports the alternate roles claim and email display-name fallback', async () => {
    oauth.getAccessToken.and.returnValue(jwt({ roles: ['BackofficeUser'] }));
    oauth.getIdentityClaims.and.returnValue({ email: 'backoffice@example.test' });
    await auth.init();
    expect(auth.roles()).toEqual(['BackofficeUser']);
    expect(auth.userName()).toBe('backoffice@example.test');
  });

  it('settles initialization without inventing a session when discovery fails', async () => {
    spyOn(console, 'error');
    oauth.hasValidAccessToken.and.returnValue(false);
    oauth.loadDiscoveryDocumentAndTryLogin.and.rejectWith(new Error('SSO unavailable'));
    await auth.init();
    expect(auth.isAuthenticated()).toBeFalse();
    expect(auth.roles()).toEqual([]);
  });
});
