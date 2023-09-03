using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString: builder.Configuration.GetConnectionString("PostgreSQL")!,
               name: "PostgreSQL",
               failureStatus: HealthStatus.Degraded)
    .AddRedis(redisConnectionString: builder.Configuration.GetConnectionString("Redis")!,
              name: "Redis",
              failureStatus: HealthStatus.Degraded)
    .AddRabbitMQ(rabbitConnectionString: builder.Configuration.GetConnectionString("RabbitMQ")!,
                 name: "RabbitMQ",
                 failureStatus: HealthStatus.Degraded);
builder.Services.AddHealthChecksUI(setupSettings: setup =>
{
    setup.AddHealthCheckEndpoint("Sample-Health-Check-API", "/_health");
    setup.MaximumHistoryEntriesPerEndpoint(500);
    setup.SetEvaluationTimeInSeconds(5);
}).AddInMemoryStorage();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapGet("/", () => Results.Redirect("healthchecks-ui"));
app.MapHealthChecks("_health");
app.UseHealthChecks("/_health", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecksUI();

app.Run();
