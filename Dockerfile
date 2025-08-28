# Gebruik officiële .NET SDK image om te builden
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj en restore
COPY SimpleFileDatabase/*.csproj SimpleFileDatabase/
RUN dotnet restore SimpleFileDatabase/SimpleFileDatabase.csproj

# Copy volledige source
COPY . .
WORKDIR /src/SimpleFileDatabase
RUN dotnet publish -c Release -o /app

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "SimpleFileDatabase.dll"]
