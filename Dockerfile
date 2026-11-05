
# ================= BUILD STAGE =================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy entire solution and source code
COPY . .

# Restore API project and its dependencies
RUN dotnet restore src/PgManagement.Api/PgManagement.Api.csproj

# Publish API project
RUN dotnet publish src/PgManagement.Api/PgManagement.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ================= RUNTIME STAGE =================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

# Render application port
ENV ASPNETCORE_URLS=http://+:10000

EXPOSE 10000

# Application entry point
ENTRYPOINT ["dotnet", "PgManagement.Api.dll"]
