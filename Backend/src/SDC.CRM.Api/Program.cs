using SDC.CRM.Api.Authentication;
using SDC.CRM.Api.Authorization;
using SDC.CRM.Api.Identity;
using SDC.CRM.Api.Middleware;
using SDC.CRM.Api.OpenApi;
using SDC.CRM.Application;
using SDC.CRM.Infrastructure;
using SDC.CRM.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

builder.Services.AddCrmAuthentication(builder.Configuration);
builder.Services.AddCrmAuthorization();
builder.Services.AddCurrentUser();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

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

app.UseCors(AuthenticationExtensions.CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
