# Architecture and Integrations

This project is a single ASP.NET Core MVC application that uses an in-memory repository (InMemoryLibraryRepository) for demo and testing. The architecture is intentionally simple for the assignment: controllers call into a repository abstraction (ILibraryRepository) which could be replaced with an EF Core-backed implementation.

Key integration points:
- Identity (ASP.NET Core Identity) for user and role management (Admin, Reception, Manager).
- Repository abstraction (ILibraryRepository) used by controllers and UI to decouple storage.
- API endpoints protected via a simple API key mechanism (X-Api-Key header).
- CSV importer uses a simple parser in InMemoryLibraryRepository.ImportItemsFromCsv.
- Notifications are simulated and persisted in the in-memory repository for audit and dashboard display.

The multi-branch model is represented by a Branch entity associated to Item via Item.BranchId.

Replacement guidance:
- To persist data, implement ILibraryRepository using ApplicationDbContext and EF Core DbSets (Branches, Reservations, Notifications) and update DI registration in Program.cs.
