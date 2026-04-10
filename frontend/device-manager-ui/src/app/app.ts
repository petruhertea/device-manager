import { Component, inject, OnInit } from '@angular/core';
import {DeviceService} from './core/services/device-service';

@Component({
  selector: 'app-root',
  standalone: true,
  template: `<p>Check the console</p>`
})
export class App implements OnInit {
  private deviceService: DeviceService = inject(DeviceService);

  ngOnInit() {
    this.deviceService.getAll().subscribe({
      next: devices => console.log('Devices loaded:', devices),
      error: err => console.error('API error:', err)
    });
  }
}
