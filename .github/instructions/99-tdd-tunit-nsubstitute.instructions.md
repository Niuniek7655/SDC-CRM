---
applyTo: "**/*Tests.cs,**/*.Tests.cs,**/tests/**/*.cs"
---

# TDD, TUnit and NSubstitute Rules

Use TDD as the default implementation workflow.

Always follow this loop:

```text
1. Write a failing test.
2. Implement the smallest amount of production code.
3. Run tests.
4. Refactor.
5. Repeat until the use case is complete.
```

Testing stack:

- TUnit for tests,
- NSubstitute for substitutes/test doubles.

TUnit rules:

- use `[Test]` for test methods,
- use TUnit assertions,
- prefer async tests when testing async code,
- keep tests behavior-focused.

NSubstitute rules:

- use substitutes for ports, repositories, brokers, clocks and external dependencies,
- do not mock domain entities or value objects,
- prefer real domain objects in domain tests,
- verify interactions only when interaction itself is the behavior.

Test naming:

```text
MethodOrUseCase_ShouldExpectedBehavior_WhenCondition
```

Examples:

```text
RejectLead_ShouldRequireRejectionReason
QualifyLead_ShouldFail_WhenLeadIsRejected
SubmitOrderToBackoffice_ShouldFail_WhenOrderIsIncomplete
ReturnOrderToSales_ShouldRequireComment
CompleteBackofficeOrder_ShouldSetCompletionDate
```

For each vertical slice, create tests before implementation:

- domain tests for aggregate invariants,
- handler tests for application behavior,
- integration/API tests when endpoint behavior is part of the story.

Do not implement production code first unless explicitly asked.
