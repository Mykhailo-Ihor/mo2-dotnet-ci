using Todo.Api;
using Xunit;

namespace Todo.IntegrationTests;

public sealed class PostgresFixture : IAsyncLifetime
{
    public TodoRepository Repository { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable("TODO_DB")
            ?? "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=todos";

        Repository = new TodoRepository(connectionString);
        await Repository.InitializeAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

[CollectionDefinition("postgres")]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>;
