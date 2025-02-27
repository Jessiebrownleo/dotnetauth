# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy solution and project files, then restore dependencies
COPY *.sln .
COPY DotnetAuthentication/*.csproj ./DotnetAuthentication/
RUN dotnet restore

# Copy the rest of the application and build
COPY DotnetAuthentication/. ./DotnetAuthentication/
WORKDIR /source/DotnetAuthentication
RUN dotnet publish -c Release -o /app --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
EXPOSE 5000
ENTRYPOINT ["dotnet", "DotnetAuthentication.dll"]