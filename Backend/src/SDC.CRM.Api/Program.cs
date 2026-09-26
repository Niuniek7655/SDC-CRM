using SDC.CRM.Api.Authentication;
using SDC.CRM.Api.Authorization;
using SDC.CRM.Api.Identity;
using SDC.CRM.Api.Middleware;
using SDC.CRM.Api.Observability;
using SDC.CRM.Api.OpenApi;
using SDC.CRM.Application;
using SDC.CRM.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCrmObservability(builder.Configuration, builder.Environment);

builder.Services.AddControllers();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

builder.Services.AddCrmAuthentication(builder.Configuration);
builder.Services.AddCrmAuthorization();
builder.Services.AddCurrentUser();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// The database schema is managed by EF Core migrations applied explicitly
// (dotnet ef database update / migration bundle) - never implicitly at startup.

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<DomainExceptionMiddleware>();
app.UseHttpsRedirection();

app.UseCors(AuthenticationExtensions.CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
