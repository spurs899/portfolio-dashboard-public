using PortfolioManager.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(); // <-- Add this
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.AddMemoryCache();
builder.Services.AddScoped<PortfolioManager.Core.Services.IMemoryCacheWrapper, PortfolioManager.Core.Services.MemoryCacheWrapper>();

var app = builder.Build();

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

app.Run();