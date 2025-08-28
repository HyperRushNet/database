FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Api/*.csproj ./Api/
WORKDIR /src/Api
RUN dotnet restore

COPY Api/. ./
RUN dotnet publish -c Release -o /app/publish /p:PublishTrimmed=true

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "Api.dll"]
