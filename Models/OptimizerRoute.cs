namespace API_trip_link.Models
{
    internal class OptimizerRoute
    {
        public bool   IsValid           { get; set; }
        public double TotalScore        { get; set; }
        public double TotalTime         { get; set; }
        public double TransitEfficiency { get; set; }
        public List<OptimizerDestination> Destinations { get; set; } = new();
        public List<ArcCost> ArcCosts { get; set; } = new();

        public OptimizerRoute Copy() => new()
        {
            IsValid           = IsValid,
            TotalScore        = TotalScore,
            TotalTime         = TotalTime,
            TransitEfficiency = TransitEfficiency,
            Destinations      = new List<OptimizerDestination>(Destinations),
            ArcCosts          = new List<ArcCost>(ArcCosts)
        };
    }
}
