# Use .NET 8 SDK for build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ./Api/*.csproj ./Api/
RUN dotnet restore ./Api/Api.csproj

COPY ./Api ./Api
WORKDIR /app/Api
RUN dotnet publish -c Release -o out -p:PublishTrimmed=true

# Use runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/Api/out ./

EXPOSE 10000
ENTRYPOINT ["dotnet", "Api.dll"]
