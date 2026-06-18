using Microsoft.EntityFrameworkCore;
using API_trip_link.Data;
using API_trip_link.Data.Repositories;
using API_trip_link.Models;

namespace API_trip_link.Services
{
    public class ItineraryService
    {
        private readonly TripContext _context;
        private readonly IOptimizerDataRepository _data;

        public ItineraryService(TripContext context, IOptimizerDataRepository data)
        {
            _context = context;
            _data    = data;
        }
        //פעולה מחזירה את תוצאות הטיול
        public async Task<TripItineraryDto?> GetItineraryAsync(int tripId)
        {
            var trip = await _context.Trips
                .Include(t => t.DesOfTrips)
                    .ThenInclude(d => d.Destination)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip == null) return null;

            var ordered = trip.DesOfTrips?
                .OrderBy(d => d.VisitNumber)
                .Select(d => d.Destination)
                .Where(d => d != null)
                .ToList() ?? new List<Destination>();

            if (ordered.Count == 0) return null;

            var legs      = new List<TripLegDto>();
            var mapPoints = new List<MapPointDto>();
            var current   = trip.StartTime.HasValue
                ? DateTime.Today.Add(trip.StartTime.Value)
                : DateTime.Today.AddHours(8);

            string prevLabel = trip.AddressStart ?? "נקודת התחלה";

            for (int i = 0; i < ordered.Count; i++)
            {
                var dest = ordered[i];
                var station = await _context.StationToDestinations
                    .Include(s => s.Station)
                    .Where(s => s.DesId == dest.DesId)
                    .OrderBy(s => s.WalkingTime)
                    .FirstOrDefaultAsync();

                var transitMinutes = 60;
                var arrival   = current.AddMinutes(transitMinutes);
                var stayHours = dest.TimeDes?.TotalHours ?? 1.5;
                var departure = arrival.AddHours(stayHours);

                var busStations = station?.Station != null
                    ? await _data.GetBusLinesForStationAsync(station.Station.StationNum)
                    : new List<BusStation>();

                var busLines = busStations.Select(bs => new BusLineDto
                {
                    BusNumber = bs.Bus?.BusNumber.ToString() ?? "",
                    Direction = bs.Bus?.Direction ?? "",
                    FromStation = station?.Station?.StationName ?? "",
                    ToStation   = station?.Station?.StationName ?? "",
                    DepartureTime = current.ToString("HH:mm"),
                    ArrivalTime   = arrival.ToString("HH:mm")
                }).ToList();

                double lat = dest.Lat.HasValue ? (double)dest.Lat.Value
                    : station?.Station?.Lat.HasValue == true ? (double)station.Station.Lat.Value : 31.5;
                double lon = dest.Lon.HasValue ? (double)dest.Lon.Value
                    : station?.Station?.Lon.HasValue == true ? (double)station.Station.Lon.Value : 34.8;

                legs.Add(new TripLegDto
                {
                    Order           = i + 1,
                    DesId           = dest.DesId,
                    DestinationName = dest.NameDes,
                    Region          = dest.Region,
                    ImageUrl        = dest.ImageUrl,
                    Lat             = lat,
                    Lon             = lon,
                    ArrivalTime     = arrival.ToString("HH:mm"),
                    DepartureTime   = departure.ToString("HH:mm"),
                    StayDuration    = FormatDuration(TimeSpan.FromHours(stayHours)),
                    Transit         = new TransitSegmentDto
                    {
                        FromLabel        = prevLabel,
                        BoardingStation  = i == 0 ? null : station?.Station?.StationName,
                        AlightingStation = station?.Station?.StationName,
                        WalkingMinutes   = station?.WalkingTime?.TotalMinutes ?? 0,
                        DepartureTime    = current.ToString("HH:mm"),
                        ArrivalTime      = arrival.ToString("HH:mm"),
                        BusLines         = busLines
                    }
                });

                mapPoints.Add(new MapPointDto
                {
                    Order = i + 1,
                    Label = dest.NameDes,
                    Lat   = lat,
                    Lon   = lon
                });

                prevLabel = dest.NameDes;
                current   = departure;
            }

            return new TripItineraryDto
            {
                TripId           = trip.TripId,
                TripName         = trip.TripName ?? "",
                AddressStart     = trip.AddressStart ?? "",
                DestinationCount = legs.Count,
                Legs             = legs,
                MapPoints        = mapPoints
            };
        }

        public async Task EnrichWithImagesAsync(OptimizeResultDto result)
        {
            var ids = result.Legs.Select(l => l.DesId).ToList();
            var images = await _context.Destinations
                .Where(d => ids.Contains(d.DesId))
                .Select(d => new { d.DesId, d.ImageUrl, d.Lat, d.Lon })
                .ToListAsync();

            foreach (var leg in result.Legs)
            {
                var img = images.FirstOrDefault(i => i.DesId == leg.DesId);
                if (img != null)
                {
                    leg.ImageUrl = img.ImageUrl;
                    if (img.Lat.HasValue) leg.Lat = (double)img.Lat.Value;
                    if (img.Lon.HasValue) leg.Lon = (double)img.Lon.Value;
                }
            }

            result.MapPoints = result.Legs
                .Where(l => l.Lat.HasValue && l.Lon.HasValue)
                .Select(l => new MapPointDto
                {
                    Order = l.Order,
                    Label = l.DestinationName,
                    Lat   = l.Lat!.Value,
                    Lon   = l.Lon!.Value
                }).ToList();
        }

        private static string FormatDuration(TimeSpan ts)
        {
            var h = (int)ts.TotalHours;
            var m = ts.Minutes;
            return h > 0 ? $"{h}ש' {m}ד'" : $"{m} דקות";
        }
    }
}
