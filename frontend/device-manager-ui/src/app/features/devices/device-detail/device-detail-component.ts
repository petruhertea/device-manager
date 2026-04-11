import {Component, inject, OnInit, signal} from '@angular/core';
import {ActivatedRoute, RouterLink} from '@angular/router';

import {Device} from '../../../core/models/device.model';
import {DeviceService} from '../../../core/services/device-service';

@Component({
  selector: 'app-device-detail',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './device-detail-component.html',
  styleUrl: './device-detail-component.scss'
})
export class DeviceDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private deviceService = inject(DeviceService);

  device = signal<Device | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.deviceService.getById(id).subscribe({
      next: device => {
        this.device.set(device);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Device not found.');
        this.loading.set(false);
      }
    });
  }
}
