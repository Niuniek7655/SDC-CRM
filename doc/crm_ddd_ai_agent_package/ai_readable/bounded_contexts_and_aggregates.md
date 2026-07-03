# Bounded contexty, agregaty, encje i value objecty

## Reguły architektoniczne
- Customer Management owns Customer. Other contexts use CustomerId and snapshots/read models.
- Lead & Pipeline owns Lead, Opportunity and Pipeline. Backoffice must not modify the sales pipeline.
- Sales Order Capture owns SalesOrder and submits it to Backoffice through SalesOrderSubmitted.
- Backoffice Order Processing owns OrderProcess and operational realization status. Sales must not change backoffice processing status directly.
- Cross-context state changes should happen through integration events. Synchronous calls are allowed mainly for queries/snapshots and authorization checks.
- Reporting & Analytics is projection/read-model oriented and should be updated from events.

## Customer Management

**Odpowiedzialność:** Źródło prawdy dla danych klienta: karta klienta, dane kontaktowe, adresy i status.

**Aktorzy:** handlowiec, menedżer sprzedaży, backoffice, admin

### Agregat: `Customer`

- Encje: ContactPerson, CustomerAddress
- Value Objects: CustomerStatus, TaxId, EmailAddress, PhoneNumber, Address
- Komendy: CreateCustomer, UpdateCustomerData, AddContactPerson, ChangeCustomerStatus
- Zdarzenia: CustomerCreated, CustomerUpdated, ContactPersonAdded, CustomerStatusChanged

## Lead & Pipeline

**Odpowiedzialność:** Obsługa leadów, kwalifikacji, opportunity i etapów pipeline sprzedażowego.

**Aktorzy:** handlowiec, menedżer sprzedaży

### Agregat: `Lead`

- Encje: none
- Value Objects: LeadSource, LeadPriority, LeadStatus, QualificationData
- Komendy: RegisterLead, AssignLeadToSalesperson, QualifyLead, RejectLead
- Zdarzenia: LeadRegistered, LeadAssigned, LeadQualified, LeadRejected

### Agregat: `Opportunity`

- Encje: StageHistoryEntry
- Value Objects: OpportunityStage, EstimatedValue (Money), Probability, ExpectedCloseDate, LostReason
- Komendy: CreateOpportunity, MoveOpportunityToStage, MarkOpportunityAsWon, MarkOpportunityAsLost
- Zdarzenia: OpportunityCreated, OpportunityStageChanged, OpportunityWon, OpportunityLost

### Agregat: `Pipeline`

- Encje: PipelineStage
- Value Objects: StageDefinition, TransitionRules
- Komendy: ConfigurePipeline, AddPipelineStage, ChangeTransitionRules
- Zdarzenia: PipelineConfigured, PipelineStageChanged

## Sales Activity

**Odpowiedzialność:** Planowanie kontaktów, follow-upów, notatek i rejestracja aktywności handlowca.

**Aktorzy:** handlowiec, menedżer sprzedaży

### Agregat: `SalesActivity`

- Encje: none
- Value Objects: ActivityType, ActivityOutcome, ActivityTimeRange
- Komendy: RegisterPhoneCall, RegisterMeeting, RegisterEmailActivity, CompleteActivity
- Zdarzenia: ActivityRegistered, ActivityCompleted

### Agregat: `FollowUpTask`

- Encje: Reminder
- Value Objects: DueDate, TaskStatus, TaskPriority
- Komendy: ScheduleFollowUp, PostponeFollowUp, CompleteFollowUp
- Zdarzenia: FollowUpScheduled, FollowUpCompleted

### Agregat: `Note`

- Encje: none
- Value Objects: NoteContent
- Komendy: AddCustomerNote, UpdateNote
- Zdarzenia: NoteAdded, NoteUpdated

## Sales Order Capture

**Odpowiedzialność:** Utworzenie i przekazanie zamówienia przez handlowca po finalizacji sprzedaży.

