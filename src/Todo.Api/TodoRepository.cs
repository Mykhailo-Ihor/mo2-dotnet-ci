using Npgsql;

namespace Todo.Api;

public sealed class TodoRepository
{
    private readonly string _connectionString;

    public TodoRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "CREATE TABLE IF NOT EXISTS todos (id SERIAL PRIMARY KEY, title TEXT NOT NULL, done BOOLEAN NOT NULL DEFAULT FALSE)",
            conn);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<TodoItem> AddAsync(string title, CancellationToken ct = default)
    {
        var normalized = TodoValidator.Normalize(title);

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO todos (title) VALUES (@title) RETURNING id, title, done",
            conn);
        cmd.Parameters.AddWithValue("title", normalized);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        await reader.ReadAsync(ct);
        return new TodoItem(reader.GetInt32(0), reader.GetString(1), reader.GetBoolean(2));
    }

    public async Task<IReadOnlyList<TodoItem>> ListAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("SELECT id, title, done FROM todos ORDER BY id", conn);

        var items = new List<TodoItem>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            items.Add(new TodoItem(reader.GetInt32(0), reader.GetString(1), reader.GetBoolean(2)));
        }

        return items;
    }

    public async Task<bool> CompleteAsync(int id, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("UPDATE todos SET done = TRUE WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        return await cmd.ExecuteNonQueryAsync(ct) == 1;
    }

    public async Task ResetAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand("TRUNCATE todos RESTART IDENTITY", conn);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
