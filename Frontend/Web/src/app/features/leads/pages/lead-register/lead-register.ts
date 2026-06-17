import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { RegisterLeadRequest } from '../../../../core/models/lead.model';
import { LeadService } from '../../data/lead.service';

@Component({
  selector: 'app-lead-register',
  imports: [ReactiveFormsModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './lead-register.html',
  styleUrl: './lead-register.scss',
})
export class LeadRegister {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly leadService = inject(LeadService);
  private readonly router = inject(Router);

  protected readonly submitting = signal(false);
  protected readonly serverError = signal<string | null>(null);

  protected readonly form = this.fb.group({
    companyName: ['', [Validators.required, Validators.maxLength(200)]],
    contactName: ['', [Validators.required, Validators.maxLength(200)]],
    contactEmail: ['', [Validators.required, Validators.email]],
    contactPhone: [''],
    source: [''],
  });

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.serverError.set(null);

    const value = this.form.getRawValue();
    const request: RegisterLeadRequest = {
      companyName: value.companyName,
      contactName: value.contactName,
      contactEmail: value.contactEmail,
      contactPhone: value.contactPhone.trim() || null,
      source: value.source.trim() || null,
      // TODO: replace with the authenticated salesperson id once auth is added.
      assignedSalespersonId: '00000000-0000-0000-0000-000000000001',
    };

    this.leadService.registerLead(request).subscribe({
      next: () => this.router.navigate(['/leads']),
      error: (error) => {
        this.serverError.set(error?.error?.detail ?? 'Nie udało się zapisać leada.');
        this.submitting.set(false);
      },
    });
  }
}
