namespace TradeWiseBackend.Api.Responses.v1;

using TradeWiseBackend.Api.Responses.models;
public record class GetSupportedModelsResponse(
    List<Model> Models
);
