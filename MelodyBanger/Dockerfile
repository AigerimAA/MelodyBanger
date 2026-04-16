FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MelodyBanger/MelodyBanger.csproj", "MelodyBanger/"]
RUN dotnet restore "MelodyBanger/MelodyBanger.csproj"

COPY . .
RUN dotnet publish "MelodyBanger/MelodyBanger.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "MelodyBanger.dll"]
