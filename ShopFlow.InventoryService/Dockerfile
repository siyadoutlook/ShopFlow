FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ShopFlow.InventoryService/ShopFlow.InventoryService.csproj", "ShopFlow.InventoryService/"]
COPY ["ShopFlow.Shared/ShopFlow.Shared.csproj", "ShopFlow.Shared/"]
RUN dotnet restore "ShopFlow.InventoryService/ShopFlow.InventoryService.csproj"
COPY . .
WORKDIR "/src/ShopFlow.InventoryService"
RUN dotnet build "./ShopFlow.InventoryService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ShopFlow.InventoryService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShopFlow.InventoryService.dll"]
