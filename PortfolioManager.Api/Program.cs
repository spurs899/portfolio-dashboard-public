using PortfolioManager.Core.Services;
using PortfolioManager.Core.Services.Brokerage;
using PortfolioManager.Core.Services.Market;
using PortfolioManager.Api.Hubs;
using PortfolioManager.Api.Services;
using Sentry.AspNetCore;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    // Add request size limit
    options.Filters.Add(new RequestSizeLimitAttribute(1048576)); // 1 MB
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// Market services
builder.Services.AddHttpClient<IMarketDataProvider, PolygonMarketDataProvider>();
builder.Services.AddScoped<IOfflineMarketStatusCalculator, NyseOfflineMarketStatusCalculator>();

// Brokerage clients
builder.Services.AddHttpClient<SharesiesClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        UseCookies = true,
        CookieContainer = new System.Net.CookieContainer()
    });

builder.Services.AddScoped<ISharesiesClient>(sp => sp.GetRequiredService<SharesiesClient>());
builder.Services.AddScoped<ISharesiesAuthClient>(sp => sp.GetRequiredService<SharesiesClient>());
builder.Services.AddScoped<ISharesiesDataClient>(sp => sp.GetRequiredService<SharesiesClient>());

// Brokerage clients
builder.Services.AddHttpClient<SharesiesClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        UseCookies = true,
        CookieContainer = new System.Net.CookieContainer()
    });

builder.Services.AddScoped<ISharesiesClient>(sp => sp.GetRequiredService<SharesiesClient>());
builder.Services.AddScoped<ISharesiesAuthClient>(sp => sp.GetRequiredService<SharesiesClient>());
builder.Services.AddScoped<ISharesiesDataClient>(sp => sp.GetRequiredService<SharesiesClient>());

// IBKR Session Manager (for web portal authentication)
builder.Services.AddSingleton<IIbkrSessionManager, IbkrSessionManager>();
builder.Services.AddScoped<IIbkrAutomatedAuthService, IbkrAutomatedAuthService>();
builder.Services.AddScoped<IIbkrAuthNotificationService, IbkrAuthNotificationService>();
builder.Services.AddHttpContextAccessor();

// SignalR for real-time IBKR auth updates
builder.Services.AddSignalR();

// IBKR Client with web portal URL and session management
builder.Services.AddHttpClient<IbkrClient>((sp, client) =>
{
    // Use web portal URL for Option B
    client.BaseAddress = new Uri("https://www.interactivebrokers.com.au");
})
.ConfigurePrimaryHttpMessageHandler(sp =>
{
    var sessionManager = sp.GetRequiredService<IIbkrSessionManager>();
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    
    // Get current user (if authenticated)
    var userId = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "anonymous";
    
    var handler = new HttpClientHandler
    {
        UseCookies = true,
        CookieContainer = sessionManager.GetSessionCookies(userId) ?? new System.Net.CookieContainer()
    };
    
    return handler;
});

builder.Services.AddScoped<IIbkrClient>(sp => sp.GetRequiredService<IbkrClient>());
builder.Services.AddScoped<IIbkrAuthClient>(sp => sp.GetRequiredService<IbkrClient>());
builder.Services.AddScoped<IIbkrDataClient>(sp => sp.GetRequiredService<IbkrClient>());

// Brokerage services - Register all implementations with ISP interfaces
builder.Services.AddScoped<IQrAuthenticationService, IbkrQrAuthenticationService>();

builder.Services.AddScoped<SharesiesBrokerageService>();
builder.Services.AddScoped<IBrokerageService, SharesiesBrokerageService>();
builder.Services.AddScoped<IBrokerageAuthenticationService, SharesiesBrokerageService>(sp => sp.GetRequiredService<SharesiesBrokerageService>());
builder.Services.AddScoped<IBrokeragePortfolioService, SharesiesBrokerageService>(sp => sp.GetRequiredService<SharesiesBrokerageService>());

builder.Services.AddScoped<IbkrBrokerageService>();
builder.Services.AddScoped<IBrokerageService, IbkrBrokerageService>();
builder.Services.AddScoped<IBrokerageAuthenticationService, IbkrBrokerageService>(sp => sp.GetRequiredService<IbkrBrokerageService>());
builder.Services.AddScoped<IBrokeragePortfolioService, IbkrBrokerageService>(sp => sp.GetRequiredService<IbkrBrokerageService>());

builder.Services.AddScoped<IBrokerageServiceFactory, BrokerageServiceFactory>();

// Legacy coordinator
builder.Services.AddScoped<PortfolioManager.Core.Coordinators.ISharesiesCoordinator, PortfolioManager.Core.Coordinators.SharesiesCoordinator>();

builder.WebHost.UseSentry((SentryAspNetCoreOptions  options) =>
{
    builder.Configuration.GetSection("Sentry").Bind(options);
    
    options.MinimumBreadcrumbLevel = LogLevel.Debug;
    options.MinimumEventLevel = LogLevel.Information;
    
    // Diagnostic settings
    options.Debug = builder.Environment.IsDevelopment();
    options.DiagnosticLevel = builder.Environment.IsDevelopment() ? SentryLevel.Debug : SentryLevel.Error;
    
    // Environment
    options.Environment = builder.Environment.EnvironmentName;
    options.EnableLogs = true;
    
    // Optional but useful
    options.TracesSampleRate = 0.0; // set >0 only if you want performance monitoring
});

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Global rate limit for all endpoints
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ipAddress,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    // Strict rate limit for authentication endpoints
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    // More lenient for portfolio data
    options.AddFixedWindowLimiter("portfolio", limiterOptions =>
    {
        limiterOptions.PermitLimit = 30;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });

    // Custom response when rate limit is exceeded
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            message = "Too many requests. Please try again later.",
            retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) 
                ? retryAfter.TotalSeconds 
                : 60
        }, cancellationToken: token);
    };
});

// Configure request size limits
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 1048576; // 1 MB
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 1048576; // 1 MB
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWasm",
        policy => policy
            .WithOrigins(
                "http://localhost:5262",  // Blazor WASM HTTP
                "https://localhost:7152", // Blazor WASM HTTPS
                "https://localhost:7169", // API HTTPS (itself)
                "http://localhost:5269",  // API HTTP (itself)
                "https://spurs899.github.io")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();

// Use Sentry request tracking
app.UseSentryTracing();

// Add Security Headers
app.Use(async (context, next) =>
{
    // Prevent MIME type sniffing
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    
    // Prevent clickjacking
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    
    // Enable XSS protection
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    
    // Referrer policy
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    // Content Security Policy (adjust as needed)
    context.Response.Headers.Append("Content-Security-Policy", 
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;");
    
    // HTTPS enforcement (in production)
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append("Strict-Transport-Security", 
            "max-age=31536000; includeSubDomains");
    }
    
    // Permissions policy
    context.Response.Headers.Append("Permissions-Policy", 
        "geolocation=(), microphone=(), camera=()");

    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/swagger");
            return;
        }
        await next();
    });
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseRouting();
app.UseCors("AllowBlazorWasm");
app.MapControllers();
app.MapHub<IbkrAuthHub>("/hubs/ibkrauth").RequireCors("AllowBlazorWasm");

SentrySdk.CaptureMessage("Sentry Initialised");

app.Run();