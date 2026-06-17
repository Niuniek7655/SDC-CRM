using SDC.CRM.Api.Middleware;
using SDC.CRM.Application;
using SDC.CRM.Infrastructure;
using SDC.CRM.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Development convenience: create the SQLite database from the model.
// TODO Technical decision: replace EnsureCreated with EF Core migrations before production.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
    dbContext.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<DomainExceptionMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
