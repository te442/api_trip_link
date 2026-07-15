namespace API_trip_link.Settings
{
    /// <summary>ערכים קבועים</summary>
    public static class Configuration
    {
        /// <summary>המרות יחידות וערכי בסיס משותפים.</summary>
        public static class Common
        {
            public const double MinutesPerHour = 60.0;
            public const double SecondsPerHour = 3600.0;
            public const double ScoreMin = 0.0;
            public const double ScoreMax = 1.0;
            public const double InvalidTransitionScore = -1.0;
            public const int MinValidTripId = 0;
            public const int OriginDestinationId = -1;
            public const int OriginNodeIndex = 0;
            public const int MinDestinationNodeIndex = 1;
            public const int MinTripMinuteCount = 1;
            public const int MissingCoordinateValue = 0;
            public const string RequiredUrlScheme = "https://";
        }

        // <summary>משתנים קבועים לחישוב אלגוריתם ראשי חישול מדומה</summary>
        public static class Optimizer
        {
            // <summary>function weight_calculator</summary>
            public const double DefaultDynamicRequirementsWhenNoData = 0.5;
            public const double HardConstraintThreshold = 1.0;
            public const double InvalidOptimalityScore = -1.0;
            public const double TransitBonusDirectRoute = 0.1;
            public const double TransitBonusPartialMatch = 0.05;
            public const double TransitBonusNone = 0.0;
            public const double WeightSoftConstraints = 0.30;
            public const double WeightCrowd = 0.10;
            public const double WeightDynamic = 0.10;
            public const double WeightTransit = 0.50;

            public const double DefaultWalkingTimeHours = 0.5;
            public const double DefaultVisitDurationHours = 1.5;
            public const double DefaultVisitCountAvg = 100;
            public const double DefaultVisitCountStd = 30;
            public const double DefaultCrowdFactor = 0.3;
            public const double DefaultSoftConstraints = 0.8;
            public const double DefaultHardConstraints = 1.0;
            public const double DefaultMinTransitEfficiency = 0.5;
            public const double MinTransitEfficiencyFloor = 0.0;
            public const double MinTransitEfficiencyCeiling = 1.0;
            public const double DefaultMinReturnHoursFallback = 1.0;
            public const double MinReturnHoursFallbackFloor = 0.0;
            public const double MinReturnHoursFallbackCeiling = 24.0;
            // <summary>score_table</summary>
            //משתני מקביליות
            public const int DefaultScoreTableConcurrency = 6;
            public const int MinScoreTableConcurrency = 1;
            public const int MaxScoreTableConcurrency = 8;

            public const int DefaultEmptyResponseAdvanceMinutes = 1;
            public const int MinEmptyResponseAdvanceMinutes = 1;
            public const int MaxEmptyResponseAdvanceMinutes = 5;
            public const int DefaultEventNoResultAdvanceMinutes = 60;
            public const int MinEventNoResultAdvanceMinutes = 15;
            public const int MaxEventNoResultAdvanceMinutes = 180;
            public const int DefaultOriginDepartureWindowMinutes = 180;
            public const int MinOriginDepartureWindowMinutes = 30;
            public const int MaxOriginDepartureWindowMinutes = 720;

            public const int ScoreTableColumnSearchRadiusMinutes = 30;
            public const int ScoreTableNearestCellSearchRadiusMinutes = 90;
            public const int ScoreTraceScoreDecimals = 3;
            public const int ScoreTraceHoursDecimals = 2;
            public const int ValidRatioDecimalPlaces = 3;
            public const int OptimizeResultDecimalPlaces = 4;
            public const int HttpEstimateDrivingBaselinePerArc = 1;

            public const string OriginNodeLabel = "מקור";
            public const string ScoreTableDumpFileName = "score-table-dump.txt";
            public const string EmptyAddressPlaceholder = "(ריק)";
            // <summary>Simulation_anneling</summary>
            public const double SaCoolingRate = 0.003;
            public const double SaMinTemperature = 0.01;
            public const int SaMaxIterations = 1000;
            public const double SaInitialTemperature = 0.95;
            public const double SaAddDestinationProbability = 0.5;
            public const int SaMinRouteDestinations = 1;
            public const int SaMinSwapRouteSize = 2;
            public const double SaTransitEfficiencyWeight = 0.4;

            public const int GoogleTransitMinDaysAhead = 1;
            public const int GoogleTransitMaxDaysAhead = 14;
            public const int GoogleTransitFallbackDaysAhead = 7;
            public const double DefaultTripDurationHours = 9;

            public const double TransitionScoreTieEpsilon = 0.0001;
            public const int DepartureScanStepMinutes = 1;

            public const int MaxRejectionSamples = 40;

            public const int StepNumberResultMapping = 7;
            public const string StepNameResult = "RESULT";
            public const int StepNumberImageEnrichment = 8;
            public const string StepNameEnrich = "ENRICH";
            public const string TraceIdGuidFormat = "N";
            
            public static readonly IReadOnlyDictionary<int, (double Avg, double Std)> VisitStatsByDestinationId =
                new Dictionary<int, (double Avg, double Std)>
                {
                    { 1,  (350, 120) }, { 2,  (200,  40) }, { 3,  (150,  80) },
                    { 4,  (180,  50) }, { 5,  (300,  60) }, { 6,  (100,  30) },
                    { 7,  ( 80,  20) }, { 8,  (250,  70) }, { 9,  (400, 150) },
                    { 10, (500, 200) },
                };
        }

        // <summary>Google Maps / Places API ושירותי תחבורה.</summary>
        public static class Transit
        {
            public const string DefaultGoogleMapsBaseUrl = "https://maps.googleapis.com/maps/api";
            public const string DefaultGoogleMapsRegion = "il";
            public const string DefaultGoogleMapsLanguage = "he";

            public const string GoogleDirectionsApiPath = "/directions/json";
            public const string GooglePlacesAutocompleteApiPath = "/place/autocomplete/json";

            public const string GoogleMapsModeTransit = "transit";
            public const string GoogleMapsModeDriving = "driving";
            public const string GoogleTravelModeTransit = "TRANSIT";
            public const string GoogleMapsAlternativesEnabled = "true";

            public const string GoogleDirectionsStatusOk = "OK";
            public const string GoogleDirectionsStatusZeroResults = "ZERO_RESULTS";
            public const string GoogleDirectionsStatusNotFound = "NOT_FOUND";

            public const string GooglePlacesStatusOk = "OK";
            public const string GooglePlacesStatusZeroResults = "ZERO_RESULTS";

            public const string GoogleMapsApiStatusMissingKey = "MISSING_KEY";
            public const string GoogleMapsApiStatusRequestDenied = "REQUEST_DENIED";
            public const string GoogleMapsApiStatusOverQueryLimit = "OVER_QUERY_LIMIT";
            public const string GoogleMapsApiStatusInvalidRequest = "INVALID_REQUEST";

            public const int PlacesAutocompleteMinInputLength = 2;
            public const string PlacesAutocompleteCountryFilter = "country:il";
            public const string PlacesAutocompleteTypes = "geocode|establishment";

            public const string DrivingCacheKeyPrefix = "drv|";
            public const string DepartureCacheKeyPrefix = "dep|";
            public const int DirectBusMaxStops = 0;
        }

        // <summary>ASP.NET, HTTPS, CORS, Swagger.</summary>
        public static class Api
        {
            public const string CorsPolicyName = "AllowAngular";
            public const int DefaultHttpsPort = 7271;
            public const int HstsMaxAgeDays = 365;

            public const string SwaggerDocumentPath = "/swagger/v1/swagger.json";
            public const string SwaggerDocumentTitle = "Trip Planner API v1";
            public const string SwaggerRoutePrefix = "";

            public const string HttpsRequiredMessage = "HTTPS is required.";
            public const string ExternalApiHttpsRequiredMessagePrefix =
                "HTTPS is required for all external API calls. Refusing HTTP request to: ";

            public const string RouteSavedSuccessMessage = "Route saved successfully";
        }

        // <summary>תוויות וטקסטים לבניית מסלול (Step6).</summary>
        public static class Itinerary
        {
            public const string DefaultVehicleTypeLabel = "תחבורה ציבורית";
            public const string DefaultVehicleType = "BUS";
            public const string FallbackBusNumberPlaceholder = "—";
            public const string FallbackBusDirectionLabel = "לפי תחנה";
        }
    }
}
