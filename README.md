# mo2-dotnet-ci

.NET minimal API backed by PostgreSQL, used to demonstrate a CI pipeline with unit tests,
database-backed integration tests, test-result artifacts and quality gates.

## Layout

| Path | Purpose |
| --- | --- |
| `src/Todo.Api` | Minimal API and `TodoRepository` (Npgsql) |
| `tests/Todo.UnitTests` | Pure validation tests, no database |
| `tests/Todo.IntegrationTests` | Tests running against a real PostgreSQL instance |

## CI

`.github/workflows/ci.yml` runs two jobs on every push and pull request to `main`:

- **unit-tests** — `dotnet restore`, `dotnet build`, `dotnet test`, publishing `unit-tests.trx` as an artifact.
- **integration-tests** — the same steps against a `postgres:16` **service container**, publishing `integration-tests.trx`.

`.github/workflows/dependency-review.yml` runs `dependency-review-action` on pull requests
and fails the check when a dependency introduces a vulnerability of **high** severity or above.

Both `unit-tests` and `integration-tests` are required status checks on `main`.

## Running locally

The integration tests read the connection string from `TODO_DB`.

```bash
docker run -d --name todo-pg -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=todos -p 5433:5432 postgres:16

TODO_DB="Host=localhost;Port=5433;Username=postgres;Password=postgres;Database=todos" \
  dotnet test
```

Port 5433 is used to avoid clashing with a locally installed PostgreSQL server.
