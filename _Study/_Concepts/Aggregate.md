https://domaincentric.net/blog/modelling-business-rules-invariants-vs-corrective-policies
https://www.jamesmichaelhickey.com/domain-driven-design-aggregates/
https://softwareengineering.stackexchange.com/questions/453564/modelling-invariants-over-different-aggregates
https://zenn.dev/135yshr/articles/4afd548d07480a?locale=en
An aggregate is a group of related domain objects treated as one consistency boundary. It has one entry point—the aggregate root—which is responsible for protecting the rules of the whole group.
An aggregate is the basic element of data storage in DDD. We request to load or save whole aggregates. Transactions should not cross aggregate boundaries.
They should only be used as write-side models, since that's what they're useful for.
Their design is driven by the true invariants that mark our consistency boundaries.