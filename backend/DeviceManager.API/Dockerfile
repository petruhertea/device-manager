FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DeviceManager.API/DeviceManager.API.csproj", "DeviceManager.API/"]
COPY ["DeviceManager.Core/DeviceManager.Core.csproj", "DeviceManager.Core/"]
COPY ["DeviceManager.Infrastructure/DeviceManager.Infrastructure.csproj", "DeviceManager.Infrastructure/"]
RUN dotnet restore "DeviceManager.API/DeviceManager.API.csproj"
COPY . .
WORKDIR "/src/DeviceManager.API"
RUN dotnet build "./DeviceManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DeviceManager.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY .env ./
ENTRYPOINT ["dotnet", "DeviceManager.API.dll"]
