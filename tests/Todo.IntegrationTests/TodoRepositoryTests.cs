using Todo.Api;
using Xunit;

namespace Todo.IntegrationTests;

[Collection("postgres")]
public class TodoRepositoryTests : IAsyncLifetime
{
    private readonly TodoRepository _repo;

    public TodoRepositoryTests(PostgresFixture fixture)
    {
        _repo = fixture.Repository;
    }

    public Task InitializeAsync() => _repo.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddAsync_PersistsTodoAndAssignsId()
    {
        var created = await _repo.AddAsync("Write CI pipeline");

        Assert.True(created.Id > 0);
        Assert.Equal("Write CI pipeline", created.Title);
        Assert.False(created.Done);
    }

    [Fact]
    public async Task AddAsync_TrimsTitleBeforeStoring()
    {
        var created = await _repo.AddAsync("   spaced out   ");

        var stored = Assert.Single(await _repo.ListAsync());
        Assert.Equal("spaced out", created.Title);
        Assert.Equal("spaced out", stored.Title);
    }

    [Fact]
    public async Task ListAsync_ReturnsItemsInInsertionOrder()
    {
        await _repo.AddAsync("first");
        await _repo.AddAsync("second");
        await _repo.AddAsync("third");

        var items = await _repo.ListAsync();

        Assert.Equal(new[] { "first", "second", "third" }, items.Select(i => i.Title));
    }

    [Fact]
    public async Task CompleteAsync_MarksExistingTodoDone()
    {
        var created = await _repo.AddAsync("ship it");

        Assert.True(await _repo.CompleteAsync(created.Id));

        var stored = Assert.Single(await _repo.ListAsync());
        Assert.True(stored.Done);
    }

    [Fact]
    public async Task CompleteAsync_ReturnsFalseForMissingTodo()
    {
        Assert.False(await _repo.CompleteAsync(4242));
    }

    [Fact]
    public async Task FindAsync_ReturnsStoredTodo()
    {
        var created = await _repo.AddAsync("find me");

        var found = await _repo.FindAsync(created.Id);

        Assert.NotNull(found);
        Assert.Equal(created.Id, found.Id);
        Assert.Equal("find me", found.Title);
        Assert.False(found.Done);
    }

    [Fact]
    public async Task FindAsync_ReturnsNullForMissingTodo()
    {
        Assert.Null(await _repo.FindAsync(9999));
    }

    [Fact]
    public async Task FindAsync_ReflectsCompletion()
    {
        var created = await _repo.AddAsync("finish me");
        await _repo.CompleteAsync(created.Id);

        var found = await _repo.FindAsync(created.Id);

        Assert.NotNull(found);
        Assert.True(found.Done);
    }

    [Fact]
    public async Task ResetAsync_ClearsTableAndRestartsIdentity()
    {
        await _repo.AddAsync("gone soon");
        await _repo.ResetAsync();

        Assert.Empty(await _repo.ListAsync());

        var fresh = await _repo.AddAsync("fresh start");
        Assert.Equal(1, fresh.Id);
    }
}
