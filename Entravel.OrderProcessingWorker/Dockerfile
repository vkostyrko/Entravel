FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "Entravel.OrderProcessingWorker/Entravel.OrderProcessingWorker.csproj"
RUN dotnet publish "Entravel.OrderProcessingWorker/Entravel.OrderProcessingWorker.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Entravel.OrderProcessingWorker.dll"]
