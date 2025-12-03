# Use .NET 9.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY backend.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet build -c Release -o /app/build

# Publish the app
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Use .NET 9.0 runtime for running
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published app from build stage
COPY --from=build /app/publish .

# Expose port (Railway will set PORT env var)
# Railway automatically detects this port
EXPOSE 8080

# Set environment to Production
ENV ASPNETCORE_ENVIRONMENT=Production

# Clear any conflicting URLS - let .NET use HTTP_PORTS automatically
ENV ASPNETCORE_URLS=

# Railway needs to know the port - set it explicitly
ENV PORT=8080

# Run the app
ENTRYPOINT ["dotnet", "backend.dll"]

