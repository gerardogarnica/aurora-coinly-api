# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/Aurora.Coinly.Api/Aurora.Coinly.Api.csproj", "src/Aurora.Coinly.Api/"]
COPY ["src/Aurora.Coinly.Infrastructure/Aurora.Coinly.Infrastructure.csproj", "src/Aurora.Coinly.Infrastructure/"]
COPY ["src/Aurora.Coinly.Application/Aurora.Coinly.Application.csproj", "src/Aurora.Coinly.Application/"]
COPY ["src/Aurora.Coinly.Domain/Aurora.Coinly.Domain.csproj", "src/Aurora.Coinly.Domain/"]
RUN dotnet restore "./src/Aurora.Coinly.Api/Aurora.Coinly.Api.csproj"
COPY . .
WORKDIR "/src/src/Aurora.Coinly.Api"
RUN dotnet build "./Aurora.Coinly.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Aurora.Coinly.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Aurora.Coinly.Api.dll"]