using System.ComponentModel.DataAnnotations;

namespace TradeWiseBackend.Dal.Entities;

public class StrategyTransition
{
    [Key] public Guid StrategyTransitionId { get; set; } // UUID strategy_transition_id

    public Guid StageSourceId { get; set; } // UUID stage_source_id
    public Guid StageDestinationId { get; set; } // UUID stage_destination_id
    public OperationType Operation { get; set; } // OperationType operation
    public double Value { get; set; } // double value
    public DateTime Timestamp { get; set; } // timestamp timestamp
    public double MiddleBand { get; set; } // double middle_band
    public double UpperBand { get; set; } // double upper_band
    public double LowerBand { get; set; } // double lower_band
    public double Signal { get; set; } // double signal
    public double Macd { get; set; } // double macd
}