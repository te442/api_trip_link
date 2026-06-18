using System.Text.Json;

namespace API_trip_link.Services.Optimizer
{
    internal static class AgentDebugLog
    {
        private const string LogPath = @"c:\Users\User\Desktop\פרויקט טיול\API_trip_link\debug-459778.log";

        public static void Write(string location, string message, object data, string hypothesisId, string runId = "pre-fix")
        {
            // #region agent log
            try
            {
                var line = JsonSerializer.Serialize(new
                {
                    sessionId = "459778",
                    runId,
                    hypothesisId,
                    location,
                    message,
                    data,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                });
                File.AppendAllText(LogPath, line + "\n");
            }
            catch { }
            // #endregion
        }
    }
}
