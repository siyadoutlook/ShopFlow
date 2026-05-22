using MassTransit;
using ShopFlow.NotificationService.Consumer;
using ShopFlow.Shared.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.Configure(options =>
{
    options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId;
});

builder.Services.AddObservability("ShopFlow.NotificationService");

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderConfirmedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMq:Host"] ?? "localhost";
        
        cfg.Host(host, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
