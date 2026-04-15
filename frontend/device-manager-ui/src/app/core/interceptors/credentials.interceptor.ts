import { HttpInterceptorFn } from '@angular/common/http';

/**
 * Attaches `withCredentials: true` to every outgoing request so that
 * the HttpOnly auth cookie is sent automatically to the API.
 */
export const credentialsInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req.clone({ withCredentials: true }));
};
