# Base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Install necessary dependencies for Syncfusion PDF viewer
RUN ln -s /lib/x86_64-linux-gnu/libdl-2.24.so /lib/x86_64-linux-gnu/libdl.so
RUN apt-get update \
 && apt-get install -y --allow-unauthenticated \
 libc6-dev \
 libgdiplus \
 libx11-dev \
 && rm -rf /var/lib/apt/lists/*

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src
COPY ["src/Api/Api.csproj", "src/Api/"]
COPY ["Confluences.Persistence/Confluences.Persistence.csproj", "Confluences.Persistence/"]
COPY ["Confluences.Application/Confluences.Application.csproj", "Confluences.Application/"]
COPY ["Confluences.Domain/Confluences.Domain.csproj", "Confluences.Domain/"]
RUN dotnet restore "src/Api/Api.csproj"
COPY . .
WORKDIR "/src/src/Api"
RUN dotnet build "Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final

# Copy published app
WORKDIR /app
COPY --from=publish /app/publish .

# RUN /app/bin/Debug/net6.0/datadog/createLogPath.sh

# Start the app
ENTRYPOINT ["dotnet", "Api.dll"]
