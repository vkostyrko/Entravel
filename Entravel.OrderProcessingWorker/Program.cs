using Entravel.Application.Orders.OrderSubmitted;
using Entravel.EF.Dependencies;
using Entravel.EF.Infrastructure.Persistence;
using Entravel.OrderProcessingWorker.Consumers;
using Entravel.Rmq;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddEfInfrastructure(builder.Configuration);

builder.Services.AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection("RabbitMq"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Host), "RabbitMq:Host is required");

builder.Services.AddScoped<IRabbitMqMessageDispatcher, OrderProcessingRabbitMqDispatcher>();
builder.Services.AddScoped<OrderSubmittedMessageHandler>();

builder.Services.AddHostedService<ApplyMigrationsHostedService>();
builder.Services.AddRabbitMqConsumers();

var host = builder.Build();
host.Run();
