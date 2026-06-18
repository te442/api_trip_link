namespace API_trip_link.Services.Security
{
    /// <summary>Blocks outbound HTTP requests; only HTTPS is permitted.</summary>
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
                    $"HTTPS is required for all external API calls. Refusing HTTP request to: {uri}");
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
