using System;

namespace TradeWiseBackend.Bll.Entities;

public enum StatType
{
    Timestamp = 0,
    MiddleBand = 1,
    UpperBand = 2,
    LowerBand = 3,
    Signal = 4,
    Macd = 5
}
