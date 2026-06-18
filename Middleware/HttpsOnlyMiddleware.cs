namespace API_trip_link.Middleware
{
    /// <summary>Rejects inbound requests that did not arrive over HTTPS.</summary>
    public sealed class HttpsOnlyMiddleware
    {
        private readonly RequestDelegate _next;

        public HttpsOnlyMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.IsHttps)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("HTTPS is required.");
                return;
            }

            await _next(context);
        }
    }
}
