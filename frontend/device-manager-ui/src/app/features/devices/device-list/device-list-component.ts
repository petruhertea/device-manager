import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Device } from '../../../core/models/device.model';
import {DeviceService} from '../../../core/services/device-service';

@Component({
  selector: 'app-device-list',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './device-list-component.html',
  styleUrl: './device-list-component.scss'
})
export class DeviceListComponent implements OnInit {
  private deviceService = inject(DeviceService);
  private router = inject(Router);

  devices = signal<Device[]>([]);
  loading = signal(true);
  error = signal<string | null>(null);
  deletingId = signal<number | null>(null);

  ngOnInit() {
    this.loadDevices();
  }

  loadDevices() {
    this.loading.set(true);
    this.error.set(null);

    this.deviceService.getAll().subscribe({
      next: devices => {
        this.devices.set(devices);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load devices. Is the API running?');
        this.loading.set(false);
      }
    });
  }

  deleteDevice(event: MouseEvent, id: number) {
    // Prevent the row click from navigating to detail
    event.stopPropagation();

    if (!confirm('Are you sure you want to delete this device?')) return;

    this.deletingId.set(id);

    this.deviceService.delete(id).subscribe({
      next: () => {
        this.devices.update(devices => devices.filter(d => d.id !== id));
        this.deletingId.set(null);
      },
      error: () => {
        this.error.set('Failed to delete device.');
        this.deletingId.set(null);
      }
    });
  }

  goToDetail(id: number) {
    this.router.navigate(['/devices', id]);
  }
}
