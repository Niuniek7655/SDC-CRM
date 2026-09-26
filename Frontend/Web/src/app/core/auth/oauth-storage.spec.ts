import { oauthStorageFactory } from './oauth-storage';

describe('oauthStorageFactory', () => {
  it('should keep OIDC tokens in sessionStorage so they are not shared across browser tabs', () => {
    expect(oauthStorageFactory()).toBe(sessionStorage);
  });

  it('should not keep OIDC tokens in localStorage', () => {
    expect(oauthStorageFactory()).not.toBe(localStorage);
  });
});

