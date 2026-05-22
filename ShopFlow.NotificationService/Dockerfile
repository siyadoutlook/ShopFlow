# Stage 1 — Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ShopFlow.NotificationService/ShopFlow.NotificationService.csproj ShopFlow.NotificationService/
COPY ShopFlow.Shared/ShopFlow.Shared.csproj ShopFlow.Shared/

RUN dotnet restore ShopFlow.NotificationService/ShopFlow.NotificationService.csproj

COPY ShopFlow.NotificationService/ ShopFlow.NotificationService/
COPY ShopFlow.Shared/ ShopFlow.Shared/

RUN dotnet publish ShopFlow.NotificationService/ShopFlow.NotificationService.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ShopFlow.NotificationService.dll"]