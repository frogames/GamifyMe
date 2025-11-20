# D:\GamifyMe_V2\GamifyMe.Api\Dockerfile

# Étape 1 : Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copie des fichiers projets pour restaurer les dépendances
COPY ["GamifyMe.Api/GamifyMe.Api.csproj", "GamifyMe.Api/"]
COPY ["GamifyMe.Shared/GamifyMe.Shared.csproj", "GamifyMe.Shared/"]

# Restauration
RUN dotnet restore "GamifyMe.Api/GamifyMe.Api.csproj"

# Copie du reste du code source
COPY . .

# Compilation et Publication
WORKDIR "/src/GamifyMe.Api"
RUN dotnet publish "GamifyMe.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Étape 2 : Runtime (Image finale légère)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GamifyMe.Api.dll"]