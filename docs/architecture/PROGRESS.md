# Progression de la documentation

## Phase terminée

Groupe 2 — données, serveur et sécurité.

## Documents créés

- `00-README.md`
- `01-ECOSYSTEM-OVERVIEW.md`
- `02-APPLICATIONS.md`
- `03-DATA-AND-MYSQL.md`
- `04-SERVER-AND-NETWORK.md`
- `05-SECURITY.md`
- `diagrams/ecosystem-current.mmd`
- `diagrams/data-domains.mmd`
- `diagrams/deployment-current.mmd`
- `diagrams/authentication.mmd`

## Sources utilisées

- projets et points d’entrée des cinq composants actifs ;
- contrôleurs et services structurants de l’API ;
- clients HTTP et mécanisme de mise à jour HandWStat ;
- programmes, pages et clients du Web ;
- services et view-models d’Integration ;
- DTO et modèles de Core.
- `HBdbcontext`, migrations EF et scripts SQL ;
- programmes et profils d’hébergement API/Web ;
- authentification, hachage de mots de passe et politiques d’autorisation.

## Éléments confirmés

- séparation client/API/Web/Integration/Core ;
- accès MySQL uniquement par l’API ;
- JWT et rôles `Admin`/`Consultation` ;
- registre central de releases et contrôle de mise à jour ;
- absence de serveur MCP et de workflow CI/CD dans les dépôts accessibles.
- MySQL/Pomelo comme persistance runtime ;
- Kestrel `5000`, service `handapi` probable et stockages sous `/home/opc` ;
- deux catégories de secrets versionnés, sans reproduction de valeur ;
- incompatibilité entre les scripts Oracle du registre et la cible MySQL.

## Éléments à confirmer

- reverse proxy, certificat, topologie et hôtes de production ;
- statut de retrait des deux applications historiques ;
- résolution en production des chemins comportant un double segment `/api` ;
- mode de packaging et publication des applications clientes.
- topologie MySQL, sauvegardes, RPO/RTO et droits SQL ;
- unité systemd, reverse proxy, firewall, certificat et supervision ;
- cible réelle des scripts SQL Oracle.

## Prochaine action

Documenter CI/CD, releases, runbook et risques, puis créer le troisième commit.

## Budget de contexte économisé

Les packs sûrs et les lectures déjà réalisées ont été réutilisés. Deux packs
temporaires capturant un identifiant sensible ont été supprimés sans
régénération. Les prototypes historiques et les gros fichiers non nécessaires
n’ont pas été réanalysés.
