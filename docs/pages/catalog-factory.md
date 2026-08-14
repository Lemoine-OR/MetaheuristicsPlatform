@page metaheuristic_catalog_factory Catalog and factory

# Catalog and factory

`MetaheuristicCatalog` is the canonical runtime inventory.

Every public algorithm has one stable ID. `MetaheuristicFactory.Create<TAlgorithm>(id)`
provides typed construction. Algorithms requiring domain-specific composition are
registered through `MetaheuristicFactory.Register(id, factory)`.
