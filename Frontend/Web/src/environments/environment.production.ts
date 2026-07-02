export const environment = {
  production: true,
  // TODO: set the deployed backend URL (or keep empty if served from the same origin).
  apiBaseUrl: '',
  sso: {
    // TODO: point to the production identity provider authority.
    issuer: 'http://localhost:5001/master',
    clientId: 'sdc-crm-web',
    responseType: 'code',
    scope: 'openid profile email role offline_access sdc-crm-api',
    requireHttps: true,
  },
};
