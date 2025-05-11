using TradeWiseBackend.Domain.Models;

namespace TradeWiseBackend.Domain.ServiceModels;

public record class AccountOverviewInfo(
    int ActiveStrategies,
    double Balance,
    List<PortfolioPosition> PortfolioPositions,
    double TodayPnl
);
