export type LeadStatus = 'New' | 'Qualified' | 'Rejected';

/** Read model returned by the backend lead lists. */
export interface LeadSummary {
  id: string;
  companyName: string;
  contactName: string;
  contactEmail: string;
  status: LeadStatus;
  createdAtUtc: string;
}

/** Payload for registering a new lead (matches the backend API contract). */
export interface RegisterLeadRequest {
  companyName: string;
  contactName: string;
  contactEmail: string;
  contactPhone: string | null;
  source: string | null;
  assignedSalespersonId: string;
}

export interface RegisterLeadResponse {
  id: string;
}
