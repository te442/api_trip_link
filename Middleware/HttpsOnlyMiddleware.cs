using API_trip_link.Settings;

namespace API_trip_link.Middleware
{
    public sealed class HttpsOnlyMiddleware
    {
        //מחלקה זו בודקת את הבקשות המגיעות לשרת וחוסמת אותן אם הן לא מגיעות באמצעות פרוטוקול HTTPS
        private readonly RequestDelegate _next;
        public HttpsOnlyMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.IsHttps)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync(Configuration.Api.HttpsRequiredMessage);
                return;
            }

            await _next(context);
        }
    }
}
