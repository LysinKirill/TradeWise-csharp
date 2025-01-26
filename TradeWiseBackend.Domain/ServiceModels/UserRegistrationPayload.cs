namespace TradeWiseBackend.Domain.ServiceModels;

public record UserRegistrationPayload(
    string FirstName,
    string SecondName,
    string Email,
    string Password
);