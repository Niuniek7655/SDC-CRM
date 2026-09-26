import { OAuthStorage } from 'angular-oauth2-oidc';

/**
 * Storage used by angular-oauth2-oidc for the access/refresh tokens, the PKCE verifier and the nonce.
 *
 * sessionStorage keeps the session across page reloads within a tab, but it is not shared between tabs
 * and is cleared when the tab is closed - a smaller exposure window than localStorage. A new tab signs in
 * again through SSO, usually without a password prompt because the identity provider session is still active.
 */
export function oauthStorageFactory(): OAuthStorage {
  return sessionStorage;
}

