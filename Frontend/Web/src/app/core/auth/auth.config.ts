import { AuthConfig } from 'angular-oauth2-oidc';

import { environment } from '../../../environments/environment';

/**
 * OpenID Connect configuration for the SPA.
 * Uses the Authorization Code flow with PKCE (public client, no secret).
 */
export const authCodeFlowConfig: AuthConfig = {
  issuer: environment.sso.issuer,
  clientId: environment.sso.clientId,
  responseType: environment.sso.responseType,
  // Redirect back to the app root; the app initializer completes the login.
  redirectUri: window.location.origin + '/',
  postLogoutRedirectUri: window.location.origin + '/',
  scope: environment.sso.scope,
  requireHttps: environment.sso.requireHttps,
  showDebugInformation: !environment.production,
  // The SPA relies on the refresh token (offline_access scope) for automatic
  // renewal, so iframe-based silent refresh is intentionally disabled.
  useSilentRefresh: false,
};
