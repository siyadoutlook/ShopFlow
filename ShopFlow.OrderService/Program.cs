using MassTransit;
using Microsoft.EntityFrameworkCore;
using ShopFlow.OrderService.Data;
using ShopFlow.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservability("ShopFlow.OrderService");

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlite("Data Source=orders.db"));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<OrderDbContext>(o =>
    {
        o.UseSqlite();
        o.UseBusOutbox();
    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddGrpcClient<ShopFlow.Shared.Protos.InventoryService.InventoryServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["InventoryService:GrpcUrl"]!);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.EnsureCreated();
}

app.MapControllers();

app.Run();