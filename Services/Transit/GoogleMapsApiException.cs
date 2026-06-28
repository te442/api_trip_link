using API_trip_link.Settings;

namespace API_trip_link.Services.Transit
{
    //מחלקה לניהול שגיאות קריאת נתוני ם מהגוגל מפס
    public sealed class GoogleMapsApiException : Exception
    {
        public string ApiStatus { get; }

        public GoogleMapsApiException(string apiStatus, string? googleMessage)
            : base(BuildMessage(apiStatus, googleMessage))
        {
            ApiStatus = apiStatus;
        }

        private static string BuildMessage(string apiStatus, string? googleMessage)
        {
            var detail = string.IsNullOrWhiteSpace(googleMessage) ? apiStatus : $"{apiStatus}: {googleMessage}";
            return apiStatus switch
            {
                Configuration.Transit.GoogleMapsApiStatusMissingKey =>
                    "מפתח Google Maps חסר. הגדירי GoogleMaps:ApiKey ב-appsettings.Development.json.",
                Configuration.Transit.GoogleMapsApiStatusRequestDenied =>
                    $"Google Maps דחה את הבקשה ({detail}). " +
                    "ודאי שה-API הרלוונטי מופעל ב-Google Cloud, שחיוב פעיל, ושמפתח ה-API ב-appsettings.Development.json תקין וללא הגבלות IP/Referrer שחוסמות את השרת.",
                Configuration.Transit.GoogleMapsApiStatusOverQueryLimit =>
                    $"חריגה ממכסת Google Maps ({detail}). נסי שוב מאוחר יותר או בדקי את המכסה ב-Cloud Console.",
                Configuration.Transit.GoogleMapsApiStatusInvalidRequest =>
                    $"בקשה לא תקינה ל-Google Maps ({detail}). בדקי כתובת התחלה וקואורדינטות יעדים.",
                _ =>
                    $"שגיאת Google Maps API: {detail}"
            };
        }
    }
}
