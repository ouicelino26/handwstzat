# Référence de configuration

## Objectif

Lister les paramètres nécessaires sans valeur sensible. En variable
d’environnement .NET, remplacer `:` par `__`, par exemple
`JwtSettings__SecretKey`.

## Architecture actuelle

### HandWStat

| Paramètre | Composant | Obligatoire | Secret | Description | Source |
|---|---|---:|---:|---|---|
| `ApiSettings:BaseUrl` | HandWStat | Oui | Non | Base publique de l’API | `appsettings.json`, `ApiSettings`, `AppSettingsLoader` |
| `ApplicationDisplayVersion` | Build HandWStat | Oui | Non | Version sémantique affichée | `Directory.Build.props` |
| `ApplicationVersion` | Build HandWStat | Oui | Non | Numéro de build entier | `Directory.Build.props` |
| `HandballManagerCorePath` | Build HandWStat | Oui actuellement | Non | Racine locale de Core | `HandWStat.csproj` |
| `WindowsPackageType` | Build Windows | Non | Non | Type de package ; défaut `None` | `HandWStat.csproj` |

Le fichier `appsettings.json` est embarqué dans le package. Aucun mécanisme de
configuration distante ou de surcharge par environnement au runtime n’est
observé dans `AppSettingsLoader`.

### HandballManagerAPI

| Paramètre | Composant | Obligatoire | Secret | Description | Source |
|---|---|---:|---:|---|---|
| `ConnectionStrings:HandDbCon` | API | Oui hors tests | Oui | Connexion MySQL complète | `Program.cs`, `HBdbcontextFactory` |
| `Database:ServerVersion` | API | Non | Non | Version MySQL interprétée par Pomelo | `Program.cs` |
| `JwtSettings:SecretKey` | API | Oui hors tests | Oui | Clé HMAC, minimum 32 octets | `Program.cs`, `AuthController` |
| `ClientCredentials:ClientId` | API token client | Oui pour `/auth/token` | Oui | Identifiant machine | `AuthController` |
| `ClientCredentials:ClientSecret` | API token client | Oui pour `/auth/token` | Oui | Secret machine | `AuthController` |
| `Downloads:Directory` | API | Non | Non | Racine historique `/download` | `Program.cs` |
| `Releases:Directory` | API | Non | Non | Racine des artefacts `/releases` | `Program.cs` |
| `ReleaseSecurity:AllowedDownloadHosts` | API | Oui fonctionnellement | Non | Hôtes admis pour les URLs | `ReleaseSecurityOptions` |
| `ReleaseSecurity:MaximumArtifactSizeBytes` | API | Non | Non | Taille maximale d’artefact | `ReleaseSecurityOptions` |
| `APP_GIT_COMMIT_SHA` | API | Non | Non | Commit exposé dans la version système | `SystemVersionService` |
| `ASPNETCORE_ENVIRONMENT` | API | Oui par environnement | Non | Development/Testing/Production | infrastructure ASP.NET Core |
| `Logging:LogLevel:*` | API | Non | Non | Niveaux de logs | `appsettings.json` |
| `AllowedHosts` | API | Non | Non | Filtrage Host ASP.NET | `appsettings.json` |

Le port `5000` est codé dans `Program.cs`, pas configuré dans les fichiers
observés. Les profils `5041/7178` sont réservés au développement.

### HandballManagerWeb

| Paramètre | Composant | Obligatoire | Secret | Description | Source |
|---|---|---:|---:|---|---|
| `HandballApi:BaseUrl` | Web | Oui | Non | Base serveur-à-serveur de l’API | `Program.cs` |
| `Hub:PublicApiBaseUrl` | Web | Non/legacy | Non | Ancienne base publique de téléchargement | `HubOptions`, `appsettings.json` |
| `Hub:Applications` | Web | Non/legacy | Non | Ancien catalogue statique d’applications | `HubOptions`, `appsettings.json` |
| `ASPNETCORE_ENVIRONMENT` | Web | Oui par environnement | Non | Active HSTS/gestion d’erreur | `Program.cs` |
| `Logging:LogLevel:*` | Web | Non | Non | Niveaux de logs | `appsettings.json` |
| `AllowedHosts` | Web | Non | Non | Filtrage Host | `appsettings.json` |

