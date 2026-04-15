import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth-service';
import { catchError, map, of } from 'rxjs';

/**
 * Protects routes by checking the /me endpoint.
 * If the cookie is still valid the user is allowed through;
 * otherwise they are redirected to /login.
 *
 * We call /me (rather than checking a local signal) so that a hard
 * refresh still works — the HttpOnly cookie survives a page reload
 * but the in-memory signal does not.
 */
export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // If we already have the user in memory (same-session navigation) allow immediately
  if (authService.isLoggedIn()) {
    return true;
  }

  // Otherwise ask the server — the auth cookie will be sent automatically
  return authService.loadCurrentUser().pipe(
    map(() => true),
    catchError(() => of(router.createUrlTree(['/login'])))
  );
};
