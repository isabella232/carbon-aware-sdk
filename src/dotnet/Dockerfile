FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /build

COPY [".", ""]
RUN dotnet build CarbonAware.WebApi/CarbonAware.WebApi.csproj

FROM build AS publish
WORKDIR /build/
RUN dotnet build "./CarbonAware.WebApi/CarbonAware.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CarbonAware.WebApi.dll"]