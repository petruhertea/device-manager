import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { User } from '../../../core/models/user.model';
import {DeviceService} from '../../../core/services/device-service';
import {UserService} from '../../../core/services/user-service';

@Component({
  selector: 'app-device-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './device-form-component.html',
  styleUrl: 'device-form-component.scss'
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
    description:     ['', Validators.required],
    assignedUserId:  [null as number | null]
  });

  ngOnInit() {
    // Load users for the assignment dropdown
    this.userService.getAll().subscribe({
      next: users => this.users.set(users)
    });

    // Check if we're in edit mode
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode.set(true);
      this.editId.set(Number(id));

      this.deviceService.getById(Number(id)).subscribe({
        next: device => {
          this.form.patchValue({
            name:            device.name,
            manufacturer:    device.manufacturer,
            type:            device.type,
            operatingSystem: device.operatingSystem,
            osVersion:       device.osVersion,
            processor:       device.processor,
            ramAmount:       device.ramAmount,
            description:     device.description,
            assignedUserId:  null  // populated from separate endpoint in Phase 3
          });
        },
        error: () => this.error.set('Failed to load device.')
      });
    }
  }

  isInvalid(field: string) {
    const control = this.form.get(field);
    return control?.invalid && control?.touched;
  }

  submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.error.set(null);
    this.apiErrors.set({});  // add this signal at the top of the class

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

        // 400 = validation errors from the server
        if (err.status === 400 && err.error?.errors) {
          this.apiErrors.set(err.error.errors);
        } else {
          this.error.set('Failed to save device. Please try again.');
        }
      }
    });
  }

  apiError(field: string): string | null {
    // API returns PascalCase field names, so capitalise the first letter
    const key = field.charAt(0).toUpperCase() + field.slice(1);
    return this.apiErrors()[key]?.[0] ?? null;
  }

  normalizeVersion() {
    const val = this.form.get('osVersion')?.value?.trim();
    if (val && !val.includes('.')) {
      this.form.get('osVersion')?.setValue(val + '.0');
    }
  }
}
