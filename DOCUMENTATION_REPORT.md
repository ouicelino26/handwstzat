# Rapport final de documentation

## Résumé exécutif

L’écosystème Handball est une architecture distribuée simple centrée sur une API
ASP.NET Core 8 et MySQL 8. HandWStat consulte et analyse les données,
HandballIntegration les importe et les administre, HandballManagerWeb inscrit
les utilisateurs et distribue les releases, et HandballManagerCore fournit les
contrats partagés.

Les fonctions métier, analytiques, de sécurité, de persistance et de release ont
été documentées à partir des fichiers réels. L’infrastructure de production
n’est que partiellement versionnée. Les principaux risques sont deux expositions
de secrets, un mélange Oracle/MySQL dans les migrations, l’absence de chaîne
CI/CD et l’absence de sauvegarde/supervision démontrée.

## Dépôts détectés

| Dépôt | Rôle | Statut documentaire |
|---|---|---|
| `HandWStat` / remote `handwstzat` | Client analytique et dépôt documentaire | Actif, documenté |
| `HandballManagerAPI` | API, Web, tests, migrations | Actif, documenté |
| `HandballIntegration` | Import WPF | Actif, documenté ; modifications utilisateur préservées |
| `HandballManagerCore` | Contrats partagés | Actif, documenté ; modification utilisateur préservée |
| `HandballManagerMaui` | Ancien client MAUI | Historique, mentionné seulement |
| `HANDBALLSTAT` | Ancien client WPF/SQLite | Historique, mentionné seulement |

Aucun dépôt de serveur MCP Handball n’a été trouvé.

## Applications documentées

- HandWStat : pages, navigation, clients API, authentification, graphiques,
  versioning, contrôle de mise à jour et cibles MAUI ;
- HandballManagerAPI : contrôleurs par domaine, services analytics/import,
  repository de releases, EF/MySQL, JWT, Swagger et fichiers statiques ;
- HandballManagerWeb : hub Blazor serveur, inscription et releases ;
- HandballIntegration : login Admin, CSV/XLSX, mapping, contrôle de doublons,
  gestion joueurs/utilisateurs et logs ;
- HandballManagerCore : DTO, modèles, rôles et références directes.

## Technologies

| Composant | Technologies principales |
|---|---|
| HandWStat | .NET 10, MAUI, Blazor Hybrid, ApexCharts |
| API | .NET 8, ASP.NET Core, EF Core, Pomelo, Swagger, ClosedXML |
| Web | .NET 8, Blazor Web App interactif serveur |
| Integration | .NET 8 Windows, WPF, MVVM Toolkit, CsvHelper, ClosedXML |
| Core | .NET 8 |
| Base | MySQL, version déclarée 8.0.36 |

## Dépendances

Les quatre applications référencent directement Core. API, Web et Integration
emploient des chemins relatifs entre dépôts. HandWStat emploie une propriété
MSBuild avec découverte locale, dont `D:\repos` en premier choix. L’API est le
seul composant accédant directement à MySQL et au stockage d’artefacts.

## Base MySQL

`HBdbcontext` regroupe utilisateurs, référentiels, équipes, joueurs, matchs,
événements, temps de jeu, historiques et registre de releases. Les relations,
index critiques et flux de lecture/écriture sont documentés par domaine.

Les migrations EF Core MySQL existent. Un second jeu de scripts SQL est
orienté Oracle et ne doit pas être appliqué à MySQL. Aucune sauvegarde,
restauration testée, réplication, rétention ou haute disponibilité n’a été
trouvée.

## Serveurs et réseau

- API Kestrel : écoute confirmée sur toutes les interfaces, port `5000` ;
- production Linux/systemd : **PROBABLE**, service `handapi` ;
- Web : port local `5100`, port de production **À CONFIRMER** ;
- stockage : `/home/opc/api/releases` et
  `/home/opc/HandballWeb/site_telechargement` par défaut Linux ;
- domaine public observé : `handballwstat.ddnsfree.com` en HTTPS ;
- reverse proxy, TLS, firewall, hôte/port MySQL, SSH et services Web/MySQL :
  **À CONFIRMER**.

## Sécurité

L’API utilise JWT HMAC-SHA256 et PBKDF2-SHA256 pour les mots de passe. Les rôles
`Admin` et `Consultation` protègent lectures et mutations. HandWStat et
Integration conservent leur JWT en mémoire. Le Web active antiforgery, HSTS hors
développement et redirection HTTPS.

### Risques P0

1. secret client dans une configuration Integration versionnée ;
2. identifiants MySQL dans des scripts versionnés.

Les valeurs ne figurent pas dans cette documentation. Rotation, retrait de Git,
audit des clones/artefacts et traitement de l’historique sont requis.

## CI/CD

Aucun workflow CI/CD actif n’a été trouvé. Builds, tests, packaging, migrations,
déploiements, copie d’artefacts, approbations, smoke tests et rollback ne sont
pas orchestrés dans les sources accessibles. Une chaîne cible complète est
décrite dans `06-CI-CD-AND-RELEASES.md`.

## Releases

Le registre API gère draft/publication/révocation, artefacts, compatibilité
API/base, builds minimums et rollout. Le Web lit la dernière release publiée.
HandWStat contrôle sa version au démarrage et ouvre l’URL HTTPS de l’artefact.
L’upload/copie du fichier et le déploiement des services restent manuels ou
**À CONFIRMER**.

