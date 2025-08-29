FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY RDB/*.csproj RDB/
RUN dotnet restore RDB/RDB.csproj

COPY RDB/ RDB/
WORKDIR /src/RDB
RUN dotnet publish RDB.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Zorg voor writeable data folder
ENV DATA_DIR=/app/data
RUN mkdir -p /app/data

COPY --from=build /app .

# Let ASP.NET Core op Render free plan gebruikt dynamische poort $PORT
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "RDB.dll"]
