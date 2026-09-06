namespace Todo.Api;

public static class TodoValidator
{
    public const int MaxTitleLength = 5;

    public static bool IsValidTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return false;
        }

        return title.Trim().Length <= MaxTitleLength;
    }

    public static string Normalize(string title)
    {
        if (!IsValidTitle(title))
        {
            throw new ArgumentException("Invalid title", nameof(title));
        }

        return title.Trim();
    }
}
