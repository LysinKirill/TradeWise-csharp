namespace TradeWiseBackend.Domain.Exceptions;

public class StrategyValidationError : Exception
{
    public StrategyValidationError()
    {
    }

    public StrategyValidationError(string message)
        : base(message)
    {
    }

    public StrategyValidationError(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}