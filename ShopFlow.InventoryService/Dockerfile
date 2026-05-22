# Stage 1 — Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ShopFlow.InventoryService/ShopFlow.InventoryService.csproj ShopFlow.InventoryService/
COPY ShopFlow.Shared/ShopFlow.Shared.csproj ShopFlow.Shared/

RUN dotnet restore ShopFlow.InventoryService/ShopFlow.InventoryService.csproj

COPY ShopFlow.InventoryService/ ShopFlow.InventoryService/
COPY ShopFlow.Shared/ ShopFlow.Shared/

RUN dotnet publish ShopFlow.InventoryService/ShopFlow.InventoryService.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["dotnet", "ShopFlow.InventoryService.dll"]