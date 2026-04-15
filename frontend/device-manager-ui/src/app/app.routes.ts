import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'devices', pathMatch: 'full' },

  // Public routes
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login-component/login-component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () =>
      import('./features/auth/register-component/register-component').then(m => m.RegisterComponent)
  },

  // Protected routes
  {
    path: 'devices',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/devices/device-list/device-list-component')
        .then(m => m.DeviceListComponent)
  },
  {
    path: 'devices/new',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/devices/device-form/device-form-component')
        .then(m => m.DeviceFormComponent)
  },
  {
    path: 'devices/:id',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/devices/device-detail/device-detail-component')
        .then(m => m.DeviceDetailComponent)
  },
  {
    path: 'devices/:id/edit',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/devices/device-form/device-form-component')
        .then(m => m.DeviceFormComponent)
  }
];
