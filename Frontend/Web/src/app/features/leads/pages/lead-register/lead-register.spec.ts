import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { Subject } from 'rxjs';
import { RegisterLeadResponse } from '../../../../core/models/lead.model';
import { LeadService } from '../../data/lead.service';
import { LeadRegister } from './lead-register';

describe('LeadRegister', () => {
  let fixture: ComponentFixture<LeadRegister>;
  let service: { registerLead: jasmine.Spy };
  let response: Subject<RegisterLeadResponse>;
  let navigate: jasmine.Spy;

  const page = () => fixture.nativeElement as HTMLElement;
  const button = () => page().querySelector('button[type=submit]') as HTMLButtonElement;
  function fill(id: string, value: string) {
    const input = page().querySelector('#' + id) as HTMLInputElement;
    input.value = value;
    input.dispatchEvent(new Event('input', { bubbles: true }));
    input.dispatchEvent(new Event('blur'));
    fixture.detectChanges();
  }
  function valid() {
    fill('companyName', 'Żółw SA');
    fill('contactName', 'Jan Kowalski');
    fill('contactEmail', 'jan@example.test');
  }
  function submit() {
    page().querySelector('form')!.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
    fixture.detectChanges();
  }

  beforeEach(() => {
    response = new Subject<RegisterLeadResponse>();
    service = { registerLead: jasmine.createSpy().and.returnValue(response) };
    TestBed.configureTestingModule({ imports: [LeadRegister], providers: [provideRouter([]), { provide: LeadService, useValue: service }] });
    navigate = spyOn(TestBed.inject(Router), 'navigate').and.resolveTo(true);
    fixture = TestBed.createComponent(LeadRegister);
    fixture.detectChanges();
  });

  it('marks required fields and does not call the API for an empty form', () => {
    submit();
    expect(service.registerLead).not.toHaveBeenCalled();
    expect(page().querySelectorAll('.error').length).toBe(3);
    expect(button().disabled).toBeFalse();
  });

  [199, 200, 201].forEach(length => {
    it('enforces the company-name limit at ' + length + ' characters', () => {
      valid();
      fill('companyName', 'a'.repeat(length));
      submit();
      expect(service.registerLead.calls.count()).toBe(length <= 200 ? 1 : 0);
    });
    it('enforces the contact-name limit at ' + length + ' characters', () => {
      valid();
      fill('contactName', 'a'.repeat(length));
      submit();
      expect(service.registerLead.calls.count()).toBe(length <= 200 ? 1 : 0);
    });
  });

  it('rejects an invalid email without sending a request', () => {
    valid();
    fill('contactEmail', 'jan@@example.test');
    submit();
    expect(service.registerLead).not.toHaveBeenCalled();
  });

  it('normalizes optional whitespace to null and waits for success before navigating', () => {
    valid();
    fill('contactPhone', '   ');
    fill('source', '   ');
    submit();
    expect(service.registerLead).toHaveBeenCalledOnceWith({ companyName: 'Żółw SA', contactName: 'Jan Kowalski', contactEmail: 'jan@example.test', contactPhone: null, source: null });
    expect(button().disabled).toBeTrue();
    expect(navigate).not.toHaveBeenCalled();
    response.next({ id: 'lead-1' });
    response.complete();
    expect(navigate).toHaveBeenCalledOnceWith(['/leads']);
  });

  it('trims optional values and disables the save button while a request is pending', () => {
    valid();
    fill('contactPhone', ' +48 123 ');
    fill('source', ' Targi ');
    submit();
    expect(service.registerLead.calls.mostRecent().args[0].contactPhone).toBe('+48 123');
    expect(service.registerLead.calls.mostRecent().args[0].source).toBe('Targi');
    expect(button().disabled).toBeTrue();
    button().click();
    expect(service.registerLead.calls.count()).toBe(1);
  });

  [400, 401, 403, 500].forEach(status => {
    it('preserves input and allows a successful retry after HTTP ' + status, () => {
      valid();
      submit();
      response.error({ status, error: status === 400 ? { detail: 'Business rule violated' } : {} });
      fixture.detectChanges();
      expect(button().disabled).toBeFalse();
      expect((page().querySelector('#companyName') as HTMLInputElement).value).toBe('Żółw SA');
      expect(page().querySelector('.error-text')?.textContent).toContain(status === 400 ? 'Business rule violated' : 'Nie udało się zapisać leada.');
      expect(navigate).not.toHaveBeenCalled();
      const retry = new Subject<RegisterLeadResponse>();
      service.registerLead.and.returnValue(retry);
      submit();
      expect(page().querySelector('.error-text')).toBeNull();
      retry.next({ id: 'retry-id' });
      expect(navigate).toHaveBeenCalledOnceWith(['/leads']);
    });
  });
});
