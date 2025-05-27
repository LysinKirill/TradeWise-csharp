namespace TradeWiseBackend.Api.Responses.v1;

public record class GetSupportedModelsResponse(
    List<models.Model> Models
);