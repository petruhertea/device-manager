import {ChangeDetectionStrategy, Component, inject} from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth-service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  templateUrl: 'app.html',
  styleUrl: 'app.scss',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class App {
  readonly authService = inject(AuthService);

  logout(): void {
    this.authService.logout().subscribe();
  }
}
