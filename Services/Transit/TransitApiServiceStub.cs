namespace API_trip_link.Services.Transit
{
    public class MockTransitApiService : ITransitApiService
    {
        public int HttpRequestCount => 0;

        public void ResetHttpRequestCount() { }

        public Task<TransitQueryResult> GetTransitTimeAsync(
            TransitLocation from,
            TransitLocation to,
            double walkingHours,
            DateTime departureTime)
        {
            return Task.FromResult(new TransitQueryResult
            {
                BusTransitHours = 0,
                CarTransitHours = 0,
                HasDirectBus    = false,
                WalkingHours    = walkingHours
            });
        }

        public Task<TransitDepartureBatch> GetDepartureOptionsAsync(
            TransitLocation from,
            TransitLocation to,
            DateTime queryTime)
        {
            return Task.FromResult(new TransitDepartureBatch { QueryTime = queryTime });
        }

        public Task<double?> GetDrivingDurationHoursAsync(
            TransitLocation from,
            TransitLocation to,
            DateTime departureTime)
        {
            return Task.FromResult<double?>(null);
        }
    }
}
