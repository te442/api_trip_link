using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using API_trip_link.Settings;
using API_trip_link.Data;
using API_trip_link.Data.Repositories;
using API_trip_link.Middleware;
using API_trip_link.Services;
using API_trip_link.Services.Optimizer;
using API_trip_link.Services.Optimizer.Steps;
using API_trip_link.Services.Security;
using API_trip_link.Services.Transit;

var builder = WebApplication.CreateBuilder(args);

ValidateHttpsConfiguration(builder.Configuration);

//התחברות ל SQL Server
builder.Services.AddDbContext<TripContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//הזרקת תלויות לשירותים במערכת
builder.Services.AddScoped<TripService>();
builder.Services.AddScoped<DestinationService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ItineraryService>();
builder.Services.AddScoped<LookupService>();
builder.Services.AddScoped<ILookupRepository, LookupRepository>();
builder.Services.AddScoped<IOptimizerDataRepository, OptimizerDataRepository>();
builder.Services.AddScoped<IOptimizerService, OptimizerServiceImpl>();


builder.Services.AddTransient<HttpsEnforcingHttpMessageHandler>();
builder.Services.AddHttpClient<ITransitApiService, GoogleMapsTransitApiService>()
    .AddHttpMessageHandler<HttpsEnforcingHttpMessageHandler>();
builder.Services.AddHttpClient<IPlacesAutocompleteService, GooglePlacesAutocompleteService>()
    .AddHttpMessageHandler<HttpsEnforcingHttpMessageHandler>();

builder.Services.AddSingleton<IOptimizationProgressStore, OptimizationProgressStore>();
builder.Services.AddSingleton<IOptimizeResultCache, OptimizeResultCache>();


builder.Services.AddScoped<OptimizerPipeline>();
builder.Services.AddScoped<IOptimizerStep, Step0_InputLoader>();
builder.Services.AddScoped<IOptimizerStep, Step2_ScoreTableBuilder>();
builder.Services.AddScoped<IOptimizerStep, Step4_InitialRouteBuilder>();
builder.Services.AddScoped<IOptimizerStep, Step5_SaOptimizer>();
builder.Services.AddScoped<IOptimizerStep, Step6_TripItineraryBuilder>();

//הגדרות אבטחה למשתמשים מורשים
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience           = true,
            ValidateLifetime           = true,
            ValidateIssuerSigningKey   = true,
            ValidIssuer                = builder.Configuration["Jwt:Issuer"],
            ValidAudience              = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey           = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

//הגדרות ה cors אילו אתרים יוכלו לגשת למערכת
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });


var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(Configuration.Api.CorsPolicyName, policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
// הגדרת פרוטוקלי תקשורת
var httpsPort = builder.Configuration.GetValue<int?>("Https:Port") ?? Configuration.Api.DefaultHttpsPort;
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
    options.HttpsPort          = httpsPort;
});

builder.Services.AddHsts(options =>
{
    options.Preload           = true;
    options.IncludeSubDomains = true;
    options.MaxAge            = TimeSpan.FromDays(Configuration.Api.HstsMaxAgeDays);
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//בניית האפליקציה
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint(Configuration.Api.SwaggerDocumentPath, Configuration.Api.SwaggerDocumentTitle);
        c.RoutePrefix = Configuration.Api.SwaggerRoutePrefix;
    });
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseMiddleware<HttpsOnlyMiddleware>();
app.UseCors(Configuration.Api.CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static void ValidateHttpsConfiguration(IConfiguration configuration)
{
    var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    foreach (var origin in origins)
    {
        if (!origin.StartsWith(Configuration.Common.RequiredUrlScheme, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"CORS origin must use HTTPS: '{origin}'. Update Cors:AllowedOrigins in appsettings.");
        }
    }

    var googleBaseUrl = configuration["GoogleMaps:BaseUrl"];
    if (!string.IsNullOrWhiteSpace(googleBaseUrl) &&
        !googleBaseUrl.StartsWith(Configuration.Common.RequiredUrlScheme, StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "GoogleMaps:BaseUrl must use HTTPS. Update appsettings.");
    }
}
