using ShopFlow.InventoryService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddGrpc();

var app = builder.Build();

app.MapControllers();
app.MapGrpcService<InventoryGrpcService>();

app.Run();

