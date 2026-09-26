import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

describe('authInterceptor', () => {
  const identityProviderUrl = 'http://localhost:5001/master/.well-known/openid-configuration';

  let http: HttpClient;
  let httpTesting: HttpTestingController;
  let auth: { getAccessToken: jasmine.Spy };

  beforeEach(() => {
    auth = jasmine.createSpyObj('AuthService', ['getAccessToken']);
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: auth },
      ],
    });
    http = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('should attach the bearer access token to backend API requests', () => {
    auth.getAccessToken.and.returnValue('access-token-123');

    http.get('/api/leads/mine').subscribe();

    const request = httpTesting.expectOne('/api/leads/mine');
    expect(request.request.headers.get('Authorization')).toBe('Bearer access-token-123');
    request.flush([]);
  });

  it('should not send the access token to other hosts such as the identity provider', () => {
    auth.getAccessToken.and.returnValue('access-token-123');

    http.get(identityProviderUrl).subscribe();

    const request = httpTesting.expectOne(identityProviderUrl);
    expect(request.request.headers.has('Authorization')).toBeFalse();
    expect(auth.getAccessToken).not.toHaveBeenCalled();
    request.flush({});
  });

  it('should send API requests without Authorization header when the user has no access token', () => {
    auth.getAccessToken.and.returnValue(null);

    http.get('/api/leads/mine').subscribe();

    const request = httpTesting.expectOne('/api/leads/mine');
    expect(request.request.headers.has('Authorization')).toBeFalse();
    request.flush([]);
  });
});

