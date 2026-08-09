# Documentation d’architecture de l’écosystème Handball

## Objectif et périmètre

Cette documentation décrit l’architecture observée des composants actifs suivants :

- HandWStat ;
- HandballManagerAPI ;
- HandballManagerWeb ;
- HandballIntegration ;
- HandballManagerCore ;
- MySQL 8 et le stockage de releases associé.

Les applications historiques sont seulement signalées. Aucun serveur MCP propre à
l’écosystème n’a été trouvé dans les dépôts accessibles.

## Comment lire les constats

- **CONFIRMÉ** : observé directement dans le code ou une configuration.
- **PROBABLE** : déduction étayée par plusieurs éléments.
- **À CONFIRMER** : information absente ou ambiguë.
- **CIBLE RECOMMANDÉE** : proposition, non constatée dans l’existant.

Les sections « Architecture actuelle » et « Architecture cible recommandée » ne
doivent pas être confondues. Les valeurs de secrets ne sont jamais reproduites.

## Documents

1. [Vue globale](01-ECOSYSTEM-OVERVIEW.md)
2. [Applications](02-APPLICATIONS.md)
3. [Données et MySQL](03-DATA-AND-MYSQL.md)
4. [Serveur et réseau](04-SERVER-AND-NETWORK.md)
5. [Sécurité](05-SECURITY.md)
6. [CI/CD et releases](06-CI-CD-AND-RELEASES.md)
7. [Runbook d’exploitation](07-OPERATIONS-RUNBOOK.md)
8. [Risques et roadmap](08-RISKS-AND-ROADMAP.md)
9. [Référence de configuration](09-CONFIGURATION-REFERENCE.md)
10. [État d’avancement](PROGRESS.md)

Le portail court se trouve dans [ARCHITECTURE.md](../../ARCHITECTURE.md) et le
bilan de mission dans [DOCUMENTATION_REPORT.md](../../DOCUMENTATION_REPORT.md).

## Racines analysées

| Dépôt | Racine observée |
|---|---|
| HandWStat | `C:\Users\donov\source\repos\HandWStat` |
| HandballManagerAPI et Web | `D:\repos\HandballManagerAPI` |
| HandballIntegration | `D:\repos\HandballIntegration` |
| HandballManagerCore | `D:\repos\HandballManagerCore` |

Ces chemins décrivent le poste audité ; ils ne constituent pas une convention de
déploiement.

## Sources principales

- `<HandWStat>/HandWStat.csproj`
- `<HandWStat>/MauiProgram.cs`
- `<HandballManagerAPI>/HandballManagerAPI/Program.cs`
- `<HandballManagerAPI>/HandballManagerAPI/Datas/HBdbcontext.cs`
- `<HandballManagerAPI>/HandballManagerWeb/Program.cs`
- `<HandballIntegration>/HandballIntegration/App.xaml.cs`
- `<HandballManagerCore>/HandballManagerCore/HandballManagerCore.csproj`

