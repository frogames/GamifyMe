# Étape 1: Construction (Build)
# On utilise l'image SDK .NET 8 pour compiler
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier tous les fichiers .csproj et le .sln pour restaurer les packages
# (Maintenant que le .sln existe, cette étape va réussir)
COPY GamifyMeV2.sln .
COPY GamifyMe.Api/GamifyMe.Api.csproj GamifyMe.Api/
COPY GamifyMe.Shared/GamifyMe.Shared.csproj GamifyMe.Shared/
COPY GamifyMe.UI.Shared/GamifyMe.UI.Shared.csproj GamifyMe.UI.Shared/
COPY GamifyMe.Web/GamifyMe.Web.csproj GamifyMe.Web/
COPY GamifyMe.Web.Client/GamifyMe.Web.Client.csproj GamifyMe.Web.Client/
COPY GamifyMe.App/GamifyMe.App.csproj GamifyMe.App/

# Restaurer les dépendances
RUN dotnet restore GamifyMeV2.sln

# Copier tout le reste du code source
COPY . .

# Publier l'API en mode Release
RUN dotnet publish GamifyMe.Api/GamifyMe.Api.csproj -c Release -o /app/publish

# Étape 2: Production (Runtime)
# On utilise l'image ASP.NET légère pour l'exécution
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# L'API écoutera sur le port 8080 à l'intérieur du conteneur
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Commande de démarrage
ENTRYPOINT ["dotnet", "GamifyMe.Api.dll"]