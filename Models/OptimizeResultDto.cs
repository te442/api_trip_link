using System.Collections.Generic;

namespace API_trip_link.Models
{
    public class OptimizeResultDto
    {
        public int                  DestinationCount    { get; set; }
        public double               TotalScore          { get; set; }
        public double               TimeUsed            { get; set; }
        public double               TimeAvailable       { get; set; }
        public double               TransitEfficiency   { get; set; }
        public List<DestinationDto> OptimalRoute        { get; set; }

        /// <summary>Arc costs for each consecutive leg in the optimal route.</summary>
        public List<ArcCostDto>     ArcCosts            { get; set; }

        /// <summary>Full Hebrew narrative of the trip itinerary.</summary>
        public string?              Narrative           { get; set; }

        public List<TripLegDto>     Legs                { get; set; } = new();
        public List<MapPointDto>    MapPoints           { get; set; } = new();
        public int                  TripId              { get; set; }
        public string?              TripName            { get; set; }
        public string?              AddressStart        { get; set; }

        /// <summary>3D score table dimensions and validity stats (Step 2).</summary>
        public ScoreTableStatsDto?  ScoreTableStats     { get; set; }

        /// <summary>Step-by-step pipeline trace from click to result.</summary>
        public List<OptimizationStepTraceDto> PipelineTrace { get; set; } = new();

        /// <summary>Cell-by-cell ScoreTable build log (Step 2).</summary>
        public List<ScoreTableCellTraceDto> ScoreTableCellTrace { get; set; } = new();
    }
}
