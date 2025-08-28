FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY Api/Api.csproj Api/
RUN dotnet restore Api/Api.csproj

COPY Api/ Api/
WORKDIR /source/Api
RUN dotnet publish Api.csproj -c Release -o /app --self-contained false --no-restore /p:PublishTrimmed=true

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 10000
ENTRYPOINT ["dotnet", "Api.dll"]
