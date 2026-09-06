using Todo.Api;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Todos")
    ?? Environment.GetEnvironmentVariable("TODO_DB")
    ?? "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=todos";

builder.Services.AddSingleton(new TodoRepository(connectionString));

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/todos", async (TodoRepository repo, CancellationToken ct) =>
    Results.Ok(await repo.ListAsync(ct)));

app.MapPost("/todos", async (TodoRepository repo, CreateTodoRequest request, CancellationToken ct) =>
{
    if (!TodoValidator.IsValidTitle(request.Title))
    {
        return Results.BadRequest(new { error = "title must be non-empty and at most 120 characters" });
    }

    var created = await repo.AddAsync(request.Title, ct);
    return Results.Created($"/todos/{created.Id}", created);
});

app.MapPost("/todos/{id:int}/complete", async (TodoRepository repo, int id, CancellationToken ct) =>
    await repo.CompleteAsync(id, ct) ? Results.NoContent() : Results.NotFound());

app.Run();

public record CreateTodoRequest(string Title);