**Aktorzy:** handlowiec, menedżer sprzedaży

### Agregat: `SalesOrder`

- Encje: SalesOrderLine
- Value Objects: OrderTerms, OrderCustomerSnapshot, OrderStatus, Money
- Komendy: CreateSalesOrder, AddOrderLine, UpdateOrderTerms, SubmitSalesOrder, CancelSalesOrder
- Zdarzenia: SalesOrderCreated, SalesOrderSubmitted, SalesOrderCancelled

## Backoffice Order Processing

**Odpowiedzialność:** Obsługa i realizacja zamówienia po stronie backoffice, niezależnie od pipeline sprzedażowego.

**Aktorzy:** backoffice, realizacja/operations, handlowiec, menedżer backoffice

### Agregat: `OrderProcess`

- Encje: OrderClarification, StatusHistoryEntry, FulfillmentRequest
- Value Objects: ProcessStatus, FulfillmentData
- Komendy: StartOrderProcessing, AssignOrderProcess, RequestMissingInformation, AcceptOrderForFulfillment, CompleteOrderProcess, RejectOrderProcess
- Zdarzenia: OrderProcessingStarted, OrderAssigned, MissingInformationRequested, MissingInformationProvided, OrderAcceptedForFulfillment, OrderCompleted, OrderRejected

### Agregat: `BackofficeTask`

- Encje: none
- Value Objects: TaskType, TaskStatus, DueDate
- Komendy: CreateBackofficeTask, AssignBackofficeTask, CompleteBackofficeTask
- Zdarzenia: BackofficeTaskCreated, BackofficeTaskCompleted

## Integration Context

**Odpowiedzialność:** ACL dla ERP, fakturowania, e-maila, kalendarza i płatności.

**Aktorzy:** system CRM, ERP, system fakturowy, e-mail, kalendarz, system płatności

### Agregat: `IntegrationJob`

- Encje: JobAttempt
- Value Objects: ExternalSystemName, JobStatus, CorrelationId
- Komendy: CreateIntegrationJob, RetryIntegrationJob, MarkIntegrationJobAsSucceeded, MarkIntegrationJobAsFailed
- Zdarzenia: IntegrationJobCreated, IntegrationJobSucceeded, IntegrationJobFailed

### Agregat: `ExternalSystemMapping`

- Encje: none
- Value Objects: ExternalId, InternalId
- Komendy: CreateExternalMapping, UpdateExternalMapping
- Zdarzenia: ExternalMappingCreated

### Agregat: `InvoiceRequest`

- Encje: none
- Value Objects: InvoiceData, BillingAddress
- Komendy: CreateInvoiceRequest, SubmitInvoiceRequest, MarkInvoiceIssued
- Zdarzenia: InvoiceRequested, InvoiceIssued, PaymentReceived

## Reporting & Analytics

**Odpowiedzialność:** Read models/projekcje dla pipeline, KPI, zamówień i przychodów.

**Aktorzy:** menedżer sprzedaży, zarząd, admin, backoffice manager

### Read models / projekcje
- `SalesPipelineReport`
- `SalespersonKpiReport`
- `OrderProcessingReport`
- `RevenueReport`
- `CustomerBaseReport`
- `ActivityReport`
- `OrderIntakeReport`

## Identity & Access

**Odpowiedzialność:** Użytkownicy, role, uprawnienia, zespoły i kontrola dostępu.

**Aktorzy:** admin, system, menedżer

### Agregat: `User`

- Encje: none
- Value Objects: UserId, UserStatus, EmailAddress
- Komendy: CreateUser, DeactivateUser
- Zdarzenia: UserCreated, UserDeactivated

### Agregat: `Role`

- Encje: Permission
- Value Objects: RoleName
- Komendy: AssignRole
- Zdarzenia: RoleAssigned

### Agregat: `SalesTeam`

- Encje: TeamMember
- Value Objects: TeamName
- Komendy: AssignUserToSalesTeam
- Zdarzenia: UserAssignedToSalesTeam
