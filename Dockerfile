FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY RDB/*.csproj RDB/
RUN dotnet restore RDB/RDB.csproj

COPY RDB/ RDB/
WORKDIR /src/RDB
RUN dotnet publish RDB.csproj -c Release -o /app
