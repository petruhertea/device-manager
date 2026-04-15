import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { AuthUser, LoginRequest, RegisterRequest } from '../models/auth.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  private _currentUser = signal<AuthUser | null>(null);

  readonly currentUser = this._currentUser.asReadonly();
  readonly isLoggedIn = computed(() => this._currentUser() !== null);
  readonly isAdmin = computed(() => this._currentUser()?.role === 'Admin');

  login(dto: LoginRequest): Observable<AuthUser> {
    return this.http
      .post<AuthUser>(`${this.baseUrl}/login`, dto, { withCredentials: true })
      .pipe(tap(user => this._currentUser.set(user)));
  }

  register(dto: RegisterRequest): Observable<AuthUser> {
    return this.http
      .post<AuthUser>(`${this.baseUrl}/register`, dto, { withCredentials: true });
  }

  logout(): Observable<void> {
    return this.http
      .post<void>(`${this.baseUrl}/logout`, {}, { withCredentials: true })
      .pipe(
        tap(() => {
          this._currentUser.set(null);
          this.router.navigate(['/login']);
        })
      );
  }

  /** Call once on app startup to rehydrate session from HttpOnly cookie */
  loadCurrentUser(): Observable<AuthUser> {
    return this.http
      .get<AuthUser>(`${this.baseUrl}/me`, { withCredentials: true })
      .pipe(tap(user => this._currentUser.set(user)));
  }

  setUser(user: AuthUser): void {
    this._currentUser.set(user);
  }
}
