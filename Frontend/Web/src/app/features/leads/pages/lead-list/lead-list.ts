import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { LeadSummary } from '../../../../core/models/lead.model';
import { LeadService } from '../../data/lead.service';

@Component({
  selector: 'app-lead-list',
  imports: [RouterLink, DatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './lead-list.html',
  styleUrl: './lead-list.scss',
})
export class LeadList {
  private readonly leadService = inject(LeadService);

  // TODO: replace with the authenticated salesperson id once auth is added.
  protected readonly salespersonId = '00000000-0000-0000-0000-000000000001';

  protected readonly leads = signal<LeadSummary[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.leadService.getMyLeads(this.salespersonId).subscribe({
      next: (leads) => {
        this.leads.set(leads);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Nie udało się pobrać leadów. Sprawdź, czy backend jest uruchomiony.');
        this.loading.set(false);
      },
    });
  }
}
