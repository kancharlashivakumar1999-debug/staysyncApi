# Step 1: Use official .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build Release binaries
COPY . ./
RUN dotnet publish -c Release -o /app/out

# Step 2: Use lightweight ASP.NET runtime image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/out .

# Expose port and start app
ENV PORT=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PgManagement.dll"]