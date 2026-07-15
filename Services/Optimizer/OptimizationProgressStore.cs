using System.Collections.Concurrent;

using API_trip_link.Models;



namespace API_trip_link.Services.Optimizer

{
    //מחלקה אחראית מעקב התקדמות ועדכון המשתמש

    public interface IOptimizationProgressStore

    {

        void Ensure(string traceId);

        void UpsertStep(string traceId, OptimizationStepTraceDto step);

        void Complete(string traceId);

        void Fail(string traceId, string errorMessage);

        void InitScoreTableProgress(string traceId, int estimatedHttpRequests);

        void SetScoreTableHttpCompleted(string traceId, int completedHttpRequests);

        void AddScoreTableCell(string traceId, ScoreTableCellTraceDto cell);

        void SetScoreTableCells(string traceId, IReadOnlyList<ScoreTableCellTraceDto> cells);

        OptimizationProgressDto? Get(string traceId);

    }

    internal sealed class OptimizationProgressStore : IOptimizationProgressStore

    {
        private readonly ConcurrentDictionary<string, OptimizationProgressDto> _sessions = new();

        private readonly ConcurrentDictionary<string, List<ScoreTableCellTraceDto>> _pendingPollCells = new();

        private readonly ConcurrentDictionary<string, long> _seqCounters = new();
        public void Ensure(string traceId)

        {
            _sessions.TryAdd(traceId, new OptimizationProgressDto { TraceId = traceId });

            _pendingPollCells.TryAdd(traceId, new List<ScoreTableCellTraceDto>());

            _seqCounters.TryAdd(traceId, 0);

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

                session.IsComplete   = true;

                session.HasError     = true;

                session.ErrorMessage = errorMessage;

            }

        }



        public void InitScoreTableProgress(string traceId, int estimatedHttpRequests)

        {

            Ensure(traceId);

            if (!_sessions.TryGetValue(traceId, out var session))

                return;



            session.ScoreTableHttpRequestsEstimated = estimatedHttpRequests;

            session.ScoreTableHttpRequestsCompleted = 0;

            session.ScoreTableCellsTotal            = 0;

            session.ScoreTableCells.Clear();

            session.ScoreTableCellsBuilt            = 0;

            _seqCounters[traceId] = 0;

            ClearPending(traceId);

        }



        public void SetScoreTableHttpCompleted(string traceId, int completedHttpRequests)

        {

            Ensure(traceId);

            if (!_sessions.TryGetValue(traceId, out var session))

                return;

            session.ScoreTableHttpRequestsCompleted = completedHttpRequests;

        }
        //פעולה הכוללת מנעול כדי למנוע race condition
        public void AddScoreTableCell(string traceId, ScoreTableCellTraceDto cell)
        {
            Ensure(traceId);

            if (!_sessions.TryGetValue(traceId, out var session))

                return;
            cell.Seq = _seqCounters.AddOrUpdate(traceId, 1, (_, v) => v + 1);

            session.ScoreTableCellsBuilt++;
            var pending = _pendingPollCells.GetOrAdd(traceId, _ => new List<ScoreTableCellTraceDto>());

            lock (pending)

            {

                pending.Add(cell);

            }

        }



        public void SetScoreTableCells(string traceId, IReadOnlyList<ScoreTableCellTraceDto> cells)

        {

            Ensure(traceId);

            if (!_sessions.TryGetValue(traceId, out var session))

                return;
            ClearPending(traceId);

            session.ScoreTableCells      = cells.ToList();

            session.ScoreTableCellsBuilt = cells.Count;

            session.ScoreTableCellsTotal = cells.Count;

        }

        public OptimizationProgressDto? Get(string traceId)

        {

            if (!_sessions.TryGetValue(traceId, out var session))

                return null;



            var delta = DrainPending(traceId);

            return new OptimizationProgressDto

            {

                TraceId                         = session.TraceId,

                IsComplete                      = session.IsComplete,

                HasError                        = session.HasError,

                ErrorMessage                    = session.ErrorMessage,

                Steps                           = session.Steps.ToList(),

                ScoreTableCellsBuilt            = session.ScoreTableCellsBuilt,

                ScoreTableCellsTotal            = session.ScoreTableCellsTotal,

                ScoreTableHttpRequestsCompleted = session.ScoreTableHttpRequestsCompleted,

                ScoreTableHttpRequestsEstimated = session.ScoreTableHttpRequestsEstimated,

                ScoreTableCells                 = delta

            };

        }



        private void ClearPending(string traceId)

        {

            if (_pendingPollCells.TryGetValue(traceId, out var pending))

            {

                lock (pending)

                    pending.Clear();

            }

        }



        private List<ScoreTableCellTraceDto> DrainPending(string traceId)

        {

            if (!_pendingPollCells.TryGetValue(traceId, out var pending))

                return new List<ScoreTableCellTraceDto>();



            lock (pending)

            {

                if (pending.Count == 0)

                    return new List<ScoreTableCellTraceDto>();



                var delta = pending.ToList();

                pending.Clear();

                return delta;

            }

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

