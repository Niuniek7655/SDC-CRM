---
description: "Implement a CRM backend vertical slice using TDD"
---

Implement this backend vertical slice using the project rules:

Use case: ${input:useCase}

Follow this process:

1. Identify the module and aggregate involved.
2. Write failing TUnit tests first.
3. Use NSubstitute for external dependencies.
4. Implement the smallest amount of production code.
5. Keep the slice aligned with Clean Architecture, CQRS and DDD.
6. Add or update API endpoint only if needed.
7. Add domain events or integration events only if they represent real business facts.
8. Do not introduce RabbitMQ directly; use messaging abstraction.
9. Run or describe the relevant test command.