## Runbooks créés

Le runbook couvre :

- API, Web et MySQL indisponibles ;
- migration et import en échec ;
- incompatibilité HandWStat ;
- artefact absent ;
- certificat expiré et disque plein ;
- erreurs d’authentification et CI/CD ;
- rollback de release.

## Diagrammes

| Fichier | Sujet |
|---|---|
| `ecosystem-current.mmd` | Architecture applicative actuelle |
| `data-domains.mmd` | Domaines et relations de données |
| `deployment-current.mmd` | Déploiement observé/probable |
| `authentication.mmd` | Séquence d’authentification |
| `release-target.mmd` | Pipeline cible |

Des diagrammes Mermaid supplémentaires d’import et de mise à jour sont intégrés
dans les documents.

## Validations exécutées

| Périmètre | Restore | Build | Tests | Résultat |
|---|---|---|---|---|
| API/Web/Core, solution Release | PASS implicite | PASS | API : 49 PASS | 0 erreur, 18 avertissements de compilation |
| HandWStat Windows Release | PASS implicite | PASS | 12 PASS | 0 erreur, 0 avertissement |
| Integration | NON TESTÉ | NON TESTÉ | ABSENTS | Limite volontaire de la phase documentaire |
| Android/iOS/MacCatalyst | NON TESTÉ | NON TESTÉ | NON TESTÉ | Packaging mobile hors budget ciblé |

Les avertissements API concernent notamment des `using` dupliqués, nullabilité
et un ancien nom de migration. Aucun code n’a été corrigé.

La syntaxe des blocs Mermaid et la présence/fermeture des blocs ont été
contrôlées structurellement. Mermaid CLI n’était pas installé ; aucun rendu
graphique automatisé n’a été généré.

## Risques P1

- scripts Oracle incompatibles avec la cible MySQL ;
- aucune sauvegarde/restauration démontrée ;
- absence de CI/CD, staging, smoke tests et rollback ;
- absence de health checks, supervision et logs centralisés ;
- import non atomique ;
- composition incohérente des URLs API ;
- JWT sans issuer/audience, rate limiting ou révocation ;
- dépendance locale directe à Core ;
- disponibilité MySQL non documentée.

## Informations manquantes

- inventaire des serveurs, OS exacts, IP et propriétaires ;
- reverse proxy, DNS, firewall, certificats et renouvellement ;
- services systemd Web/MySQL et comptes système ;
- hôte, port, droits, sauvegarde, restauration et HA MySQL ;
- procédure de packaging/signature Windows et Android ;
- protections GitHub et éventuels pipelines distants non présents localement ;
- statut de retrait des applications historiques ;
- éventuel serveur MCP hors des chemins accessibles.

## Hypothèses explicitement classées

- Linux/systemd en production : **PROBABLE**, fondé sur chemins et commandes ;
- reverse proxy devant Kestrel : **PROBABLE**, fondé sur HTTPS public et HTTP
  interne ;
- transfert manuel des artefacts : **PROBABLE**, aucun endpoint d’upload trouvé ;
- le reste des informations absentes est marqué **À CONFIRMER** dans les
  documents.

## Documents créés ou mis à jour

- `ARCHITECTURE.md`
- `docs/architecture/00-README.md`
- `docs/architecture/01-ECOSYSTEM-OVERVIEW.md`
- `docs/architecture/02-APPLICATIONS.md`
- `docs/architecture/03-DATA-AND-MYSQL.md`
- `docs/architecture/04-SERVER-AND-NETWORK.md`
- `docs/architecture/05-SECURITY.md`
- `docs/architecture/06-CI-CD-AND-RELEASES.md`
- `docs/architecture/07-OPERATIONS-RUNBOOK.md`
- `docs/architecture/08-RISKS-AND-ROADMAP.md`
- `docs/architecture/09-CONFIGURATION-REFERENCE.md`
- `docs/architecture/PROGRESS.md`
- cinq sources Mermaid sous `docs/architecture/diagrams`
- `DOCUMENTATION_REPORT.md`
- redaction d’une valeur sensible dans une documentation historique existante.

## Commits créés

1. `70c2658` — `docs: add ecosystem overview and applications`
2. `e21b744` — `docs: document data infrastructure and security`
3. `5e59632` — `docs: add release operations and risk roadmap`
4. `docs: finalize architecture documentation` — commit contenant ce rapport

## Éléments non analysés pour économiser le budget

- détail fonctionnel des deux applications historiques ;
- inventaire colonne par colonne et endpoint par endpoint ;
- accès distant aux serveurs et à MySQL ;
- builds de toutes les plateformes MAUI ;
- build Integration et tests inexistants de Web/Core/Integration ;
- analyse d’un MCP non trouvé ;
- installation d’un moteur de rendu Mermaid.

## Statistiques RTK

Snapshot global avant le commit final :

- commandes : 809 ;
- tokens d’entrée : 346,0 K ;
- tokens de sortie : 299,0 K ;
- tokens économisés : 46,9 K, soit 13,6 % ;
- temps d’exécution cumulé global : 66 min 11 s.

## Statut final

Toutes les parties accessibles du périmètre prioritaire sont documentées. Les
informations absentes sont distinguées des faits et font l’objet d’actions de
confirmation.

DOCUMENTATION_STATUS=COMPLETE

