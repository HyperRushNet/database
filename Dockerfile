# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY SimpleFileDatabaseApi/*.csproj SimpleFileDatabaseApi/
RUN dotnet restore SimpleFileDatabaseApi/SimpleFileDatabaseApi.csproj
COPY . .
WORKDIR /src/SimpleFileDatabaseApi
RUN dotnet publish -c Release -o /app

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "SimpleFileDatabaseApi.dll"]
