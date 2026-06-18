using System.Text;
using API_trip_link.Data.Repositories;
using API_trip_link.Models;

namespace API_trip_link.Services.Optimizer.Steps
{
    internal class Step6_TripItineraryBuilder : IOptimizerStep
    {
        private readonly IOptimizerDataRepository _data;

        public Step6_TripItineraryBuilder(IOptimizerDataRepository data)
        {
            _data = data;
        }

        public int StepNumber => 6;
        public string StepName => "OUTPUT";

        //הפונקציה ממירה את תוצאות האופטמזצית מסלול כאובייקט שיוצג בממשק משתמש
       //מקבלת אובייקט המכיל את כל הפרמטרים ואת המסלול האופטימלי
       //מחזירה אובייקט שיוצג בממשק משתמש
        public async Task ExecuteAsync(OptimizationContext ctx)
        {
           //סימון היעדים שקיימים במסלול
            var selectedIds = new HashSet<int>(ctx.BestRoute.Destinations.Select(d => d.DestinationId));

            //יצירת רשימת היעדים שיוצגו בממשק משתמש
            var available = ctx.Destinations.Select(d => new AvailableDestination
            {
                DesId          = d.DestinationId,
                NameDes        = d.Name,
                Region         = ctx.TripRegion,
                VisitHours     = d.VisitDuration,
                NearestStation = d.NearestStation,
                WasSelected    = selectedIds.Contains(d.DestinationId)
            }).ToList();

            //יצירת רשימת הקטעים שיוצגו בממשק משתמש
            var legs        = new List<TripLeg>();
            var currentTime = ctx.Params.TripStartTime;
            string prevLabel = ctx.Params.AddressStart;

            //בונים קטע מסלול לכל יעד — זמנים, תחבורה וקווי אוטובוס
            for (int i = 0; i < ctx.BestRoute.Destinations.Count; i++)
            {
                var dest = ctx.BestRoute.Destinations[i];
                var arc  = i < ctx.BestRoute.ArcCosts.Count ? ctx.BestRoute.ArcCosts[i] : null;

                // BestDepartureTime מהטבלה מקושר לשכבת שעה — לא לעזיבה מהיעד הקודם; מקשרים שרשרת זמנים
                var nominalDeparture = arc?.BestDepartureTime ?? currentTime;
                var departureTime    = nominalDeparture < currentTime ? currentTime : nominalDeparture;
                var transitHours     = arc != null ? arc.BusTransitHours : dest.TransitTimeHours;
                var arrivalTime   = departureTime.AddHours(transitHours + dest.WalkingTimeHours);
                var leaveTime     = arrivalTime.AddHours(dest.VisitDuration);

                var busLines = await FindBusLinesAsync(dest.NearestStation);
                //יצירת קטע מסלול
                legs.Add(new TripLeg
                {
                    Order           = i + 1,
                    DesId           = dest.DestinationId,
                    DestinationName = dest.Name,
                    Region          = ctx.TripRegion,
                    ArrivalTime     = arrivalTime,
                    DepartureTime   = leaveTime,
                    StayDuration    = TimeSpan.FromHours(dest.VisitDuration),
                    Transit         = new TransitSegment
                    {
                        FromLabel         = prevLabel,
                        BoardingStation   = i == 0 ? null : ctx.BestRoute.Destinations[i - 1].NearestStation,
                        AlightingStation  = dest.NearestStation,
                        WalkingMinutes    = dest.WalkingTimeHours * 60,
                        BusLines          = busLines,
                        DepartureTime     = departureTime,
                        ArrivalTime       = arrivalTime,
                        TransitEfficiency = arc?.TransitEfficiency ?? 0
                    }
                });

                prevLabel   = dest.Name;
                currentTime = leaveTime; //הקטע הבא מתחיל אחרי עזיבה מהיעד הנוכחי
            }

            //יצירת אובייקט המכיל את כל הפרמטרים ואת המסלול האופטימלי
            var plan = new TripPlan
            {
                TripName          = ctx.TripName,
                AddressStart      = ctx.Params.AddressStart,
                TripStartTime     = ctx.Params.TripStartTime,
                TripEndTime       = ctx.Params.TripEndTime,
                TotalScore        = ctx.BestRoute.TotalScore,
                TransitEfficiency = ctx.BestRoute.TransitEfficiency,
                AvailableDestinations = available,
                Legs = legs,
                Narrative = BuildNarrative(ctx, legs, available)//סיכום מסלול לממשק משתמש
            };

            ctx.TripPlan = plan;
        }

        //שולף מה-DB קווים שעוברים בתחנה — לתצוגה ב-UI, לא לחישוב מסלול
        private async Task<List<BusLineInfo>> FindBusLinesAsync(StationInfo? station)
        {
            if (station == null) return new();

            var busStations = await _data.GetBusLinesForStationAsync(station.StationNum);

            return busStations.Select(bs => new BusLineInfo
            {
                BusId     = bs.BusId,
                BusNumber = bs.Bus != null ? bs.Bus.BusNumber.ToString() : bs.Bus?.BusCode ?? "",
                Direction = bs.Bus?.Direction ?? ""
            }).ToList();
        }

        //טקסט קריא בעברית — סיכום המסלול למשתמש (לא משפיע על החישוב)
        private static string BuildNarrative(
            OptimizationContext ctx,
            List<TripLeg> legs,
            List<AvailableDestination> available)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"הטיול מתחיל ב-{ctx.Params.TripStartTime:HH:mm} מכתובת: {ctx.Params.AddressStart}.");
            sb.AppendLine($"יעדים זמינים: {available.Count} | נבחרו במסלול: {legs.Count}");
            sb.AppendLine();

            foreach (var leg in legs)
            {
                sb.AppendLine($"── קטע {leg.Order}: {leg.Transit.FromLabel} → {leg.DestinationName} ──");

                if (leg.Transit.BusLines.Count > 0)
                {
                    var bus = leg.Transit.BusLines[0];
                    sb.AppendLine($"נסיעה: קו {bus.BusNumber} ({leg.Transit.DepartureTime:HH:mm} → {leg.Transit.ArrivalTime:HH:mm})");
                }
                else
                {
                    sb.AppendLine($"נסיעה: {leg.Transit.DepartureTime:HH:mm} → {leg.Transit.ArrivalTime:HH:mm}");
                }

                if (leg.Transit.AlightingStation != null)
                    sb.AppendLine($"תחנת ירידה: {leg.Transit.AlightingStation.StationName}");

                if (leg.Transit.WalkingMinutes > 0)
                    sb.AppendLine($"הליכה: {leg.Transit.WalkingMinutes:F0} דקות מהתחנה ליעד");

                sb.AppendLine($"הגעה: {leg.ArrivalTime:HH:mm} | שהייה: {leg.StayDuration.TotalHours:F1} שעות | עזיבה: {leg.DepartureTime:HH:mm}");
                sb.AppendLine();
            }

            sb.AppendLine($"סה\"כ: {legs.Count} יעדים | יעילות תח\"צ: {ctx.BestRoute.TransitEfficiency:P0}");
            return sb.ToString();
        }
    }
}
