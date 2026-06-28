using API_trip_link.Settings;
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
                walkingHours: toDest.WalkingTimeHours,
                departureTime: nominalDeparture);
                //חישוב יעילות תחבורה
            double efficiency = (result.BusTransitHours > 0 && result.CarTransitHours > 0)
                ? Math.Max(Configuration.Common.ScoreMin, Math.Min(Configuration.Common.ScoreMax, result.CarTransitHours / result.BusTransitHours))
                : 0;

            return new ArcCost
            {
                FromDestinationId = fromDest?.DestinationId ?? Configuration.Common.OriginDestinationId,
                ToDestinationId   = toDest.DestinationId,
                BestDepartureTime = nominalDeparture,
                BusTransitHours   = result.BusTransitHours,
                CarTransitHours   = result.CarTransitHours,
                WalkingHours      = result.WalkingHours,
                TransitEfficiency = efficiency,
                HasDirectBus      = result.HasDirectBus,
                TransitSteps      = result.TransitSteps.Select(s => new TransitLegStep
                {
                    LineName      = s.LineName,
                    VehicleType   = s.VehicleType,
                    FromStation   = s.FromStation,
                    ToStation     = s.ToStation,
                    DepartureTime = s.DepartureTime,
                    ArrivalTime   = s.ArrivalTime,
                    DurationHours = s.DurationHours
                }).ToList()
            };
        }
        //פונקציה המחזירהאת עלות הקשת אם היא קיימת במיקום הנוכחי ואם לא היא מחשבת קשת 
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
            int minuteIndex = scoreTable.TimeToMinuteIndex(departureTime);

            if (fromIndex < 0 || toIndex < Configuration.Common.MinDestinationNodeIndex)
                return Compute(fromDest, toDest, departureTime);

            var cell = scoreTable.FindNearestValidCell(fromIndex, toIndex, minuteIndex);
            return cell?.IsValid == true ? cell.ArcCost : Compute(fromDest, toDest, departureTime);
        }
        //פונקציה שמה את המיקום
        private TransitLocation BuildLocation(OptimizerDestination? dest, bool isOrigin)
        {
            //אם זה יעד המקןר
            if (isOrigin)
            {
                return new TransitLocation
                {
                    DestinationId = Configuration.Common.OriginDestinationId,
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
