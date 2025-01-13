# Unter https://aka.ms/customizecontainer erfahren Sie, wie Sie Ihren Debugcontainer anpassen und wie Visual Studio dieses Dockerfile verwendet, um Ihre Images für ein schnelleres Debuggen zu erstellen.

# Diese Stufe wird verwendet, wenn sie von VS im Schnellmodus ausgeführt wird (Standardeinstellung für Debugkonfiguration).
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Diese Stufe wird zum Erstellen des Dienstprojekts verwendet.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DayPlanner.Api/DayPlanner.Api.csproj", "DayPlanner.Api/"]
COPY ["DayPlanner.Abstractions/DayPlanner.Abstractions.csproj", "DayPlanner.Abstractions/"]
COPY ["DayPlanner.Authorization/DayPlanner.Authorization.csproj", "DayPlanner.Authorization/"]
COPY ["DayPlanner.FireStore/DayPlanner.FireStore.csproj", "DayPlanner.FireStore/"]
COPY ["DayPlanner.ThirdPartyImports/DayPlanner.ThirdPartyImports.csproj", "DayPlanner.ThirdPartyImports/"]
RUN dotnet restore "./DayPlanner.Api/DayPlanner.Api.csproj"
COPY . .
WORKDIR "/src/DayPlanner.Api"
RUN dotnet build "./DayPlanner.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Diese Stufe wird verwendet, um das Dienstprojekt zu veröffentlichen, das in die letzte Phase kopiert werden soll.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DayPlanner.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Diese Stufe wird in der Produktion oder bei Ausführung von VS im regulären Modus verwendet (Standard, wenn die Debugkonfiguration nicht verwendet wird).
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DayPlanner.Api.dll"]