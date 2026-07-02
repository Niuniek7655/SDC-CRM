import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { LeadSummary, RegisterLeadRequest, RegisterLeadResponse } from '../../../core/models/lead.model';

/**
 * Thin data-access service for the Leads feature.
 * Talks to the backend Leads API and returns typed models.
 */
@Injectable({ providedIn: 'root' })
export class LeadService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/leads`;

  registerLead(request: RegisterLeadRequest): Observable<RegisterLeadResponse> {
    return this.http.post<RegisterLeadResponse>(this.baseUrl, request);
  }

  getMyLeads(): Observable<LeadSummary[]> {
    return this.http.get<LeadSummary[]>(`${this.baseUrl}/mine`);
  }
}
