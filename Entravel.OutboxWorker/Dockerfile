FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "Entravel.OutboxWorker/Entravel.OutboxWorker.csproj"
RUN dotnet publish "Entravel.OutboxWorker/Entravel.OutboxWorker.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Entravel.OutboxWorker.dll"]

