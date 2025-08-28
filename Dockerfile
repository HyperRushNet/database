FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY RDB/*.csproj RDB/
RUN dotnet restore RDB/RDB.csproj

COPY RDB/ RDB/
WORKDIR /src/RDB

RUN dotnet publish RDB.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV DATA_DIR=/data
RUN mkdir -p $DATA_DIR
VOLUME ["/data"]

COPY --from=build /app .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "RDB.dll"]
