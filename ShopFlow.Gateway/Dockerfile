FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ShopFlow.Gateway/ShopFlow.Gateway.csproj", "ShopFlow.Gateway/"]
COPY ["ShopFlow.Shared/ShopFlow.Shared.csproj", "ShopFlow.Shared/"]
RUN dotnet restore "ShopFlow.Gateway/ShopFlow.Gateway.csproj"
COPY . .
WORKDIR "/src/ShopFlow.Gateway"
RUN dotnet build "./ShopFlow.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ShopFlow.Gateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShopFlow.Gateway.dll"]
