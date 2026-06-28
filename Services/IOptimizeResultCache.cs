using API_trip_link.Models;

namespace API_trip_link.Services
{
    /// <summary>
    /// שמירה זמנית בזיכרון של תוצאת אופטימיזציה לפי מזהה טיול (ללא שינוי סכמת DB).
    /// </summary>
    public interface IOptimizeResultCache
    {
        void Set(int tripId, OptimizeResultDto result);
        OptimizeResultDto? Get(int tripId);
    }
}
