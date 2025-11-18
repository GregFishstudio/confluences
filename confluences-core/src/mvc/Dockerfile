#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/mvc/mvc.csproj", "src/mvc/"]
COPY ["Confluences.Infrastructure/Confluences.Infrastructure.csproj", "Confluences.Infrastructure/"]
RUN dotnet restore "src/mvc/mvc.csproj"
COPY . .
WORKDIR "/src/src/mvc"
RUN dotnet build "mvc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "mvc.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "mvc.dll"]