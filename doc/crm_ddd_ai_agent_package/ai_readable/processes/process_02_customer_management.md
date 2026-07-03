# Proces 2 – Zarządzanie klientem

## Cel

Diagram procesu biznesowego z naniesionymi agregatami, bounded contextami i zdarzeniami.

## Bounded contexty
- `Customer Management`
- `Sales Activity`
- `Sales Order Capture`
- `Reporting & Analytics`

## Agregaty biorące udział
- `Customer`
- `SalesActivity`
- `SalesOrder`

## Encje i Value Objects
- `ContactPerson`
- `CustomerAddress`
- `CustomerStatus`
- `TaxId`
- `EmailAddress`
- `PhoneNumber`
- `Address`

## Zdarzenia
- `CustomerCreated`
- `CustomerUpdated`

## Kroki procesu
| Krok | Nazwa | Dodatkowe informacje |
|---|---|---|
| 1 | Pozyskanie danych klienta | type: start |
| 2 | Weryfikacja czy klient już istnieje | aggregate: Customer |
| 3 | Klient istnieje? | type: decision |
| 3T | Aktualizacja danych klienta | aggregate: Customer; event: CustomerUpdated |
| 3N | Utworzenie klienta | aggregate: Customer; event: CustomerCreated |
| 4 | Dodanie danych kontaktowych i adresów | entities: ContactPerson, CustomerAddress |
| 5 | Nadanie statusu klienta | value_objects: CustomerStatus, TaxId, EmailAddress, PhoneNumber, Address |
| 6 | Powiązanie klienta z aktywnościami / sprzedażą | references_by_id: SalesActivity, SalesOrder |
| 7 | Klient gotowy do użycia w sprzedaży i zamówieniach | type: end |

## Mermaid
```mermaid
flowchart LR
  Start([Pozyskanie danych klienta]) --> Verify[Weryfikacja czy klient istnieje\nAggregate: Customer]
  Verify --> Exists{Klient istnieje?}
  Exists -- TAK --> Update[Aktualizacja danych klienta\nAggregate: Customer]
  Exists -- NIE --> Create[Utworzenie klienta\nAggregate: Customer]
  Update -. CustomerUpdated .-> Activity[SalesActivity reference by CustomerId]
  Create -. CustomerCreated .-> Activity
  Update --> ContactData[Dodanie danych kontaktowych i adresów\nEntities: ContactPerson, CustomerAddress]
  Create --> ContactData
  ContactData --> Status[Nadanie statusu klienta\nVO: CustomerStatus, TaxId, EmailAddress, PhoneNumber, Address]
  Status --> Link[Powiązanie przez ID z SalesActivity i SalesOrder]
  Link --> End([Klient gotowy do użycia])
  Status -. projection .-> Reports[Reporting: CustomerBaseReport / SalesPipelineReport]
  Order[Sales Order Capture] -->|sync query: customer snapshot| Verify

```