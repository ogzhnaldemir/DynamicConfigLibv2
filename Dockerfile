FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Add NuGet source for preview packages
RUN dotnet nuget add source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet9/nuget/v3/index.json --name dotnet9-preview

# Copy csproj files and restore dependencies
COPY *.sln .
COPY DynamicConfigLib.Api/*.csproj ./DynamicConfigLib.Api/
COPY DynamicConfigLib.Core/*.csproj ./DynamicConfigLib.Core/
COPY DynamicConfigLib.Tests/*.csproj ./DynamicConfigLib.Tests/
COPY DynamicConfigLib.SampleApp/*.csproj ./DynamicConfigLib.SampleApp/
COPY DynamicConfigLib.TestConsole/*.csproj ./DynamicConfigLib.TestConsole/

# Create Directory.Build.props file to handle missing projects
RUN echo '<Project><PropertyGroup><SkipNuGet>true</SkipNuGet></PropertyGroup></Project>' > Directory.Build.props

# Restore only the API project and its dependencies
RUN dotnet restore DynamicConfigLib.Api/DynamicConfigLib.Api.csproj

# Copy the relevant projects
COPY DynamicConfigLib.Api/. ./DynamicConfigLib.Api/
COPY DynamicConfigLib.Core/. ./DynamicConfigLib.Core/

# Build and publish API project
RUN dotnet publish -c Release -o out DynamicConfigLib.Api/DynamicConfigLib.Api.csproj

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "DynamicConfigLib.Api.dll"] 