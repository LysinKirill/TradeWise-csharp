namespace TradeWiseBackend.Api.Responses.v1;

public record class RefreshTokenResponse(
    string AccessToken,
    string RefreshToken
);
