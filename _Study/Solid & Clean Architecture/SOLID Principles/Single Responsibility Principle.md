- What defines a *"reason to change"*?
- When you write a software module, you want to make sure that when changes are requested, those changes can only originate from a **single person**, or rather, a single tightly coupled group of people representing a **single narrowly defined business function**.
- Each module is responsible (responds to) the needs of just that one business function.
- This is the reason that business rules should not know the database schema. This is the reason we separate concerns.
- Gather together the things that change for the same reasons. Separate those things that change for different reasons.
- We want to increase the cohesion between things that change for the same reasons, and we want to decrease the coupling between those things that change for different reasons.
- **Ask who owns the rule.**

### Questions to ask
- What business capability does this code represent?
- Who owns decisions about that capability?
- What kinds of changes would cause this code to change?
- Do those changes belong together?
- Is another independent actor (stakeholder) also causing this module to change?