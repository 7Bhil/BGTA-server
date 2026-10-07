# BRAIN - MEMOIRE DU PROJET BGTA

## Contexte Global
- Solution multi-projets : `BGTA.sln`
- Dépôts Git distincts situés dans `C:\Users\7bhil\Documents\BASIA\` :
  - `BGTA-server` (https://github.com/7Bhil/BGTA-server)
  - `BGTA-api` (https://github.com/7Bhil/BGTA-api)
  - `BGTA-shared` (https://github.com/7Bhil/BGTA-shared)
  - `BGTA-test` (https://github.com/7Bhil/BGTA-test)
- Branche d'intégration cible : `developp`

## Environnements Dual-Boot & Mémoire Persistante
- Machine en dual-boot physique :
  - **Partition Ubuntu** : environnement de développement principal où BAPI et la base ont été configurés.
  - **Partition Windows (actuelle)** : environnement secondaire avec IIS (port 8081) et SQL Server Express (localhost\SQLEXPRESS,1432).
  - La partition Ubuntu n'est pas accessible en direct depuis Windows (système de fichiers ext4 non monté sous Windows sans WSL/driver tiers).
- Règles de synchronisation :
  - La mémoire de projet doit toujours être tracée dans `BRAIN.md` à la racine pour être lisible et partagée quel que soit le système booté.
  - Les modifications de code doivent être versionnées sur Git (branche `developp`) pour éviter toute désynchronisation entre les deux OS.

- 2026-10-07 : Audit des dépôts locaux suite à des modifications poussées sur GitHub.
  - Constat : des commits récents ont été publiés sur la branche distante `origin/developp` de `BGTA-server` (jusqu'à `fcb08f2`).
  - Actions :
    - Remisage (stash) des modifications locales non validées sur `BGTA-server`.
    - Création/Bascule sur la branche locale `developp` alignée sur `origin/developp`.
- 2026-10-07 : Résolution de la disparité d'interface (Dashboard).
  - Constat : La solution Visual Studio ouverte (`BGTA/BGTA/BGTA.sln`) contenait une ancienne version statique de `Pages/Index.razor` ("Système Intégré de Gestion des Marchés Publics...").
  - Le nouveau tableau de bord multi-profils avec le **Visual Control Center** (`VisualControlCenterDashboard`, `ComptabiliteDashboard`, `TerrainDashboard`) se trouvait dans le dépôt `BGTA-server` sous Git.
  - Action : Synchronisation complète des composants (`Components/Dashboards`, `Pages/Index.razor`, etc.) vers la solution active `BGTA/BGTA/BGTA.Server` tout en préservant `appsettings.json` local.
  - Build validé avec succès (0 erreur).
- 2026-10-07 : Performance d'affichage des 5965 véhicules & Adresses Serveur.
  - Constat de performance : L'appel de `PS310493` extrait 5 965 véhicules. Sans pagination Blazor, le DOM s'alourdissait considérablement.
  - Correctifs appliqués :
    - Mise en cache mémoire (`MemoryCache`) pendant 5 minutes.
    - Ajout du téléavertisseur paginé (`MudTablePager` avec tranches 25, 50, 100, 250 lignes) dans `StylizedExceptionGrid.razor`.
  - Adresses des serveurs répertoriées :
    - Serveur local (IIS) : `http://127.0.0.1:8081` (clé API locale `a010eb21-df52-48df-8965-c33bc3371db9`).
    - Serveur distant : `http://192.168.100.160:55179` (connectivité TCP OK).
- 2026-10-07 : Reconfiguration architecturale propre (Option A).
- 2026-10-07 : Bascule sur le serveur distant (Option B).
  - `BGTA-server/appsettings.json` reconfiguré avec `http://192.168.100.160:55179` et clé `e6d8d783-e3d7-4211-852f-d70067d94070`.
  - `UserSecrets` (`secrets.json`) réaligné sur la clé `e6d8d783-e3d7-4211-852f-d70067d94070`.
  - Tests BAPI sur le serveur distant validés :
    - Authentification native `PS20020` (Compte `BASIATVS`) : HTTP 200 OK.
    - Chargement des profils `20038` : HTTP 200 OK (12 profils).
  - Build de la solution : succès (0 erreur, 0 avertissement).
- 2026-10-07 : Audit final de la branche `developp` sur les 4 dépôts.
  - `BGTA-server` : branche `developp` alignée avec `origin/developp` (commit `fcb08f2`).
  - `BGTA-api` : branche `developp` alignée avec `origin/developp` (commit `67db099`).
  - `BGTA-shared` : branche `developp` alignée avec `origin/developp` (commit `0b9e8a8`).
  - `BGTA-test` : branche `developp` alignée avec `origin/developp` (commit `0637ae5`).
- 2026-10-07 : Nouveaux commits déployés sur GitHub (`origin/developp`) :
  - `BGTA-server` : commit `f24a152` (*feat: ajout de la pagination de la grille d exceptions et harmonisation de la configuration serveur distant*).
  - `BGTA-test` : commit `b9284b7` (*fix: alignement des references de projets relatifs sur BGTA-server et BGTA-shared*).
  - Poussée Git réussie et visible en direct sur GitHub sur la branche `developp`.


