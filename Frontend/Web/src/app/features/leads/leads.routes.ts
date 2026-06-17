import { Routes } from '@angular/router';

export const LEADS_ROUTES: Routes = [
  {
    path: '',
    title: 'Moje leady',
    loadComponent: () => import('./pages/lead-list/lead-list').then((m) => m.LeadList),
  },
  {
    path: 'new',
    title: 'Nowy lead',
    loadComponent: () => import('./pages/lead-register/lead-register').then((m) => m.LeadRegister),
  },
];
