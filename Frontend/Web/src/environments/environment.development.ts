export const environment = {
  production: false,
  // During development requests go through the dev-server proxy (proxy.conf.json).
  apiBaseUrl: '',
  sso: {
    issuer: 'http://localhost:5001/master',
    clientId: 'sdc-crm-web',
    responseType: 'code',
    scope: 'openid profile email role offline_access sdc-crm-api',
    requireHttps: false,
  },
};
