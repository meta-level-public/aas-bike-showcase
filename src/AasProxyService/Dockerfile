# Verwende das offizielle .NET Runtime Image als Base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Verwende das .NET SDK Image für den Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["AasProxyService.csproj", "."]
RUN dotnet restore "./AasProxyService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "AasProxyService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AasProxyService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AasProxyService.dll"]