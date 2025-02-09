namespace TradeWiseBackend.Dal.DatabaseSettings;

public record DbSettings
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string Host { get; init; }
    public required string Database { get; init; }
    public required string Options { get; init; }

    public string ConnectionString =>
        $"""
         Username={Username};
         Password={Password};
         Host={Host};
         Database={Database};
         {Options};
         """;
}