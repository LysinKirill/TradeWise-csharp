using TradeWiseBackend.Api.Responses.models;

namespace TradeWiseBackend.Api.Responses.v1;

public record class GetAccountOverviewResponse(
    int ActiveStrategies,
    double Balance,
    List<PortfolioPosition> PortfolioPositions,
    double TodayPnl
);
