# Stage 1 — Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ShopFlow.OrderService/ShopFlow.OrderService.csproj ShopFlow.OrderService/
COPY ShopFlow.Shared/ShopFlow.Shared.csproj ShopFlow.Shared/

RUN dotnet restore ShopFlow.OrderService/ShopFlow.OrderService.csproj

COPY ShopFlow.OrderService/ ShopFlow.OrderService/
COPY ShopFlow.Shared/ ShopFlow.Shared/

RUN dotnet publish ShopFlow.OrderService/ShopFlow.OrderService.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ShopFlow.OrderService.dll"]