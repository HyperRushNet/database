# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopieer csproj en restore dependencies
COPY workspace/Database/*.csproj Database/
RUN dotnet restore Database/Database.csproj

# Kopieer de rest van de code
COPY workspace/Database/ Database/
WORKDIR /src/Database

# Build & publish
RUN dotnet publish -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV DATA_DIR=/data
RUN mkdir -p $DATA_DIR
VOLUME ["/data"]

COPY --from=build /app .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Database.dll"]
