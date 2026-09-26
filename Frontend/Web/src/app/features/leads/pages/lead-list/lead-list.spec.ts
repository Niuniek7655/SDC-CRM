import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { LeadSummary } from '../../../../core/models/lead.model';
import { LeadService } from '../../data/lead.service';
import { LeadList } from './lead-list';

describe('LeadList', () => {
  let leadService: { getMyLeads: jasmine.Spy };

  const qualifiedLead: LeadSummary = {
    id: '3f0c2f1e-8a47-4d3b-9a4b-2f6d9c1e5a77',
    companyName: 'Acme Sp. z o.o.',
    contactName: 'Jan Kowalski',
    contactEmail: 'jan.kowalski@acme.test',
    status: 'Qualified',
    createdAtUtc: '2026-09-26T10:00:00Z',
  };

  function render() {
    TestBed.configureTestingModule({
      imports: [LeadList],
      providers: [provideRouter([]), { provide: LeadService, useValue: leadService }],
    });
    const fixture = TestBed.createComponent(LeadList);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  }

  beforeEach(() => {
    leadService = jasmine.createSpyObj('LeadService', ['getMyLeads']);
  });

  it('should show each lead with the Polish status label', () => {
    leadService.getMyLeads.and.returnValue(of([qualifiedLead]));

    const page = render();

    const badge = page.querySelector('.badge') as HTMLElement;
    expect(page.textContent).toContain('Acme Sp. z o.o.');
    expect(badge.textContent?.trim()).toBe('Zakwalifikowany');
    expect(badge.classList).toContain('badge--qualified');
  });

  it('should show the empty state when the user has no leads', () => {
    leadService.getMyLeads.and.returnValue(of([]));

    const page = render();

    expect(page.textContent).toContain('Nie masz jeszcze żadnych leadów.');
    expect(page.querySelector('table')).toBeNull();
  });

  it('should show an error with a retry button when leads cannot be loaded', () => {
    leadService.getMyLeads.and.returnValue(throwError(() => new Error('network error')));

    const page = render();

    expect(page.textContent).toContain('Nie udało się pobrać leadów');
    expect(page.querySelector('button')?.textContent).toContain('Spróbuj ponownie');
  });
});

