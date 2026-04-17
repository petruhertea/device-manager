import {ChangeDetectionStrategy, Component, computed, inject, OnInit, signal} from '@angular/core';
import {Router, RouterLink} from '@angular/router';
import {FormsModule} from '@angular/forms';
import {debounceTime, distinctUntilChanged, of, Subject, switchMap} from 'rxjs';
import {Device} from '../../../core/models/device.model';
import {DeviceService} from '../../../core/services/device-service';
import {AuthService} from '../../../core/services/auth-service';

@Component({
  selector: 'app-device-list',
  imports: [RouterLink, FormsModule],
  templateUrl: './device-list-component.html',
  styleUrl:    './device-list-component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DeviceListComponent implements OnInit {
  private deviceService  = inject(DeviceService);
  private router         = inject(Router);
  private authService    = inject(AuthService);

  allDevices  = signal<Device[]>([]);
  displayed   = signal<Device[]>([]);
  loading     = signal(true);
  searching   = signal(false);
  error       = signal<string | null>(null);
  deletingId  = signal<number | null>(null);
  searchQuery = signal('');

  readonly isAdmin     = computed(() => this.authService.currentUser()?.role === 'Admin');
  readonly isSearching = computed(() => this.searchQuery().trim().length > 0);

  private readonly search$ = new Subject<string>();

  ngOnInit() {
    this.loadDevices();

    this.search$
      .pipe(
        debounceTime(350),
        distinctUntilChanged(),
        switchMap(query => {
          const trimmed = query.trim();
          if (!trimmed) {
            this.searching.set(false);
            return of(this.allDevices());
          }
          this.searching.set(true);
          return this.deviceService.search(trimmed);
        })
      )
      .subscribe({
        next: results => {
          this.displayed.set(results);
          this.searching.set(false);
          this.error.set(null);
        },
        error: () => {
          this.error.set('Search failed. Please try again.');
          this.searching.set(false);
        }
      });
  }

  onSearchInput(query: string): void {
    this.searchQuery.set(query);
    this.search$.next(query);
  }

  clearSearch(): void {
    this.searchQuery.set('');
    this.search$.next('');
  }

  loadDevices(): void {
    this.loading.set(true);
    this.error.set(null);

    this.deviceService.getAll().subscribe({
      next: devices => {
        this.allDevices.set(devices);
        this.displayed.set(devices);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load devices. Is the API running?');
        this.loading.set(false);
      }
    });
  }

  deleteDevice(event: MouseEvent, id: number): void {
    event.stopPropagation();
    if (!confirm('Are you sure you want to delete this device?')) return;

    this.deletingId.set(id);
    this.deviceService.delete(id).subscribe({
      next: () => {
        this.allDevices.update(ds => ds.filter(d => d.id !== id));
        this.displayed.update(ds => ds.filter(d => d.id !== id));
        this.deletingId.set(null);
      },
      error: () => {
        this.error.set('Failed to delete device.');
        this.deletingId.set(null);
      }
    });
  }

  goToDetail(id: number): void {
    this.router.navigate(['/devices', id]);
  }
}
