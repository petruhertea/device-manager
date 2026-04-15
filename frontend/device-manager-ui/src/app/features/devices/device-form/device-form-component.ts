import {ChangeDetectionStrategy, Component, inject, OnInit, signal} from '@angular/core';
import {ActivatedRoute, Router, RouterLink} from '@angular/router';
import {FormBuilder, ReactiveFormsModule, Validators} from '@angular/forms';
import {User} from '../../../core/models/user.model';
import {DeviceService} from '../../../core/services/device-service';
import {UserService} from '../../../core/services/user-service';
import {forkJoin} from 'rxjs';

@Component({
  selector: 'app-device-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './device-form-component.html',
  styleUrl: 'device-form-component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DeviceFormComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private deviceService = inject(DeviceService);
  private userService = inject(UserService);

  isEditMode = signal(false);
  editId = signal<number | null>(null);
  submitting = signal(false);
  error = signal<string | null>(null);
  users = signal<User[]>([]);
  apiErrors = signal<Record<string, string[]>>({});

  form = this.fb.group({
    name:            ['', Validators.required],
    manufacturer:    ['', Validators.required],
    type:            ['phone', Validators.required],
    operatingSystem: ['', Validators.required],
    osVersion:       ['', Validators.required],
    processor:       ['', Validators.required],
    ramAmount:       [null as number | null, [Validators.required, Validators.min(1)]],
    description:     [''],
    assignedUserId:  [null as number | null]
  });

  ngOnInit() {
    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam) {
      // Edit mode — load users and device in parallel
      this.isEditMode.set(true);
      this.editId.set(Number(idParam));

      forkJoin({
        users: this.userService.getAll(),
        device: this.deviceService.getById(Number(idParam))
      }).subscribe({
        next: ({ users, device }) => {
          this.users.set(users);

          // Resolve the assignedUserId from the device's assignedUserName
          const assignedUser = device.assignedUserName
            ? users.find(u => u.name === device.assignedUserName) ?? null
            : null;

          this.form.patchValue({
            name:            device.name,
            manufacturer:    device.manufacturer,
            type:            device.type,
            operatingSystem: device.operatingSystem,
            osVersion:       device.osVersion,
            processor:       device.processor,
            ramAmount:       device.ramAmount,
            description:     device.description,
            assignedUserId:  assignedUser?.id ?? null
          });
        },
        error: () => this.error.set('Failed to load device.')
      });
    } else {
      // Create mode — just load users
      this.userService.getAll().subscribe({
        next: users => this.users.set(users)
      });
    }
  }

  isInvalid(field: string): boolean {
    const control = this.form.get(field);
    return !!(control?.invalid && control?.touched);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.apiErrors.set({});

    const payload = {
      ...this.form.value,
      ramAmount: Number(this.form.value.ramAmount),
      assignedUserId: this.form.value.assignedUserId
        ? Number(this.form.value.assignedUserId)
        : null
    } as any;

    const request$ = this.isEditMode()
      ? this.deviceService.update(this.editId()!, payload)
      : this.deviceService.create(payload);

    request$.subscribe({
      next: device => this.router.navigate(['/devices', device.id]),
      error: err => {
        this.submitting.set(false);
        if (err.status === 400 && err.error?.errors) {
          this.apiErrors.set(err.error.errors);
        } else {
          this.error.set('Failed to save device. Please try again.');
        }
      }
    });
  }

  apiError(field: string): string | null {
    const key = field.charAt(0).toUpperCase() + field.slice(1);
    return this.apiErrors()[key]?.[0] ?? null;
  }

  normalizeVersion(): void {
    const val = this.form.get('osVersion')?.value?.trim();
    if (val && !val.includes('.')) {
      this.form.get('osVersion')?.setValue(val + '.0');
    }
  }
}
