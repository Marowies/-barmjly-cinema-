# Use the official .NET 9 SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy the entire solution and projects
COPY *.sln .
COPY Cinema.API/*.csproj ./Cinema.API/
COPY Cinema.Core/*.csproj ./Cinema.Core/
COPY Cinema.Infrastructure/*.csproj ./Cinema.Infrastructure/
COPY Cinema.Tests/*.csproj ./Cinema.Tests/

# Restore dependencies
RUN dotnet restore

# Copy everything else and build the app
COPY . .
WORKDIR /app/Cinema.API
RUN dotnet publish -c Release -o /out

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /out .

# Expose the port (Render uses PORT environment variable, usually 80 or 8080)
EXPOSE 80
EXPOSE 443

# Start the application
ENTRYPOINT ["dotnet", "Cinema.API.dll"]
