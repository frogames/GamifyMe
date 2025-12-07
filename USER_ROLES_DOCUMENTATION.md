# Documentation des Rôles et Fonctionnalités - GamifyMe

Ce document détaille les droits d'accès et les fonctionnalités disponibles pour les différents types d'utilisateurs administratifs de la plateforme GamifyMe : **Super Admin**, **Admin**, **Editeur**, et **Gestionnaire**.

## Vue d'ensemble des Rôles

*   **Super Admin / Admin / Editeur** : Ces rôles sont considérés comme des **Administrateurs Système** ou **Maîtres du Jeu**. Ils ont un accès complet à la configuration de la plateforme, y compris la structure des équipes (Groupes) et l'économie globale (Bonus).
*   **Gestionnaire** : Ce rôle est opérationnel, conçu pour le **Staff sur le terrain**. Les gestionnaires s'occupent de la validation quotidienne (scan des joueurs, distribution des commandes boutique) et peuvent ajuster le contenu (Objectifs, Boutique), mais ne touchent pas à la structure globale (Groupes, Bonus).

## Matrice des Accès

| Fonctionnalité | Page | Super Admin / Admin / Editeur | Gestionnaire |
| :--- | :--- | :---: | :---: |
| **Tableau de Bord** | `/dashboard` | ✅ | ✅ |
| **Scanner Centralisé** | `/scanner` | ✅ | ✅ |
| **Gestion des Objectifs** | `/admin/objectives` | ✅ | ✅ |
| **Gestion de la Boutique** | `/admin/store` | ✅ | ✅ |
| **Gestion des Groupes** | `/admin/groups` | ✅ | ❌ |
| **Périodes de Bonus** | `/admin/bonus-periods` | ✅ | ❌ |

---

## 1. Guide pour les Gestionnaires

En tant que **Gestionnaire**, votre rôle principal est d'interagir avec les joueurs et de gérer le flux quotidien de l'application.

### A. Le Tableau de Bord (`/dashboard`)
C'est votre page d'accueil. Elle vous permet de :
*   **Accéder rapidement** aux autres sections (Scanner, Objectifs, Boutique).
*   **Voir le Journal d'Activité** : Une liste en temps réel de toutes les actions (validations d'objectifs, achats boutique) effectuées dans l'établissement. Utile pour vérifier qui a validé quoi.
*   **Générer le QR Code d'inscription** : Crée un QR Code que les nouveaux utilisateurs peuvent scanner pour créer un compte directement lié à votre établissement.

### B. Le Scanner (`/scanner`)
C'est votre outil principal sur le terrain. Il a deux modes :

1.  **Scanner un Profil (Joueur)** :
    *   Cliquez sur **"Scanner un joueur"** et visez le QR Code du joueur (disponible sur son profil mobile).
    *   **Infos Joueur** : Affiche son nom, solde, et niveau.
    *   **Commandes en attente** : Si le joueur a acheté un objet **physique** (ex: un goodies), il apparaîtra ici. Cliquez sur **"Valider"** une fois l'objet remis au joueur.
    *   **Objectifs en cours** : Affiche les objectifs que le joueur est en train de réaliser. Vous pouvez cliquer sur **"Valider"** pour confirmer manuellement qu'il a réussi un objectif.

2.  **Scanner pour valider un Objectif spécifique** :
    *   En bas de la page scanner, vous voyez la liste de tous les objectifs.
    *   Cliquez sur une carte objectif (ex: "Participer à l'atelier").
    *   Une fenêtre s'ouvre. Scannez le QR Code des participants à la chaîne pour leur valider cet objectif instantanément.

### C. Gestion du Contenu
Vous avez également la main pour :
*   **Objectifs** : Créer des missions temporaires ou permanentes.
*   **Boutique** : Ajouter des articles, gérer les stocks (ex: remettre du stock quand vous recevez des goodies).

---

## 2. Guide pour les Admins / Editeurs / Super Admin

Vous avez tous les pouvoirs du Gestionnaire, plus la capacité de structurer "le jeu".

### A. Gestion des Groupes (`/admin/groups`)
C'est ici que vous définissez les équipes ou "maisons" que les joueurs peuvent rejoindre.
*   **Création** : Nom, Description, Icône et Couleur.
*   **Durée d'inscription** : Définissez combien de temps (en heures) un joueur reste dans le groupe. `0` signifie illimité.
*   **QR Code Groupe** : Téléchargez un QR Code spécifique à un groupe. Lorsqu'un joueur le scanne, il rejoint automatiquement ce groupe.

### B. Périodes de Bonus (`/admin/bonus-periods`)
Utilisez cet outil pour dynamiser l'activité.
*   **Principe** : Appliquez un multiplicateur (ex: x2, x1.5) sur les gains d'XP ou de Crédits pendant une période donnée.
*   **Exemple** : "Happy Hour Vendredi" : x2 sur les Crédits de 17h à 19h.
*   **Configuration** : Choisissez la date/heure de début et de fin, le type de ressource impactée (XP ou Crédits), et le multiplicateur.

---

## 3. Détails des Fonctionnalités (Configuration Avancée)

### Gestion des Objectifs (`/admin/objectives`)
Lors de la création d'un objectif, plusieurs options définissent son comportement :

*   **Récompenses** : Définissez les gains en **XP** (progression niveau) et **Crédits** (monnaie boutique).
*   **Catégorie** :
    *   *Principal / Secondaire* : Pour l'organisation visuelle.
    *   *Événement* : Apparaît en priorité.
    *   *Onboarding* : Partie du parcours d'intégration des nouveaux.
*   **Dates & Horaires** :
    *   *Date Événement* : La date réelle de l'action.
    *   *Affichage Début/Fin* : Contrôle quand la carte est visible sur l'app des joueurs.
*   **Contraintes** :
    *   *Objet Unique* : Le joueur ne peut le valider qu'une seule fois (ex: "Inscription").
    *   *Fréquence* : Délai d'attente entre deux validations (ex: "Une fois toutes les 24h").
    *   *Prérequis* : L'objectif n'apparaît que si un autre objectif a été validé avant.
    *   *Durée de vie* : Une fois apparu, le joueur a X heures pour le faire avant qu'il disparaisse.

### Gestion de la Boutique (`/admin/store`)
La boutique propose deux types d'objets :

1.  **Objets Physiques** :
    *   Nécessitent une remise en main propre.
    *   **Stock** : Gérez la quantité disponible. Si 0, l'objet est marqué "Épuisé".
    *   L'achat crée une "Commande en attente" que le Gestionnaire doit valider via le Scanner.

2.  **Objets Numériques** :
    *   Délivrés instantanément. Pas de gestion de stock.
    *   **Presets (Préréglages)** : Sélectionnez un type (Boost XP, Thème Hiver, Son Beep...) pour configurer automatiquement l'objet.
    *   **Boosts** : Les boosts d'XP (x2, x3) s'activent depuis l'inventaire du joueur et durent un temps défini.

### Scanner & Validation
*   **Sons** : Le scanner émet un son de succès ou d'erreur. Les joueurs peuvent acheter des sons personnalisés dans la boutique !
*   **Mode "Scan Rapide"** : Pour valider un objectif à une file d'attente, ouvrez l'objectif depuis la page Scanner. Cela évite de devoir sélectionner l'objectif pour chaque personne.
