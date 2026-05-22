using MassTransit;
using ShopFlow.InventoryService.Consumer;
using ShopFlow.InventoryService.Services;
using ShopFlow.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservability("ShopFlow.InventoryService");

builder.Services.AddControllers();
builder.Services.AddGrpc();

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

var app = builder.Build();

app.MapControllers();
app.MapGrpcService<InventoryGrpcService>();

app.Run();

