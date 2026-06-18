using API_trip_link.Models;
using API_trip_link.Services.Transit;

namespace API_trip_link.Services.Optimizer.Steps
{
    internal class Step5_SaOptimizer : IOptimizerStep
    {
        private readonly ITransitApiService _transitApi;
        private readonly Random _rng = new();
        //הגדרת הקבועים של קירור האלגוריתם
        private const double CoolingRate    = 0.003;
        private const double MinTemperature = 0.01;
        private const int MaxIterations  = 1000;
        private const double    temp  = 0.95;
        public Step5_SaOptimizer(ITransitApiService transitApi)
        {
            _transitApi = transitApi;
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
            //יצירת פתרון התחלתי 
            var currentRoute = ctx.InitialRoute;
            //והעתקתו לפתרון הטוב ביותר
            var bestRoute    = currentRoute.Copy();
            //הגדרת טמפרטורה התחלתית
            double temperature = temp ;
            //יצירת אובייקט מסוג תוצאה של האלגוריתם
            var saResult = new SaLoopResult();
            int accepted = 0, rejected = 0;
            //לולאת loop של האלגוריתם הראשי
            //הלולאה רצה בתנאי ש הטמפרטורה המקוררת הנוכחית קטנה מהטמפרטורה המינימלית 
            //ושמספר האיטרציה הנוכחית קטן מהמספר המקסימלי של האיטרציות
            for (int i = 1; i <= MaxIterations && temperature > MinTemperature; i++)
            {
                //יצירת פתרון שכן
                var neighbor = GetNeighborRoute(currentRoute, ctx, routeBuilder);
                //בדיקת האם הפתרון שכן תקין
                if (!neighbor.IsValid)
                {
                    //אם הפתרון שכן אינו תקין נדחה ונעלה את מספר הדחיות
                    rejected++;
                    //נקרר את הטמפרטורה
                    temperature *= (1.0 - CoolingRate);
                    continue;
                }
                //בדיקת האם הפתרון שכן תקין
                if (AcceptSolution(neighbor, currentRoute, temperature))
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
                    }
                }
                else

                {
                    rejected++;
                }
                //נקרר את הטמפרטורה
                temperature *= (1.0 - CoolingRate);
            }
            //מכניס את התוצאות לאובייקט מסוג תוצאה של האלגוריתם
            saResult.TotalIterations     = accepted + rejected;
            saResult.AcceptedCount       = accepted;
            saResult.RejectedCount       = rejected;
            saResult.FinalTemperature    = temperature;

            ctx.SaResult  = saResult;
            ctx.BestRoute = bestRoute;
            //מחזיר את התוצאות לפעולה הראשית
            return Task.CompletedTask;
        }
        //פונקציה יוצרת פתרון שכן
        //מקבלת פתרון נוכחי, אובייקט המכיל את כל הפרמטרים ואובייקט מסלול
        //מחזירה מסלול שכן כלומר פתרון חילופי על ידי יצירת שינויים על המסלול שהתקבל
        private OptimizerRoute GetNeighborRoute(
            OptimizerRoute current,
            OptimizationContext ctx,
            RouteBuilder routeBuilder)
        {
            int currentCount = current.Destinations.Count;
            int destCount    = ctx.Destinations.Count;
            //פתרון שכן על ידי הוספת יעד
            if (currentCount < destCount && _rng.NextDouble() < 0.5)
            {
                var ids         = new HashSet<int>(current.Destinations.Select(d => d.DestinationId));
                var notIncluded = ctx.Destinations.Where(d => !ids.Contains(d.DestinationId)).ToList();
                if (notIncluded.Count > 0)
                {
                    var toAdd   = notIncluded[_rng.Next(notIncluded.Count)];
                    var newList = new List<OptimizerDestination>(current.Destinations) { toAdd };
                    return routeBuilder.Build(ctx.ScoreTable, ctx.Destinations, newList);
                }
            }
            //פתרון שכן על ידי מחיקת יעד
            if (currentCount > 1)
            {
                int idx     = _rng.Next(0, currentCount);
                var newList = current.Destinations.Where((_, i) => i != idx).ToList();
                return routeBuilder.Build(ctx.ScoreTable, ctx.Destinations, newList);
            }
            //פתרון שכן על ידי החלפת סדר ביעדים
            var swapped = new List<OptimizerDestination>(current.Destinations);
            if (swapped.Count >= 2)
            {
                int i1 = _rng.Next(0, swapped.Count);
                int i2; do { i2 = _rng.Next(0, swapped.Count); } while (i2 == i1);
                (swapped[i1], swapped[i2]) = (swapped[i2], swapped[i1]);
            }
            return routeBuilder.Build(ctx.ScoreTable, ctx.Destinations, swapped);
        }
        //פונקציה מחשבת את העלות הכוללת של המסלול
        private static double CombinedCost(OptimizerRoute route)
            => route.TotalScore + route.TransitEfficiency * 0.4 * route.Destinations.Count;

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
