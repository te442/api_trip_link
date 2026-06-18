namespace API_trip_link.Services.Transit
{
    public class MockTransitApiService : ITransitApiService
    {
        public Task<TransitQueryResult> GetTransitTimeAsync(
            TransitLocation from,
            TransitLocation to,
            double baseTransitHours,
            double walkingHours,
            DateTime departureTime)
        {
            double tod    = departureTime.Hour + departureTime.Minute / 60.0;
            bool   isPeak = (tod >= 7.0 && tod < 9.0) || (tod >= 16.0 && tod < 18.0);

            double busHours = isPeak ? baseTransitHours * 1.20 : baseTransitHours;
            bool directBus  = !isPeak && (departureTime.Minute % 15 == 0);
            if (directBus) busHours *= 0.85;

            double carHours = baseTransitHours * 0.75;

            return Task.FromResult(new TransitQueryResult
            {
                BusTransitHours = busHours,
                CarTransitHours = carHours,
                HasDirectBus    = directBus,
                WalkingHours    = walkingHours
            });
        }
    }
}
