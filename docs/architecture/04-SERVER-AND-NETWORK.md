# Serveur et réseau

## Objectif

Reconstituer le déploiement et les communications visibles dans le code sans
inventer la topologie physique absente.

## Architecture actuelle

### Matrice d’hébergement

| Composant | Hébergement observé/probable | Port interne | URL | Service | Stockage | Logs | Statut |
|---|---|---:|---|---|---|---|---|
| API | Kestrel ; Linux probable | `5000` confirmé | `https://handballwstat.ddnsfree.com/api/` côté clients | `handapi` probable | `/home/opc/api/releases`, `/home/opc/HandballWeb/site_telechargement` | journal systemd probable | Partiel |
| Web | ASP.NET Core/Blazor Server | `5100` local ; production à confirmer | URL publique à confirmer | À confirmer | fichiers statiques du projet | sortie ASP.NET Core | Partiel |
| MySQL | Serveur MySQL 8 | À confirmer | Non exposée dans les dépôts | À confirmer | volume/répertoire à confirmer | À confirmer | Non documenté |
| Artefacts | Middleware statique de l’API | même port que l’API | `/releases` et `/download` | API | répertoires locaux | logs API | Confirmé côté code |
| Integration | Poste Windows | sortant uniquement | base API configurée | application WPF | fichiers importés et logs locaux | fichiers `integration_*.log` | Confirmé |
| HandWStat | Windows/Android/iOS/macOS | sortant uniquement | base API embarquée | application cliente | préférences locales MAUI | debug local | Confirmé |

Le port `5000` est forcé par `ConfigureKestrel(ListenAnyIP)`. Les profils locaux
exposent aussi `5041/7178`, mais ces valeurs de développement ne décrivent pas
la production.

### Déploiement probable

```mermaid
flowchart TB
    Internet[Internet] -->|HTTPS 443 probable| RP[Reverse proxy / terminaison TLS<br/>À CONFIRMER]
    RP -->|HTTP vers 5000 confirmé côté API| API[Kestrel API]
    RP -->|Port interne à confirmer| WEB[Blazor Web]
    API -->|TCP, paramètres à confirmer| MYSQL[(MySQL 8)]
    API --> RELEASES[/home/opc/api/releases]
    API --> DOWNLOADS[/home/opc/HandballWeb/site_telechargement]
    ADMIN[Poste Integration] -->|HTTPS| RP
    CLIENT[HandWStat] -->|HTTPS| RP
```

Source réutilisable : [deployment-current.mmd](diagrams/deployment-current.mmd).

**PROBABLE** — Linux et systemd sont indiqués par les chemins `/home/opc`, les
commandes `systemctl` et `journalctl`, et le nom de service `handapi`.

**À CONFIRMER** — aucun fichier d’unité systemd, Nginx/Apache, Docker,
Docker Compose, IIS de production, firewall ou certificat n’est versionné.

### Communications

| Source | Destination | Protocole/format | Authentification |
|---|---|---|---|
| HandWStat | API | HTTPS, JSON | JWT utilisateur |
| Integration | API | HTTPS, JSON | JWT utilisateur Admin |
| Web | API | HTTP(S) serveur à serveur, JSON | Public pour inscription/releases |
| API | MySQL | protocole MySQL via Pomelo | chaîne de connexion secrète |
| Web/navigateur | artefact | HTTPS GET | Public |
| API | stockage local | appels fichier | droits du compte système |

L’API n’active pas CORS. Cela n’empêche pas les clients natifs ni les appels Web
côté serveur, mais toute future application navigateur appelant directement
l’API nécessitera une politique explicite.

### HTTPS, DNS et certificats

- l’URL publique des clients est en HTTPS : **CONFIRMÉ** ;
- `UseHttpsRedirection` et HSTS hors développement sont actifs dans le Web :
  **CONFIRMÉ** ;
- l’API Kestrel écoute en HTTP sur `5000` dans son point d’entrée :
  **CONFIRMÉ** ;
- terminaison TLS, renouvellement du certificat et propriétaire DNS :
  **À CONFIRMER**.

### Fichiers et permissions

L’API crée les répertoires de téléchargement et de release au démarrage. Elle
sert `.exe`, `.apk`, `.msix` et `.appinstaller` avec des types MIME dédiés. Le
compte système, les permissions, le montage de disque, la capacité et la
sauvegarde de ces répertoires sont **À CONFIRMER**.

## Supervision actuelle

Le seul mécanisme d’exploitation explicite trouvé consulte l’état et le journal
du service `handapi` après un redémarrage. Aucun health check HTTP, métrique,
trace distribuée, collecte de logs, alerte disque, sonde MySQL ou contrôle de
certificat n’est configuré dans les dépôts.

## Architecture cible recommandée

- reverse proxy documenté avec TLS automatisé et en-têtes de sécurité ;
- services API et Web distincts, comptes système non privilégiés et unités
  versionnées ;
- réseau privé entre API et MySQL, règles firewall minimales ;
- stockage d’artefacts versionné ou objet, avec contrôle d’intégrité ;
- endpoints `/health/live` et `/health/ready` ;
- logs centralisés, métriques et alertes sur disponibilité, latence, erreurs,
  disque, base et certificat ;
- inventaire DNS/ports/certificats tenu hors du code mais accessible aux
  exploitants.

## Sources

- `<HandballManagerAPI>/HandballManagerAPI/Program.cs`
- `<HandballManagerAPI>/HandballManagerAPI/Properties/launchSettings.json`
- `<HandballManagerAPI>/HandballManagerWeb/Program.cs`
- `<HandballManagerAPI>/HandballManagerWeb/Properties/launchSettings.json`
- `<HandballManagerAPI>/apply-migration.sh`
- `<HandWStat>/Configuration/ApiSettings.cs`
- `<HandballIntegration>/HandballIntegration/appsettings.json` (valeurs
  sensibles non reproduites)

