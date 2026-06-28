using API_trip_link.Models;

namespace API_trip_link.Services
{
    public interface IOptimizerService
    {
        //ממשק מעקב אחר בנית טיול
        Task<OptimizeResultDto> OptimizeTripAsync(OptimizeRequestDto request);
        void EnsureProgress(string traceId);
        OptimizationProgressDto? GetProgress(string traceId);
    }
}
