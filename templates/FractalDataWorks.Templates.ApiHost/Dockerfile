FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FractalDataWorksApiHost.csproj", "."]
RUN dotnet restore "./FractalDataWorksApiHost.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./FractalDataWorksApiHost.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FractalDataWorksApiHost.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FractalDataWorksApiHost.dll"]