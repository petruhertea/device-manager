import {ChangeDetectionStrategy, Component, inject, signal} from '@angular/core';
import {FormBuilder, ReactiveFormsModule, Validators} from '@angular/forms';
import {Router, RouterLink} from '@angular/router';
import {AuthService} from '../../../core/services/auth-service';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  standalone: true,
  templateUrl: './register-component.html',
  styleUrl: './register-component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  submitting = signal(false);
  error = signal<string | null>(null);

  form = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(2)]],
    email:    ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    location: ['']
  });

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

    this.authService.register(this.form.value as {
      fullName: string;
      email: string;
      password: string;
      location: string;
    }).subscribe({
      next: () => this.router.navigate(['/login'], { queryParams: { registered: '1' } }),
      error: (err) => {
        this.submitting.set(false);
        if (err.status === 409) {
          this.error.set('An account with this email already exists.');
        } else if (err.status === 400 && err.error?.message) {
          this.error.set(err.error.message);
        } else {
          this.error.set('Registration failed. Please try again.');
        }
      }
    });
  }
}
