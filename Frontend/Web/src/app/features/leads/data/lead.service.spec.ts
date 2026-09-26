import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { LeadSummary, RegisterLeadRequest, RegisterLeadResponse } from '../../../core/models/lead.model';
import { LeadService } from './lead.service';

describe('LeadService', () => {
  let service: LeadService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(LeadService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('should load the current user leads with GET /api/leads/mine', () => {
    const leads: LeadSummary[] = [
      {
        id: '3f0c2f1e-8a47-4d3b-9a4b-2f6d9c1e5a77',
        companyName: 'Acme Sp. z o.o.',
        contactName: 'Jan Kowalski',
        contactEmail: 'jan.kowalski@acme.test',
        status: 'New',
        createdAtUtc: '2026-09-26T10:00:00Z',
      },
    ];
    let received: LeadSummary[] | undefined;

    service.getMyLeads().subscribe((result) => (received = result));

    const request = httpTesting.expectOne('/api/leads/mine');
    expect(request.request.method).toBe('GET');
    request.flush(leads);
    expect(received).toEqual(leads);
  });

  it('should register a lead with POST /api/leads without sending the owning salesperson', () => {
    const payload: RegisterLeadRequest = {
      companyName: 'Acme Sp. z o.o.',
      contactName: 'Jan Kowalski',
      contactEmail: 'jan.kowalski@acme.test',
      contactPhone: null,
      source: 'Targi',
    };
    let response: RegisterLeadResponse | undefined;

    service.registerLead(payload).subscribe((result) => (response = result));

    const request = httpTesting.expectOne('/api/leads');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    expect(Object.keys(request.request.body)).not.toContain('assignedSalespersonId');
    request.flush({ id: '3f0c2f1e-8a47-4d3b-9a4b-2f6d9c1e5a77' });
    expect(response).toEqual({ id: '3f0c2f1e-8a47-4d3b-9a4b-2f6d9c1e5a77' });
  });
});

