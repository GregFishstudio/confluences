FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

RUN apt-get update \
 && apt-get install -y --allow-unauthenticated \
 libc6-dev \
 libgdiplus \
 libx11-dev \
 && rm -rf /var/lib/apt/lists/*

# Copie les binaires pré-compilés (dotnet publish exécuté localement avant docker build)
COPY publish_api/ .

ENTRYPOINT ["dotnet", "Api.dll"]
