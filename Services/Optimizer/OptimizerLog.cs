using API_trip_link.Models;

namespace API_trip_link.Services.Optimizer
{
    internal static class OptimizerLog
    {
        //מחלקת עזר לרישום לוגים
        public static void StepStart(ILogger logger, OptimizationContext ctx, int stepNumber, string stepName, string? detail = null)
        {
            logger.LogInformation(
                "[Optimizer] tripId={TripId} trace={TraceId} שלב {Step} ({Name}) — התחלה{Detail}",
                ctx.Request.TripId,
                ctx.TraceId ?? "-",
                stepNumber,
                stepName,
                detail != null ? $" | {detail}" : "");
        }

        public static void StepEnd(ILogger logger, OptimizationContext ctx, int stepNumber, string stepName, string summary, long? durationMs = null)
        {
            logger.LogInformation(
                "[Optimizer] tripId={TripId} trace={TraceId} שלב {Step} ({Name}) — הושלם ({Duration}ms): {Summary}",
                ctx.Request.TripId,
                ctx.TraceId ?? "-",
                stepNumber,
                stepName,
                durationMs ?? 0,
                summary);
        }

        public static void StepFailed(ILogger logger, OptimizationContext ctx, int stepNumber, string stepName, string error)
        {
            logger.LogError(
                "[Optimizer] tripId={TripId} trace={TraceId} שלב {Step} ({Name}) — נכשל: {Error}",
                ctx.Request.TripId,
                ctx.TraceId ?? "-",
                stepNumber,
                stepName,
                error);
        }

        public static void Info(ILogger logger, OptimizationContext ctx, string message, params object[] args)
        {
            var all = new object[2 + args.Length];
            all[0] = ctx.Request.TripId;
            all[1] = ctx.TraceId ?? "-";
            Array.Copy(args, 0, all, 2, args.Length);
            logger.LogInformation("[Optimizer] tripId={TripId} trace={TraceId} " + message, all);
        }
    }
}
