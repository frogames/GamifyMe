
# Procédure de Déploiement normal - GamifyMe

Pour mettre à jour le code (Site ou API)
Sur votre PC : Faites vos modifications, committez et pushez sur GitHub.
bash
git push
Attendez que GitHub Actions finisse de construire les images (environ 3 minutes).
Sur le VPS : Lancez simplement votre script de déploiement (ou les commandes équivalentes).
bash
cd ~/gamifyme
./deploy.sh
(Ou manuellement : docker compose -f docker-compose.prod.yml pull && docker compose -f docker-compose.prod.yml up -d)





OU







# Procédure de Déploiement et de Dépannage - GamifyMe

Ce document décrit le processus de déploiement automatique via GitHub Actions et les procédures manuelles à suivre en cas de problème sur le VPS.

## 1. Déploiement Automatique

Le déploiement est entièrement géré par le fichier `.github/workflows/deploy.yml`.

**Déclencheur :**
- Tout `push` sur la branche `main` déclenche automatiquement le déploiement.

**Ce que fait le script :**
1.  Construit les images Docker pour l'API et le Web.
2.  Pousse les images sur le registre GitHub (ghcr.io).
3.  Copie le fichier `docker-compose.prod.yml` sur le VPS.
4.  Se connecte au VPS en SSH.
5.  Télécharge les nouvelles images (`docker compose pull`).
6.  Redémarre les conteneurs en forçant la recréation (`docker compose up -d --force-recreate --wait`).
7.  Nettoie les anciennes images inutilisées.

---

## 2. Vérification Post-Déploiement

Après un déploiement, vérifiez toujours :

1.  **L'API de version** :
    Accédez à `https://gamifyme.fun/version`.
    - Vous devez voir un JSON avec la version actuelle (ex: `{"version": "1.0.1"}`).
    - Si vous voyez une 404 ou une vieille version, le déploiement a échoué.

2.  **Le site principal** :
    Accédez à `https://gamifyme.fun`.
    - Vérifiez que la bannière de debug (si active) affiche la bonne version.

---

## 3. Dépannage Manuel (En cas d'échec)

Si le site est inaccessible (404, 502 Bad Gateway) ou si le déploiement GitHub échoue.

### Étape 1 : Se connecter au VPS
Utilisez Putty ou SSH :
```bash
ssh root@<IP_DU_VPS>
cd /root/gamifyme/
```

### Étape 2 : Vérifier l'état des conteneurs
```bash
docker ps -a
```
- **Status "Up"** : Le conteneur tourne.
- **Status "Exited"** : Le conteneur a planté. Regardez les logs : `docker logs --tail 50 gamifyme-web`.
- **Status "Created"** : Le conteneur n'a pas pu démarrer (souvent un problème de port).

### Étape 3 : Problème de Port Occupé (Zombie)
Si le conteneur Web refuse de démarrer car le port 5001 est pris ("Address already in use"), c'est qu'un processus fantôme tourne sur le serveur.

1.  **Identifier le coupable :**
    ```bash
    netstat -ltnp | grep 5001
    ```
    Notez le PID (numéro du processus, ex: `12345/dotnet`).

2.  **Tuer le processus :**
    ```bash
    kill -9 <PID>
    ```

3.  **Vérifier les services système parasites :**
    Il ne doit PAS y avoir de service `gamifyme` géré par systemd qui entre en conflit avec Docker.
    ```bash
    systemctl list-units --type=service | grep gamifyme
    ```
    Si un service apparaît, arrêtez-le et désactivez-le :
    ```bash
    systemctl stop gamifyme.service
    systemctl disable gamifyme.service
    ```

### Étape 4 : Forcer un redémarrage propre
Si tout semble bloqué, faites un nettoyage complet :

```bash
# 1. Tout arrêter
docker compose -f docker-compose.prod.yml down

# 2. Supprimer les images locales (pour forcer le retéléchargement)
docker rmi ghcr.io/frogames/gamifyme-web:latest
docker rmi ghcr.io/frogames/gamifyme-api:latest

# 3. Relancer le déploiement via GitHub Actions
# (Allez sur GitHub > Actions > Re-run jobs)

---

## 4. Configuration Nginx (Infrastructure)

Le serveur Nginx du VPS doit être configuré pour router le trafic correctement entre le Frontend et le Backend.

**Fichier de config :** `/etc/nginx/sites-available/gamifyme.fun` (ou `default`)

**Configuration requise :**

```nginx
server {
    server_name gamifyme.fun;

    # 1. Frontend (Blazor) -> Port 5001
    location / {
        proxy_pass http://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    # 2. Backend (API) -> Port 5000
    # IMPORTANT : C'est ce bloc qui manquait pour l'inscription !
    location /api/ {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Après toute modification :
```bash
sudo nginx -t
sudo systemctl reload nginx
```
```
**Note :** Ne faites pas `docker compose up` manuellement sur le VPS si vous n'avez pas exporté les variables d'environnement (`DB_CONNECTION_STRING`, etc.) au préalable. Il est plus sûr de passer par GitHub Actions.
