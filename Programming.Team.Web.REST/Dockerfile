# Base runtime image for ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src
COPY ["Programming.Team.Web.REST/Programming.Team.Web.REST.csproj", "Programming.Team.Web.REST/"]
RUN dotnet restore "./Programming.Team.Web.REST/Programming.Team.Web.REST.csproj"
COPY . . 
WORKDIR "/src/Programming.Team.Web.REST"
RUN dotnet build "./Programming.Team.Web.REST.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "./Programming.Team.Web.REST.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image with LaTeX support
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install LaTeX dependencies (full installation)
RUN apt-get update && apt-get install -y \
    texlive-latex-base \
    texlive-latex-extra \
    texlive-fonts-recommended \
    lmodern


# Copy published app
COPY --from=publish /app/publish .

# Set entry point
ENTRYPOINT ["dotnet", "Programming.Team.Web.REST.dll"]
