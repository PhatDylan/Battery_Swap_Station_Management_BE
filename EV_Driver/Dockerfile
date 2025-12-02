FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EV_Driver/EV_Driver.csproj", "EV_Driver/"]
COPY ["BusinessObject/BusinessObject.csproj", "BusinessObject/"]
COPY ["Service/Service.csproj", "Service/"]
RUN dotnet restore "EV_Driver/EV_Driver.csproj"
COPY . .
WORKDIR "/src/EV_Driver"
RUN dotnet build "./EV_Driver.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EV_Driver.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EV_Driver.dll"]
