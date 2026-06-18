using API_trip_link.Models;

namespace API_trip_link.Services.Optimizer
{
    internal static class OptimizationTraceReporter
    {
        public static OptimizationStepTraceDto Begin(
            OptimizationContext ctx,
            IOptimizationProgressStore? store,
            int stepNumber,
            string stepName,
            string? detail = null)
        {
            var step = new OptimizationStepTraceDto
            {
                StepNumber = stepNumber,
                StepName   = stepName,
                Label      = OptimizationStepLabels.GetLabel(stepNumber, stepName),
                Status     = "Running",
                Detail     = detail,
                StartedAt  = DateTime.UtcNow
            };

            ctx.StepTrace.Add(step);
            if (!string.IsNullOrWhiteSpace(ctx.TraceId) && store != null)
                store.UpsertStep(ctx.TraceId, Clone(step));

            return step;
        }

        public static void End(
            OptimizationContext ctx,
            IOptimizationProgressStore? store,
            OptimizationStepTraceDto step,
            string? detail = null)
        {
            step.Status     = "Completed";
            step.FinishedAt = DateTime.UtcNow;
            step.DurationMs = step.StartedAt.HasValue
                ? (long)(step.FinishedAt.Value - step.StartedAt.Value).TotalMilliseconds
                : null;
            if (detail != null)
                step.Detail = detail;

            if (!string.IsNullOrWhiteSpace(ctx.TraceId) && store != null)
                store.UpsertStep(ctx.TraceId, Clone(step));
        }

        public static void Fail(
            OptimizationContext ctx,
            IOptimizationProgressStore? store,
            OptimizationStepTraceDto step,
            string error)
        {
            step.Status     = "Failed";
            step.FinishedAt = DateTime.UtcNow;
            step.DurationMs = step.StartedAt.HasValue
                ? (long)(step.FinishedAt.Value - step.StartedAt.Value).TotalMilliseconds
                : null;
            step.Detail = error;

            if (!string.IsNullOrWhiteSpace(ctx.TraceId) && store != null)
                store.UpsertStep(ctx.TraceId, Clone(step));
        }

        private static OptimizationStepTraceDto Clone(OptimizationStepTraceDto step) => new()
        {
            StepNumber = step.StepNumber,
            StepName   = step.StepName,
            Label      = step.Label,
            Status     = step.Status,
            Detail     = step.Detail,
            StartedAt  = step.StartedAt,
            FinishedAt = step.FinishedAt,
            DurationMs = step.DurationMs
        };
    }
}
