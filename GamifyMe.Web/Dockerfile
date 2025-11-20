# D:\GamifyMe_V2\GamifyMe.Web\Dockerfile

# Étape 1 : Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copie des références (Web dépend de Client, UI.Shared, et Shared)
COPY ["GamifyMe.Web/GamifyMe.Web.csproj", "GamifyMe.Web/"]
COPY ["GamifyMe.Web.Client/GamifyMe.Web.Client.csproj", "GamifyMe.Web.Client/"]
COPY ["GamifyMe.UI.Shared/GamifyMe.UI.Shared.csproj", "GamifyMe.UI.Shared/"]
COPY ["GamifyMe.Shared/GamifyMe.Shared.csproj", "GamifyMe.Shared/"]

# Restauration
RUN dotnet restore "GamifyMe.Web/GamifyMe.Web.csproj"

# Copie du reste du code
COPY . .

# Compilation et Publication
WORKDIR "/src/GamifyMe.Web"
RUN dotnet publish "GamifyMe.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Étape 2 : Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GamifyMe.Web.dll"]