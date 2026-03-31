var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddGrpcClient<ShopFlow.Shared.Protos.InventoryService.InventoryServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["InventoryService:GrpcUrl"]!);
});

var app = builder.Build();

app.MapControllers();

app.Run();