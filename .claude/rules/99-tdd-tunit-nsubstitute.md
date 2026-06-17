---
paths:
  - "**/*.cs"
---

# TDD, TUnit and NSubstitute Rules

Use TDD or test-first as the default implementation workflow for backend or mobile code.

Always follow this loop:

```text
1. Write a failing test.
2. Implement the smallest amount of production code.
3. Run tests.
4. Refactor without changing behavior.
5. Repeat until the use case is complete.
```

Prefer the red-green-refactor rhythm:

```text
red -> green -> refactor
```

Do not implement production code first unless explicitly asked.

## Testing stack

- TUnit for tests.
- NSubstitute for substitutes/test doubles.

## What counts as a unit test

A unit test must execute production code in controlled conditions, in memory, inside a single process, and verify one clearly specified behavior automatically.

A test is not a unit test when it depends on:

- a real database,
- the file system,
- network access,
- an external service,
- a real message broker,
- real system time,
- random values that are not controlled by the test,
- execution order of other tests.

Such tests are integration, contract, component or API tests and must be placed in the appropriate test layer.

## FIRST rules for unit tests

Every unit test must satisfy FIRST.

### Fast

Tests must be fast to run and fast to read.

- Avoid expensive setup.
- Avoid booting the application host for unit tests.
- Avoid real infrastructure in unit tests.
- Keep the arrange phase focused on the tested scenario.
- Prefer simple builders/object mothers only when they improve readability.
- Do not hide important scenario details behind overly generic setup helpers.

### Isolated

Tests must be isolated from the environment and from each other.

- Do not use real databases, files, network calls, queues, brokers or external systems in unit tests.
- Replace external dependencies with NSubstitute substitutes, fakes or in-memory implementations behind ports/interfaces.
- Do not share mutable state between tests.
- Do not rely on static/global state.
- Do not mock domain entities or value objects.
- Prefer real domain objects in domain tests.

### Repeatable

Tests must produce the same result on every machine, in every time zone, in any order, and at any date.

- Control time through an injected clock/time provider.
- Control random values, GUIDs, IDs and generated numbers.
- Avoid assertions depending on current culture, local time, machine configuration or test execution order.
- Avoid sleeps, polling and timing-based assertions in unit tests.

### Self-verifying

Tests must verify their result explicitly.

- Every test must contain at least one assertion.
- A test without an assertion is invalid.
- If the expected behavior is "does not throw", express that with an explicit assertion.
- Do not write tests only to increase code coverage.
- Do not verify implementation details unless the interaction itself is the behavior.

### Timely

Tests should be written at the right time: before the production code.

- For new behavior, write tests before implementation.
- For a bug fix, first write a failing test reproducing the bug, then fix the code.
- For refactoring, first secure the current behavior with tests when the behavior is not already covered.

## Test naming convention

Use this convention for test method names:

```text
NazwaTestowanejMetody__When_Testowany_Scenariusz__Should_Oczekiwany_Rezultat
```

For actual C# test method names, use English identifiers with the same structure:

```text
TestedMethodName__When_tested_scenario__Should_expected_result
```

Naming rules:

- Start with the tested method, use case or behavior name.
- Use double underscores `__` to separate the tested member, scenario and expected result.
- Use single underscores `_` between words inside the scenario and expected result.
- The `When_...` section must describe the tested scenario, condition or input state.
- The `Should_...` section must describe the observable result.
- Prefer readable, descriptive names over short names.
- Long names are acceptable in tests because test names act as executable documentation and appear in test runner/build reports.
- Never use names such as `Test1`, `Parse_Test`, `HappyPath`, `ShouldWork`, `SuccessTest` or other names that do not explain behavior.
- Keep the test name synchronized with the behavior. When behavior changes, rename or rewrite the test.

Examples:

```text
RejectLead__When_rejection_reason_is_missing__Should_fail_validation
QualifyLead__When_lead_is_rejected__Should_return_failure
SubmitOrderToBackoffice__When_order_is_incomplete__Should_return_validation_error
ReturnOrderToSales__When_comment_is_missing__Should_fail_validation
CompleteBackofficeOrder__When_order_can_be_completed__Should_set_completion_date
Shorten__When_input_is_shorter_than_required_length__Should_return_whole_text
Shorten__When_input_matches_required_length__Should_return_whole_text
Shorten__When_input_is_longer_than_required_length__Should_return_shorter_text_ending_with_dots
```

## Test structure: AAA

Structure tests using Arrange, Act and Assert.

```text
Arrange -> Act -> Assert
```

Rules:

- Arrange: create the tested object and prepare only the data/dependencies needed for the scenario.
- Act: execute the tested behavior. Prefer one logical act line.
- Assert: verify the observable result.
- Prefer one logical assertion per test. Multiple physical assertions are acceptable only when they verify one coherent result.
- Do not mix arrange, act and assert logic.
- Use `Arrange`, `Act`, `Assert` comments only when they improve readability.

## TUnit rules

- Use `[Test]` for test methods.
- Use TUnit assertions.
- Prefer async tests when testing async code.
- Keep tests behavior-focused.
- Do not use test order as part of the test logic.
- Do not introduce hidden shared mutable state between tests.

## NSubstitute rules

- Use substitutes for ports, repositories, brokers, clocks, gateways, API clients and other external dependencies.
- Do not mock domain entities or value objects.
- Prefer real domain objects in domain tests.
- Verify interactions only when the interaction itself is the observable behavior.
- Prefer state/result assertions over interaction assertions when both are possible.
- Do not over-specify calls that are implementation details.

## Test design rules

- Tests are production-quality code and must be refactored when they become hard to read or hard to extend.
- Avoid copy-paste setup across many tests. Extract focused builders, fixtures or helper methods when they reduce noise.
- Keep important scenario data visible in the test.
- Group tests with high cohesion: one test class should cover one aggregate, handler, service, method group or use case.
- Tests should explain how the component behaves without requiring the reader to inspect production implementation.
- Prefer testing behavior and business rules over private methods or internal implementation details.
- Do not add tests only because a coverage report is low.
- Treat failing tests seriously, like compilation errors.

## Vertical slice testing strategy

For each vertical slice, create tests before implementation:

- domain tests for aggregate invariants, value object rules and domain behavior,
- handler/application tests for use case behavior, validation, authorization decisions and port interactions,
- integration/API tests when endpoint behavior, persistence, serialization, authentication, authorization, messaging or infrastructure behavior is part of the story.

Keep the boundary clear:

- domain and application unit tests must remain FIRST-compliant,
- infrastructure, API, database, broker and end-to-end checks belong to integration or higher-level tests.

## Working with existing code

Do not add unit tests to existing legacy code only to increase coverage.

Use these rules instead:

- For new code added to an existing system, use test-first.
- For bug fixes, first create a failing test that proves the bug exists.
- For refactoring, add characterization tests around the behavior that must not change, then refactor.
- Introduce seams, ports or small testable components when the current code is too coupled to test directly.

