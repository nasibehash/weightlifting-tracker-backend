# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish WeightliftingApi.csproj -c Release -o /app

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
# SQLite file lives here; mount a disk at /data if your host supports it
ENV DB_PATH=/data/app.db
RUN mkdir -p /data
EXPOSE 8080
ENTRYPOINT ["dotnet", "WeightliftingApi.dll"]
