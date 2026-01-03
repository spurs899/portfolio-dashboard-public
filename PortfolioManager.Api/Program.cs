using PortfolioManager.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<ISharesiesClient, SharesiesClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        UseCookies = true,
        CookieContainer = new System.Net.CookieContainer()
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapPost("/sharesies/login", async (ISharesiesClient sharesiesClient, string email, string password) =>
{
    var success = await sharesiesClient.LoginAsync(email, password);
    return success is { Authenticated: true } ? Results.Ok() : Results.Unauthorized();
});

app.MapGet("/sharesies/profile", async (ISharesiesClient sharesiesClient) =>
{
    var profile = await sharesiesClient.GetProfileAsync();
    return profile != null ? Results.Ok(profile) : Results.NotFound();
});

app.MapGet("/sharesies/portfolio", async (ISharesiesClient sharesiesClient) =>
{
    var portfolio = await sharesiesClient.GetPortfolioAsync();
    return portfolio != null ? Results.Ok(portfolio) : Results.NotFound();
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
