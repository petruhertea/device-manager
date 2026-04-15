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
  private route = inject(ActivatedRoute);
  private deviceService = inject(DeviceService);
  readonly authService = inject(AuthService);

  device = signal<Device | null>(null);
  loading = signal(true);
  error = signal<string | null>(null);
  assigning = signal(false);

  /**
   * True when the device is assigned to the currently logged-in user.
   * Used to show "Unassign" instead of "Assign to me".
   */
  readonly isAssignedToMe = computed(() => {
    const user = this.authService.currentUser();
    const device = this.device();
    if (!user || !device) return false;
    return device.assignedUserName === user.fullName;
  });

  /**
   * True when the device is assigned to someone else.
   * The assign button should be hidden in this case.
   */
  readonly isAssignedToOther = computed(() => {
    const device = this.device();
    if (!device?.assignedUserName) return false;
    return !this.isAssignedToMe();
  });

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.loadDevice(id);
  }

  private loadDevice(id: number): void {
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

  assignToMe(): void {
    const device = this.device();
    const user = this.authService.currentUser();
    if (!device || !user) return;

    this.assigning.set(true);
    this.error.set(null);

    // Build a full UpdateDeviceDto — the PUT endpoint replaces the whole entity
    const payload = {
      name:            device.name,
      manufacturer:    device.manufacturer,
      type:            device.type,
      operatingSystem: device.operatingSystem,
      osVersion:       device.osVersion,
      processor:       device.processor,
      ramAmount:       device.ramAmount,
      description:     device.description,
      assignedUserId:  user.id
    };

    this.deviceService.update(device.id, payload).subscribe({
      next: updated => {
        this.device.set(updated);
        this.assigning.set(false);
      },
      error: () => {
        this.error.set('Failed to assign device. Please try again.');
        this.assigning.set(false);
      }
    });
  }

  unassign(): void {
    const device = this.device();
    if (!device) return;

    this.assigning.set(true);
    this.error.set(null);

    const payload = {
      name:            device.name,
      manufacturer:    device.manufacturer,
      type:            device.type,
      operatingSystem: device.operatingSystem,
      osVersion:       device.osVersion,
      processor:       device.processor,
      ramAmount:       device.ramAmount,
      description:     device.description,
      assignedUserId:  null
    };

    this.deviceService.update(device.id, payload).subscribe({
      next: updated => {
        this.device.set(updated);
        this.assigning.set(false);
      },
      error: () => {
        this.error.set('Failed to unassign device. Please try again.');
        this.assigning.set(false);
      }
    });
  }
}
