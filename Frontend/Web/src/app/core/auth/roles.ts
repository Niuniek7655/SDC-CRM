/** CRM roles as delivered in the access token `role` claim by SimpleIdServer. */
export const CrmRoles = {
  Salesperson: 'Salesperson',
  SalesManager: 'SalesManager',
  BackofficeUser: 'BackofficeUser',
  BackofficeManager: 'BackofficeManager',
  Admin: 'Admin',
} as const;

export type CrmRole = (typeof CrmRoles)[keyof typeof CrmRoles];
