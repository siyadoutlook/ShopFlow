# Stage 1 — Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first (layer caching — only re-run restore if csproj changes)
COPY ShopFlow.Gateway/ShopFlow.Gateway.csproj ShopFlow.Gateway/
COPY ShopFlow.Shared/ShopFlow.Shared.csproj ShopFlow.Shared/

RUN dotnet restore ShopFlow.Gateway/ShopFlow.Gateway.csproj

# Copy source and build
COPY ShopFlow.Gateway/ ShopFlow.Gateway/
COPY ShopFlow.Shared/ ShopFlow.Shared/

RUN dotnet publish ShopFlow.Gateway/ShopFlow.Gateway.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2 — Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ShopFlow.Gateway.dll"]