using ShopFlow.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddObservability("ShopFlow.Gateway");

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapReverseProxy();

app.Run();