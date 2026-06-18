using System.Collections.Concurrent;
using API_trip_link.Models;

namespace API_trip_link.Services.Optimizer
{
    public interface IOptimizationProgressStore
    {
        void Ensure(string traceId);
        void UpsertStep(string traceId, OptimizationStepTraceDto step);
        void Complete(string traceId);
        void Fail(string traceId, string errorMessage);
        void SetScoreTableTotals(string traceId, int totalCells);
        void AddScoreTableCell(string traceId, ScoreTableCellTraceDto cell);
        OptimizationProgressDto? Get(string traceId);
    }

    internal sealed class OptimizationProgressStore : IOptimizationProgressStore
    {
        private readonly ConcurrentDictionary<string, OptimizationProgressDto> _sessions = new();
        private readonly int _cellWindowSize;

        public OptimizationProgressStore(IConfiguration config)
        {
            _cellWindowSize = Math.Clamp(config.GetValue("Optimizer:ProgressCellWindowSize", 80), 20, 500);
        }

        public void Ensure(string traceId)
        {
            _sessions.TryAdd(traceId, new OptimizationProgressDto { TraceId = traceId });
        }

        public void UpsertStep(string traceId, OptimizationStepTraceDto step)
        {
            if (!_sessions.TryGetValue(traceId, out var session))
            {
                session = new OptimizationProgressDto { TraceId = traceId };
                _sessions[traceId] = session;
            }

            var idx = session.Steps.FindIndex(s => s.StepNumber == step.StepNumber && s.StepName == step.StepName);
            if (idx >= 0)
                session.Steps[idx] = step;
            else
                session.Steps.Add(step);
        }

        public void Complete(string traceId)
        {
            if (_sessions.TryGetValue(traceId, out var session))
                session.IsComplete = true;
        }

        public void Fail(string traceId, string errorMessage)
        {
            if (_sessions.TryGetValue(traceId, out var session))
            {
                session.IsComplete  = true;
                session.HasError    = true;
                session.ErrorMessage = errorMessage;
            }
        }

        public void SetScoreTableTotals(string traceId, int totalCells)
        {
            if (!_sessions.TryGetValue(traceId, out var session))
            {
                session = new OptimizationProgressDto { TraceId = traceId };
                _sessions[traceId] = session;
            }

            session.ScoreTableCellsTotal = totalCells;
            session.ScoreTableCells.Clear();
            session.ScoreTableCellsBuilt = 0;
        }

        public void AddScoreTableCell(string traceId, ScoreTableCellTraceDto cell)
        {
            if (!_sessions.TryGetValue(traceId, out var session))
            {
                session = new OptimizationProgressDto { TraceId = traceId };
                _sessions[traceId] = session;
            }

            session.ScoreTableCells.Add(cell);
            session.ScoreTableCellsBuilt++;

            while (session.ScoreTableCells.Count > _cellWindowSize)
                session.ScoreTableCells.RemoveAt(0);
        }

        public OptimizationProgressDto? Get(string traceId)
        {
            return _sessions.TryGetValue(traceId, out var session) ? session : null;
        }
    }

    internal static class OptimizationStepLabels
    {
        public static string GetLabel(int stepNumber, string stepName) => stepNumber switch
        {
            -1 => "התחלת תהליך האופטימיזציה",
            0  => "טעינת נתוני טיול ויעדים",
            2  => "בניית טבלת ציונים (Google Maps)",
            4  => "בניית מסלול ראשוני",
            5  => "אופטימיזציה (Simulated Annealing)",
            6  => "בניית מסלול מפורט",
            7  => "הכנת תוצאה סופית",
            8  => "העשרת תמונות ונתוני תצוגה",
            _  => stepName
        };
    }
}
