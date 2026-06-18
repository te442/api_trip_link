using API_trip_link.Models;

namespace API_trip_link.Services
{
    public interface IOptimizerService
    {
        Task<OptimizeResultDto> OptimizeTripAsync(OptimizeRequestDto request);
        OptimizationProgressDto? GetProgress(string traceId);
    }
}
