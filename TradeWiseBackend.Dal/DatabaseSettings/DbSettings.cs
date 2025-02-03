namespace TradeWiseBackend.Dal.DatabaseSettings;

public record DbSettings
{
    public required string UserId { get; init; }
    public required string Password { get; init; }
    public required string Host { get; init; }
    public required string Database { get; init; }
    public required string Options { get; init; }

    public string ConnectionString =>
        $"""
         User ID={UserId};
         Password={Password};
         Host={Host};
         DataBase={Database};
         {Options};
         """;
}