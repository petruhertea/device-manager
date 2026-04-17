import {ChangeDetectionStrategy, Component, computed, inject, OnInit, signal} from '@angular/core';
import {ActivatedRoute, RouterLink} from '@angular/router';
import {Device} from '../../../core/models/device.model';
import {DeviceService} from '../../../core/services/device-service';
import {AuthService} from '../../../core/services/auth-service';

@Component({
  selector: 'app-device-detail',
  imports: [RouterLink],
  templateUrl: './device-detail-component.html',
  styleUrl: './device-detail-component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DeviceDetailComponent implements OnInit {
  private route         = inject(ActivatedRoute);
  private deviceService = inject(DeviceService);
  readonly authService  = inject(AuthService);

  device   = signal<Device | null>(null);
  loading  = signal(true);
  error    = signal<string | null>(null);
  assigning = signal(false);

  readonly isAdmin = computed(() => this.authService.currentUser()?.role === 'Admin');

  readonly isAssignedToMe = computed(() => {
    const user   = this.authService.currentUser();
    const device = this.device();
    if (!user || !device?.assignedUserName) return false;
    return device.assignedUserName === user.fullName;
  });

  readonly isAssignedToOther = computed(() =>
    !!this.device()?.assignedUserName && !this.isAssignedToMe()
  );

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.loadDevice(id);
  }

  private loadDevice(id: number): void {
    this.deviceService.getById(id).subscribe({
      next:  device => { this.device.set(device); this.loading.set(false); },
      error: ()     => { this.error.set('Device not found.'); this.loading.set(false); }
    });
  }

  assignToMe(): void {
    const device = this.device();
    const user   = this.authService.currentUser();
    if (!device || !user) return;

    this.assigning.set(true);
    this.error.set(null);

    this.deviceService.updateAssignment(device.id, user.id).subscribe({
      next:  updated => { this.device.set(updated); this.assigning.set(false); },
      error: err     => {
        this.assigning.set(false);
        this.error.set(
          err.status === 409
            ? 'This device has just been assigned to someone else.'
            : 'Failed to assign device. Please try again.'
        );
      }
    });
  }

  unassign(): void {
    const device = this.device();
    if (!device) return;

    this.assigning.set(true);
    this.error.set(null);

    this.deviceService.updateAssignment(device.id, null).subscribe({
      next:  updated => { this.device.set(updated); this.assigning.set(false); },
      error: ()      => {
        this.assigning.set(false);
        this.error.set('Failed to unassign device. Please try again.');
      }
    });
  }
}
