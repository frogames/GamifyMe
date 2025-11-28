#!/bin/bash

# Définition des couleurs
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}=== DÉPLOIEMENT GAMIFYME ===${NC}"

# 1. Vérification du fichier .env
if [ ! -f .env ]; then
    echo -e "${RED}Le fichier .env n'existe pas. Création automatique...${NC}"
    cat <<EOT >> .env
ConnectionStrings__DefaultConnection="Host=postgresql-15efa419-o6fcb0608.database.cloud.ovh.net;Port=20184;Database=defaultdb;Username=gamifyme_admin;Password=xYl8K1TGr9a2MP0XynEA;Ssl Mode=Require;Trust Server Certificate=true"
ASPNETCORE_ENVIRONMENT=Production
Jwt__Key="Ceci est une phrase secrète très très longue pour satisfaire l'algorithme HMAC SHA512 qui est gourmand en bits 123456789"
SMTP_HOST="ssl0.ovh.net"
SMTP_PORT="587"
SMTP_SENDER_EMAIL="contact@gamifyme.fun"
SMTP_PASSWORD="<VOTRE_MOT_DE_PASSE_SMTP>"
EOT
    echo -e "${GREEN}Fichier .env créé avec succès !${NC}"
    echo -e "${RED}ATTENTION : Modifiez le mot de passe SMTP dans le fichier .env si nécessaire.${NC}"
fi

# 2. Login Docker (si nécessaire)
echo -e "${GREEN}Vérification de la connexion Docker...${NC}"
# On tente un pull pour voir si on est connecté
docker compose -f docker-compose.prod.yml pull gamifyme-api > /dev/null 2>&1
if [ $? -ne 0 ]; then
    echo -e "${RED}Vous n'êtes pas connecté au registre GitHub.${NC}"
    echo "Veuillez entrer votre Token GitHub (ghp_...) :"
    read -s TOKEN
    echo "Veuillez entrer votre nom d'utilisateur GitHub :"
    read USERNAME
    echo "$TOKEN" | docker login ghcr.io -u "$USERNAME" --password-stdin
fi

# 3. Mise à jour et redémarrage
echo -e "${GREEN}Téléchargement des nouvelles images...${NC}"
docker compose -f docker-compose.prod.yml pull

echo -e "${GREEN}Redémarrage des services...${NC}"
docker compose -f docker-compose.prod.yml up -d --force-recreate

echo -e "${GREEN}=== DÉPLOIEMENT TERMINÉ AVEC SUCCÈS ===${NC}"
