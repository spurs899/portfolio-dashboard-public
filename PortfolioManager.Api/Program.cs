using PortfolioManager.Core.Services;
using Sentry.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // <-- Add this
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWasm",
        policy => policy
            .WithOrigins("http://localhost:5262", "https://localhost:7262")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

builder.Services.AddHttpClient<ISharesiesClient, SharesiesClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        UseCookies = true,
        CookieContainer = new System.Net.CookieContainer()
    });

builder.Services.AddScoped<PortfolioManager.Core.Coordinators.ISharesiesCoordinator, PortfolioManager.Core.Coordinators.SharesiesCoordinator>();

var app = builder.Build();

// Use Sentry request tracking
app.UseSentryTracing();

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
app.UseCors("AllowBlazorWasm");
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

SentrySdk.CaptureMessage("Sentry Initialised");

app.Run();