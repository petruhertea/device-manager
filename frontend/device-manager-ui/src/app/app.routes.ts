import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'devices', pathMatch: 'full' },
  {
    path: 'devices',
    loadComponent: () =>
      import('./features/devices/device-list/device-list-component')
        .then(m => m.DeviceListComponent)
  },
  {
    path: 'devices/new',
    loadComponent: () =>
      import('./features/devices/device-form/device-form-component')
        .then(m => m.DeviceFormComponent)
  },
  {
    path: 'devices/:id',
    loadComponent: () =>
      import('./features/devices/device-detail/device-detail-component')
        .then(m => m.DeviceDetailComponent)
  },
  {
    path: 'devices/:id/edit',
    loadComponent: () =>
      import('./features/devices/device-form/device-form-component')
        .then(m => m.DeviceFormComponent)
  }
];