La page d’accueil actuelle lit les releases via `ReleaseClient`; le catalogue
statique `Hub:Applications` n’est pas sa source principale observée.

### HandballIntegration

| Paramètre | Composant | Obligatoire | Secret | Description | Source |
|---|---|---:|---:|---|---|
| `ApiSettings:BaseUrl` | Integration | Oui | Non | Base de l’API | `App.xaml.cs`, `ApiSettings` |
| `ApiSettings:ClientId` | Integration | À confirmer | Oui | Champ configuré mais non utilisé par le login observé | `ApiSettings`, configuration |
| `ApiSettings:ClientSecret` | Integration | À confirmer | Oui | Champ sensible versionné ; rotation requise | `ApiSettings`, configuration |

Le fichier est copié dans le répertoire de sortie. **PROBABLE** — l’ajout
explicite du JSON après les sources par défaut de `Host.CreateDefaultBuilder`
peut lui donner priorité sur les variables d’environnement ; l’ordre doit être
testé avant de considérer `ApiSettings__*` comme surcharge sûre.

### MySQL

| Paramètre | Composant | Obligatoire | Secret | Description | Source |
|---|---|---:|---:|---|---|
| Hôte/port MySQL | Chaîne API | Oui | Partiel | Endpoint réseau privé | `ConnectionStrings:HandDbCon` |
| Nom de base | Chaîne API | Oui | Non | Schéma Handball | `ConnectionStrings:HandDbCon` |
| Utilisateur/mot de passe | Chaîne API | Oui | Oui | Identité applicative | `ConnectionStrings:HandDbCon` |
| Version de schéma | API/base | Oui | Non | Migration/version compatible | migrations, `DB_MIGRATION_HISTORY` |

Le port, l’hôte et les comptes de production sont **À CONFIRMER**. Les
identifiants historiques des scripts ne doivent pas être réutilisés.

### Releases, stockage et exploitation

| Paramètre | Composant | Obligatoire | Secret | Description | Source |
|---|---|---:|---:|---|---|
| Application/canal/version | Registre | Oui | Non | Identité d’une release | DTO et repository |
| Plateforme/architecture/package | Artefact | Oui | Non | Cible du package | DTO et repository |
| Build minimum/rollout | Update | Oui | Non | Compatibilité et déploiement progressif | `ClientUpdateService` |
| URL/taille/SHA-256 | Artefact | Oui | Non | Localisation et intégrité | registre de releases |
| Empreinte de signature | Artefact | Non | Non | Métadonnée de signature | registre de releases |
| Service API | Exploitation | Probable | Non | `handapi` | scripts shell |
| Service Web/MySQL | Exploitation | À confirmer | Non | Noms d’unités | non trouvé |
| Hôte SSH | Exploitation | À confirmer | Sensible | Accès serveur | non trouvé |
| Reverse proxy/certificat | Réseau | À confirmer | Oui pour clé privée | TLS public | non trouvé |

## Secrets : règles de stockage

**Architecture actuelle** — les fichiers API fournissent des emplacements vides
pour les secrets, mais des valeurs sensibles sont versionnées dans Integration
et des scripts MySQL. Voir le document sécurité.

**CIBLE RECOMMANDÉE** :

- secret manager par environnement ;
- variables injectées au processus, jamais copiées dans un package ;
- rotation et audit ;
- comptes applicatif, migration et administration séparés ;
- aucune valeur dans Git, logs, arguments de processus ou documentation.

## Éléments à confirmer

- valeurs non sensibles propres à chaque environnement ;
- noms des services Web/MySQL et hôtes ;
- ordre de priorité de configuration Integration ;
- politique CORS future ;
- configuration du reverse proxy, TLS, sauvegarde et supervision.

## Sources

- configurations, points d’entrée et classes Options des cinq composants ;
- `<HandWStat>/Directory.Build.props`, `HandWStat.csproj`
- `<HandballManagerAPI>/HandballManagerAPI/Services/Releases/*`
- `<HandballManagerAPI>/HandballManagerAPI/Services/System/*`

