https://milanjovanovic.tech/blog/what-invariants-are-and-why-a-domain-model-is-the-best-place-to-enforce-them
An invariant is a business rule about an object that must hold true for as long as the object exists. The rule has to hold every time the object appears, no matter how it gets into memory. It's an statement about the **domain**.
A model of this object must never accept an invalid state. If an instance of the object or entity is used, it should be trusted. No `if (a == null)` should be necessary at any point down the call stack.
##### Ways to implement an invariant
- Block the construction of invalid entities by making its properties read-only. The creation of a new object must go through a factory method that validates against errors.
- Encapsulate state transitions. If there's a state pipeline, it should be checked against the invariant rules that apply to each state.
- Encapsulate the aggregates. When an invariant spans across multiple objects within the same transactional boundary, we call them [[Aggregate]] Invariants. Any changes applied to this collection of objects must be done in the same transaction.

⚠ Invariant != Validation != External Polocy