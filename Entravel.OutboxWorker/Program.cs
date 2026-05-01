using Entravel.EF.Dependencies;
using Entravel.EF.Infrastructure.Persistence;
using Entravel.OutboxWorker.Outbox;
using Entravel.Rmq;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddEfInfrastructure(builder.Configuration);

builder.Services.AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection("RabbitMq"))
    .Validate(o => !string.IsNullOrWhiteSpace(o.Host), "RabbitMq:Host is required");

builder.Services.AddSingleton<IMessageRouteResolver, DefaultMessageRouteResolver>();
builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

builder.Services.AddOptions<OutboxPublisherOptions>()
    .Bind(builder.Configuration.GetSection("Outbox"));

builder.Services.AddHostedService<ApplyMigrationsHostedService>();
builder.Services.AddHostedService<OutboxPublisherBackgroundService>();

var host = builder.Build();
host.Run();
