using Entravel.API.ExceptionHandlers;
using Entravel.Application.Orders.SubmitOrder;
using Entravel.API.Startup;
using Entravel.API.Validation;
using Entravel.EF.Dependencies;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<SubmitOrderCommand>());
builder.Services.AddValidatorsFromAssemblyContaining<SubmitOrderCommand>();
builder.Services.AddValidatorsFromAssemblyContaining<SubmitOrderRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.ConfigureMappings();
builder.Services.AddEfInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

await app.ApplyDatabaseMigrationsAndSeedAsync();

app.Run();
