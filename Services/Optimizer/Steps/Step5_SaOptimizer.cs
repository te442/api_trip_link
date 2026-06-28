using API_trip_link.Settings;
using API_trip_link.Models;
using API_trip_link.Services.Optimizer.Instrumentation;
using API_trip_link.Services.Transit;

namespace API_trip_link.Services.Optimizer.Steps
{
    internal class Step5_SaOptimizer : IOptimizerStep
    {
        private readonly ITransitApiService _transitApi;
        private readonly ILogger<Step5_SaOptimizer> _logger;
        private readonly Random _rng = new();
        public Step5_SaOptimizer(ITransitApiService transitApi, ILogger<Step5_SaOptimizer> logger)
        {
            _transitApi = transitApi;
            _logger     = logger;
        }

        public int StepNumber => 5;
        public string StepName => "SA_LOOP";
        //פעולה המבצעת את האלגוריתם
        //מקבלת אובייקט המכיל את כל הנתונים לצורך האלגוריתם
        public Task ExecuteAsync(OptimizationContext ctx)
        {
            //
            var arcCalculator = new ArcCostCalculator(_transitApi, ctx.Params);
            var routeBuilder  = new RouteBuilder(ctx.Params, arcCalculator);
            var tripId        = ctx.Request.TripId;
            //יצירת פתרון התחלתי 
            var currentRoute = ctx.InitialRoute;
            //והעתקתו לפתרון הטוב ביותר
            var bestRoute    = currentRoute.Copy();
            //הגדרת טמפרטורה התחלתית
            double temperature = Configuration.Optimizer.SaInitialTemperature;
            //יצירת אובייקט מסוג תוצאה של האלגוריתם
            var saResult = new SaLoopResult();
            int accepted = 0, rejected = 0;
            //לולאת loop של האלגוריתם הראשי
            //הלולאה רצה בתנאי ש הטמפרטורה המקוררת הנוכחית קטנה מהטמפרטורה המינימלית 
            //ושמספר האיטרציה הנוכחית קטן מהמספר המקסימלי של האיטרציות
            for (int i = 1; i <= Configuration.Optimizer.SaMaxIterations && temperature > Configuration.Optimizer.SaMinTemperature; i++)
            {
                var iterationSnapshot = ctx.Instrumentation?.BeginSaIteration(i, currentRoute.TotalScore);
                //יצירת פתרון שכן
                var neighbor = GetNeighborRoute(currentRoute, ctx, routeBuilder, tripId, UsageScope.SA_Iteration);
                //בדיקת האם הפתרון שכן תקין
                if (!neighbor.IsValid)
                {
                    //אם הפתרון שכן אינו תקין נדחה ונעלה את מספר הדחיות
                    rejected++;
                    if (iterationSnapshot.HasValue)
                        ctx.Instrumentation?.EndSaIteration(iterationSnapshot.Value, currentRoute.TotalScore, accepted: false, bestUpdated: false);
                    //נקרר את הטמפרטורה
                    temperature *= (1.0 - Configuration.Optimizer.SaCoolingRate);
                    continue;
                }
                //בדיקת האם הפתרון שכן תקין
                bool acceptedSolution = AcceptSolution(neighbor, currentRoute, temperature);
                bool bestUpdated = false;
                if (acceptedSolution)
                {
                    currentRoute = neighbor;
                    accepted++;
                    //חישוב עלות הפתרון השכן
                    double nC = CombinedCost(neighbor);
                    //חישוב עלות הפתרון הטוב ביותר
                    double bC = CombinedCost(bestRoute);
                    //בודקת האם השיא בניקוד של הפתרון הנוכחי גדול מהשיא בניקוד של הפתרון הטוב ביותר
                    if (nC > bC)
                    {
                        bestRoute = neighbor.Copy();
                        saResult.BestScoreProgression.Add(nC);
                        bestUpdated = true;
                    }
                }
                else

                {
                    rejected++;
                }
                if (iterationSnapshot.HasValue)
                    ctx.Instrumentation?.EndSaIteration(iterationSnapshot.Value, currentRoute.TotalScore, acceptedSolution, bestUpdated);
                //נקרר את הטמפרטורה
                temperature *= (1.0 - Configuration.Optimizer.SaCoolingRate);
            }
            //מכניס את התוצאות לאובייקט מסוג תוצאה של האלגוריתם
            saResult.TotalIterations     = accepted + rejected;
            saResult.AcceptedCount       = accepted;
            saResult.RejectedCount       = rejected;
            saResult.FinalTemperature    = temperature;

            ctx.SaResult  = saResult;
            ctx.BestRoute = bestRoute;

            var routeNames = string.Join(" → ", bestRoute.Destinations.Select(d => d.Name));
            OptimizerLog.Info(_logger, ctx,
                "SA הסתיים: איטרציות={Iter}, מקובלים={Acc}, נדחו={Rej}, מסלול סופי [{Count}]: {Route}, ציון={Score:F2}",
                saResult.TotalIterations, accepted, rejected,
                bestRoute.Destinations.Count, routeNames, bestRoute.TotalScore);

            return Task.CompletedTask;
        }
        //פונקציה יוצרת פתרון שכן
        //מקבלת פתרון נוכחי, אובייקט המכיל את כל הפרמטרים ואובייקט מסלול
        //מחזירה מסלול שכן כלומר פתרון חילופי על ידי יצירת שינויים על המסלול שהתקבל
        private OptimizerRoute GetNeighborRoute(
            OptimizerRoute current,
            OptimizationContext ctx,
            RouteBuilder routeBuilder,
            int tripId,
            UsageScope usageScope)
        {
            int currentCount = current.Destinations.Count;
            int destCount    = ctx.Destinations.Count;
            //פתרון שכן על ידי הוספת יעד
            if (currentCount < destCount && _rng.NextDouble() < Configuration.Optimizer.SaAddDestinationProbability)
            {
                var ids         = new HashSet<int>(current.Destinations.Select(d => d.DestinationId));
                var notIncluded = ctx.Destinations.Where(d => !ids.Contains(d.DestinationId)).ToList();
                if (notIncluded.Count > 0)
                {
                    var toAdd   = notIncluded[_rng.Next(notIncluded.Count)];
                    var newList = new List<OptimizerDestination>(current.Destinations) { toAdd };
                    return routeBuilder.Build(ctx.ScoreTable, ctx.Destinations, newList, _logger, tripId, usageScope);
                }
            }
            //פתרון שכן על ידי מחיקת יעד
            if (currentCount > Configuration.Optimizer.SaMinRouteDestinations)
            {
                int idx     = _rng.Next(0, currentCount);
                var newList = current.Destinations.Where((_, i) => i != idx).ToList();
                return routeBuilder.Build(ctx.ScoreTable, ctx.Destinations, newList, _logger, tripId, usageScope);
            }
            //פתרון שכן על ידי החלפת סדר ביעדים
            var swapped = new List<OptimizerDestination>(current.Destinations);
            if (swapped.Count >= Configuration.Optimizer.SaMinSwapRouteSize)
            {
                int i1 = _rng.Next(0, swapped.Count);
                int i2; do { i2 = _rng.Next(0, swapped.Count); } while (i2 == i1);
                (swapped[i1], swapped[i2]) = (swapped[i2], swapped[i1]);
            }
            return routeBuilder.Build(ctx.ScoreTable, ctx.Destinations, swapped, _logger, tripId, usageScope);
        }
        //פונקציה מחשבת את העלות הכוללת של המסלול
        private static double CombinedCost(OptimizerRoute route)
            => route.TotalScore + route.TransitEfficiency * Configuration.Optimizer.SaTransitEfficiencyWeight * route.Destinations.Count;

        //פונקציה בודקת האם לקבל את הפתרון השכן
        private bool AcceptSolution(OptimizerRoute newR, OptimizerRoute curR, double temperature)
        {
        
            double delta = CombinedCost(newR) - CombinedCost(curR);
            if (delta > 0) return true;
            //כאשר ההפרש נמוך נשתמש בערך רנדומלי כדי לקבוע האם עדיין לקבל את הפתרון
            return _rng.NextDouble() < Math.Exp(delta / temperature);
        }
    }
}
