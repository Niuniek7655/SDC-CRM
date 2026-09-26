import { LeadStatus } from '../../core/models/lead.model';
import { leadStatusLabel } from './lead-status-label';

describe('leadStatusLabel', () => {
  it('should show New as "Nowy" (glossary 6.1)', () => {
    expect(leadStatusLabel('New')).toBe('Nowy');
  });

  it('should show Qualified as "Zakwalifikowany"', () => {
    expect(leadStatusLabel('Qualified')).toBe('Zakwalifikowany');
  });

  it('should show Rejected as "Odrzucony"', () => {
    expect(leadStatusLabel('Rejected')).toBe('Odrzucony');
  });

  it('should fall back to the status code when the backend sends a status unknown to the UI', () => {
    expect(leadStatusLabel('InContact' as LeadStatus)).toBe('InContact');
  });
});

