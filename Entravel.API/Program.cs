using Entravel.API.ExceptionHandlers;
using Entravel.Application.Orders.SubmitOrder;
using Entravel.API.Startup;
using Entravel.API.Swagger;
using Entravel.API.Validation;
using Entravel.EF.Dependencies;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Entravel Order Processing Demo",
        Version = "v1",
        Description =
            "How to test:\n" +
            "1. Use seeded CustomerId and InventoryId from the README.\n" +
            "2. Submit an order via POST /api/orders with totalAmount and discount.\n" +
            "3. The API returns orderId.\n" +
            "4. Outbox worker publishes OrderSubmitted to RabbitMQ.\n" +
            "5. Order processing worker consumes the message and applies discount in the domain model.\n" +
            "6. Call GET /api/orders/{orderId} to verify Status/amounts.\n\n" +
            "Notes:\n" +
            "- Only seeded Customers and Inventory items are valid for local Docker testing.\n" +
            "- DB-backed existence validation is intentionally out of scope for this test task."
    });

    options.SchemaFilter<SubmitOrderRequestSchemaFilter>();
});

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
