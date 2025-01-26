namespace TradeWiseBackend.Api.Requests.v1;

public record RegisterUserRequest(
    string FirstName,
    string SecondName,
    string Email,
    string Password
);