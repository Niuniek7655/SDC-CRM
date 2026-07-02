import { Routes } from '@angular/router';

import { authGuard } from './core/auth/auth.guard';
import { CrmRoles } from './core/auth/roles';
import { Shell } from './layout/shell/shell';

export const routes: Routes = [
  {
    path: '',
    component: Shell,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'leads' },
      {
        path: 'leads',
        canActivate: [authGuard],
        data: { roles: [CrmRoles.Salesperson, CrmRoles.SalesManager, CrmRoles.Admin] },
        loadChildren: () => import('./features/leads/leads.routes').then((m) => m.LEADS_ROUTES),
      },
      {
        path: 'forbidden',
        title: 'Brak dostępu',
        loadComponent: () => import('./features/errors/forbidden/forbidden').then((m) => m.Forbidden),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
