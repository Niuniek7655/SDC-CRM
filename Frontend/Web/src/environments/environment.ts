export const environment = {
  production: false,
  // Empty base URL => requests use relative paths (e.g. /api/leads),
  // proxied to the backend by proxy.conf.json during `ng serve`.
  apiBaseUrl: '',
  sso: {
    // SimpleIdServer authority (realm-prefixed).
    issuer: 'http://localhost:5001/master',
    clientId: 'sdc-crm-web',
    responseType: 'code',
    scope: 'openid profile email role offline_access sdc-crm-api',
    // Local SSO is served over HTTP, so relax the HTTPS requirement.
    requireHttps: false,
  },
};
