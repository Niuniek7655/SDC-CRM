import { LeadStatus } from '../../core/models/lead.model';

/** Polish labels of lead statuses from the ubiquitous-language glossary (doc/01, section 6.1). */
const LEAD_STATUS_LABELS: Readonly<Record<LeadStatus, string>> = {
  New: 'Nowy',
  Qualified: 'Zakwalifikowany',
  Rejected: 'Odrzucony',
};

/**
 * Label shown to users for a lead status code returned by the API. Unknown codes (e.g. a status added
 * on the backend before the UI knows it) are shown as-is instead of an empty badge.
 */
export function leadStatusLabel(status: LeadStatus): string {
  const labels: Readonly<Record<string, string>> = LEAD_STATUS_LABELS;
  return labels[status] ?? status;
}

