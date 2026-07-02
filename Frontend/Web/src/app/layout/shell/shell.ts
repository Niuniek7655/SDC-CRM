import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthService } from '../../core/auth/auth.service';
import { CrmRoles } from '../../core/auth/roles';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  private readonly auth = inject(AuthService);

  protected readonly userName = this.auth.userName;
  protected readonly roles = this.auth.roles;

  protected readonly canSeeLeads = computed(() =>
    this.auth.hasAnyRole([CrmRoles.Salesperson, CrmRoles.SalesManager, CrmRoles.Admin]),
  );

  protected logout(): void {
    this.auth.logout();
  }
}
