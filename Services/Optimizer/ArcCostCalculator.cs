using API_trip_link.Models;
using API_trip_link.Services.Transit;

namespace API_trip_link.Services.Optimizer
{
    internal class ArcCostCalculator
    {
        private readonly ITransitApiService _transitApi;
        private readonly OptimizerParams _tripParams;

        public ArcCostCalculator(ITransitApiService transitApi, OptimizerParams tripParams)
        {
            _transitApi  = transitApi;
            _tripParams  = tripParams;
        }
        //פעולה המחשבת עלות הקשת
        //מקבלת את נקודת ההתחלה ואת נקודת היעד ואת שעת היציאה 
        // מחזירה אובייקט קשת הכולל שעת היציאה המיטבית בהתאם לתחבורה הציבורית, וכן זמני נסיעה ויעילות תחבורה    
        public ArcCost Compute(OptimizerDestination? fromDest, OptimizerDestination toDest, DateTime nominalDeparture)
            => ComputeAsync(fromDest, toDest, nominalDeparture).GetAwaiter().GetResult();

        /// <summary>
        /// Single Google Maps lookup per table cell (nominal departure hour).
        /// Fine-grained departure search belongs in ScoreTable build, not per-cell multiplication.
        /// </summary>
        public async Task<ArcCost> ComputeAsync(
            OptimizerDestination? fromDest,
            OptimizerDestination toDest,
            DateTime nominalDeparture)
        {
            if (fromDest == null && nominalDeparture < _tripParams.TripStartTime)
                nominalDeparture = _tripParams.TripStartTime;

            var from = BuildLocation(fromDest, isOrigin: fromDest == null);
            var to   = BuildLocation(toDest, isOrigin: false);

            var result = await _transitApi.GetTransitTimeAsync(
                from,
                to,
                baseTransitHours: toDest.TransitTimeHours > 0 ? toDest.TransitTimeHours : 1.0,
                walkingHours: toDest.WalkingTimeHours,
                departureTime: nominalDeparture);

            double efficiency = (result.BusTransitHours > 0 && result.CarTransitHours > 0)
                ? Math.Max(0.0, Math.Min(1.0, result.CarTransitHours / result.BusTransitHours))
                : 0.5;

            return new ArcCost
            {
                FromDestinationId = fromDest?.DestinationId ?? -1,
                ToDestinationId   = toDest.DestinationId,
                BestDepartureTime = nominalDeparture,
                BusTransitHours   = result.BusTransitHours,
                CarTransitHours   = result.CarTransitHours,
                WalkingHours      = result.WalkingHours,
                TransitEfficiency = efficiency,
                HasDirectBus      = result.HasDirectBus
            };
        }

        public ArcCost LookupOrCompute(
            ScoreTable? scoreTable,
            OptimizerDestination? fromDest,
            OptimizerDestination toDest,
            DateTime departureTime)
        {
            if (scoreTable == null)
                return Compute(fromDest, toDest, departureTime);

            int fromIndex = fromDest == null
                ? scoreTable.OriginIndex
                : scoreTable.DestIdToIndex(fromDest.DestinationId);

            int toIndex   = scoreTable.DestIdToIndex(toDest.DestinationId);
            int hourIndex = scoreTable.TimeToHourIndex(departureTime);

            if (fromIndex < 0 || toIndex < 1)
                return Compute(fromDest, toDest, departureTime);

            var cell = scoreTable.Get(fromIndex, toIndex, hourIndex);
            return cell.IsValid ? cell.ArcCost : Compute(fromDest, toDest, departureTime);
        }

        private TransitLocation BuildLocation(OptimizerDestination? dest, bool isOrigin)
        {
            if (isOrigin)
            {
                return new TransitLocation
                {
                    DestinationId = -1,
                    Address       = _tripParams.AddressStart,
                    Latitude      = _tripParams.StartLatitude,
                    Longitude     = _tripParams.StartLongitude
                };
            }

            return new TransitLocation
            {
                DestinationId = dest!.DestinationId,
                Address       = dest.Name,
                Latitude      = dest.Latitude,
                Longitude     = dest.Longitude
            };
        }
    }
}
