import {Component, inject, OnInit} from '@angular/core';
import {DeviceService} from './core/services/device-service';
import {RouterLink, RouterOutlet} from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink],
  templateUrl: 'app.html',
  styleUrl: 'app.scss'
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
