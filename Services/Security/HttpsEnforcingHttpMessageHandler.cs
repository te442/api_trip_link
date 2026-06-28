using API_trip_link.Settings;

namespace API_trip_link.Services.Security
{
    //מחלקה לניהול בקשות HTTP בלבד
    public sealed class HttpsEnforcingHttpMessageHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var uri = request.RequestUri;
            if (uri is not null && !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"{Configuration.Api.ExternalApiHttpsRequiredMessagePrefix}{uri}");
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
