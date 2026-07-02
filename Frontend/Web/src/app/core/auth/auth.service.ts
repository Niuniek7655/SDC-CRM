import { Injectable, computed, inject, signal } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';

import { authCodeFlowConfig } from './auth.config';
import { CrmRole } from './roles';

/**
 * Central authentication facade over angular-oauth2-oidc.
 * Owns the login/logout lifecycle and exposes reactive identity state
 * (authenticated flag, user name and CRM roles) as signals.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly oauth = inject(OAuthService);

  private readonly authenticated = signal(false);
  private readonly roleList = signal<string[]>([]);
  private readonly displayName = signal<string | null>(null);

  readonly isAuthenticated = this.authenticated.asReadonly();
  readonly roles = this.roleList.asReadonly();
  readonly userName = this.displayName.asReadonly();
  readonly isAdmin = computed(() => this.roleList().includes('Admin'));

  /** Configures the OIDC client and completes any pending login redirect. */
  async init(): Promise<void> {
    this.oauth.configure(authCodeFlowConfig);
    this.oauth.setupAutomaticSilentRefresh();

    this.oauth.events.subscribe(() => this.syncState());

    try {
      await this.oauth.loadDiscoveryDocumentAndTryLogin();
    } catch (error) {
      console.error('[auth] discovery/login failed', error);
    }

    this.syncState();
  }

  /** Starts the Authorization Code + PKCE redirect to the identity provider. */
  login(targetUrl?: string): void {
    this.oauth.initLoginFlow(targetUrl);
  }

  /** Ends the session locally and at the identity provider. */
  logout(): void {
    this.oauth.logOut();
  }

  getAccessToken(): string | null {
    return this.oauth.getAccessToken() ?? null;
  }

  hasRole(role: CrmRole | string): boolean {
    return this.roleList().includes(role);
  }

  hasAnyRole(roles: readonly (CrmRole | string)[]): boolean {
    return roles.some((role) => this.roleList().includes(role));
  }

  private syncState(): void {
    const hasValidToken = this.oauth.hasValidAccessToken();
    this.authenticated.set(hasValidToken);

    if (!hasValidToken) {
      this.roleList.set([]);
      this.displayName.set(null);
      return;
    }

    this.roleList.set(this.readRolesFromAccessToken());

    const claims = this.oauth.getIdentityClaims() as Record<string, unknown> | null;
    const name =
      (claims?.['name'] as string) ??
      (claims?.['preferred_username'] as string) ??
      (claims?.['email'] as string) ??
      null;
    this.displayName.set(name);
  }

  /**
   * Roles gate UI affordances only; the API re-validates them. They are read
   * from the access token because that is what the backend authorizes against.
   */
  private readRolesFromAccessToken(): string[] {
    const token = this.oauth.getAccessToken();
    if (!token) {
      return [];
    }

    const payload = decodeJwtPayload(token);
    if (!payload) {
      return [];
    }

    const raw = payload['role'] ?? payload['roles'];
    if (Array.isArray(raw)) {
      return raw.map(String);
    }
    if (typeof raw === 'string') {
      return [raw];
    }
    return [];
  }
}

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  const parts = token.split('.');
  if (parts.length < 2) {
    return null;
  }

  try {
    const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
    const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=');
    const json = decodeURIComponent(
      atob(padded)
        .split('')
        .map((c) => '%' + c.charCodeAt(0).toString(16).padStart(2, '0'))
        .join(''),
    );
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return null;
  }
}
