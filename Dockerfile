FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/FridgeWatch.API/FridgeWatch.API.csproj"
RUN dotnet publish "src/FridgeWatch.API/FridgeWatch.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8083
ENV ASPNETCORE_URLS=http://+:8083
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8083/health || exit 1
ENTRYPOINT ["dotnet", "FridgeWatch.API.dll"]
