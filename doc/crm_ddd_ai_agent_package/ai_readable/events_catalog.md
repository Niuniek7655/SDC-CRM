# Katalog zdarzeń i komunikacji między bounded contextami

## Integration events — przepływy między contextami

| Event | Publikuje | Subskrybuje | Kiedy |
|---|---|---|---|
| `CustomerCreated / CustomerUpdated` | Customer.Customer Management | Lead & Pipeline, Sales Activity, Reporting & Analytics | po utworzeniu lub zmianie danych klienta |
| `OpportunityWon` | Opportunity.Lead & Pipeline | Sales Order Capture, Reporting & Analytics | gdy sprzedaż została wygrana |
| `OpportunityLost` | Opportunity.Lead & Pipeline | Reporting & Analytics | gdy szansa została przegrana |
| `SalesOrderCreated` | SalesOrder.Sales Order Capture | Reporting & Analytics | po utworzeniu szkicu zamówienia |
| `SalesOrderSubmitted` | SalesOrder.Sales Order Capture | Backoffice Order Processing, Reporting & Analytics | gdy handlowiec przekazuje kompletne zamówienie |
| `MissingInformationRequested` | OrderProcess.Backoffice Order Processing | Sales Activity | gdy backoffice wykryje braki w danych |
| `MissingInformationProvided` | Sales Activity / Backoffice | Backoffice Order Processing | gdy handlowiec uzupełni brakujące dane |
| `OrderAcceptedForFulfillment` | OrderProcess.Backoffice Order Processing | Integration Context, Reporting & Analytics | gdy zamówienie zostaje zaakceptowane do realizacji |
| `InvoiceRequested` | Backoffice / Integration | Integration Context | gdy trzeba wystawić fakturę lub przekazać dane do ERP |
| `InvoiceIssued` | Integration Context | Reporting & Analytics, Backoffice Order Processing | gdy faktura została wystawiona |
| `PaymentReceived` | Integration Context | Reporting & Analytics, Backoffice Order Processing | gdy pojawi się informacja o płatności |
| `OrderCompleted` | OrderProcess.Backoffice Order Processing | Reporting & Analytics, Sales Activity, Integration Context | gdy proces obsługi zamówienia zostaje zakończony |

## Wszystkie zdarzenia domenowe/integracyjne z agregatów

| Event | Context | Aggregate |
|---|---|---|
| `CustomerCreated` | Customer Management | Customer |
| `CustomerUpdated` | Customer Management | Customer |
| `ContactPersonAdded` | Customer Management | Customer |
| `CustomerStatusChanged` | Customer Management | Customer |
| `LeadRegistered` | Lead & Pipeline | Lead |
| `LeadAssigned` | Lead & Pipeline | Lead |
| `LeadQualified` | Lead & Pipeline | Lead |
| `LeadRejected` | Lead & Pipeline | Lead |
| `OpportunityCreated` | Lead & Pipeline | Opportunity |
| `OpportunityStageChanged` | Lead & Pipeline | Opportunity |
| `OpportunityWon` | Lead & Pipeline | Opportunity |
| `OpportunityLost` | Lead & Pipeline | Opportunity |
| `PipelineConfigured` | Lead & Pipeline | Pipeline |
| `PipelineStageChanged` | Lead & Pipeline | Pipeline |
| `ActivityRegistered` | Sales Activity | SalesActivity |
| `ActivityCompleted` | Sales Activity | SalesActivity |
| `FollowUpScheduled` | Sales Activity | FollowUpTask |
| `FollowUpCompleted` | Sales Activity | FollowUpTask |
| `NoteAdded` | Sales Activity | Note |
| `NoteUpdated` | Sales Activity | Note |
| `SalesOrderCreated` | Sales Order Capture | SalesOrder |
| `SalesOrderSubmitted` | Sales Order Capture | SalesOrder |
| `SalesOrderCancelled` | Sales Order Capture | SalesOrder |
| `OrderProcessingStarted` | Backoffice Order Processing | OrderProcess |
| `OrderAssigned` | Backoffice Order Processing | OrderProcess |
| `MissingInformationRequested` | Backoffice Order Processing | OrderProcess |
| `MissingInformationProvided` | Backoffice Order Processing | OrderProcess |
| `OrderAcceptedForFulfillment` | Backoffice Order Processing | OrderProcess |
| `OrderCompleted` | Backoffice Order Processing | OrderProcess |
| `OrderRejected` | Backoffice Order Processing | OrderProcess |
| `BackofficeTaskCreated` | Backoffice Order Processing | BackofficeTask |
| `BackofficeTaskCompleted` | Backoffice Order Processing | BackofficeTask |
| `IntegrationJobCreated` | Integration Context | IntegrationJob |
| `IntegrationJobSucceeded` | Integration Context | IntegrationJob |
| `IntegrationJobFailed` | Integration Context | IntegrationJob |
| `ExternalMappingCreated` | Integration Context | ExternalSystemMapping |
| `InvoiceRequested` | Integration Context | InvoiceRequest |
| `InvoiceIssued` | Integration Context | InvoiceRequest |
| `PaymentReceived` | Integration Context | InvoiceRequest |
| `UserCreated` | Identity & Access | User |
| `UserDeactivated` | Identity & Access | User |
| `RoleAssigned` | Identity & Access | Role |
| `UserAssignedToSalesTeam` | Identity & Access | SalesTeam |