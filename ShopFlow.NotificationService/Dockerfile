FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ShopFlow.NotificationService/ShopFlow.NotificationService.csproj", "ShopFlow.NotificationService/"]
COPY ["ShopFlow.Shared/ShopFlow.Shared.csproj", "ShopFlow.Shared/"]
RUN dotnet restore "ShopFlow.NotificationService/ShopFlow.NotificationService.csproj"
COPY . .
WORKDIR "/src/ShopFlow.NotificationService"
RUN dotnet build "./ShopFlow.NotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ShopFlow.NotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShopFlow.NotificationService.dll"]
