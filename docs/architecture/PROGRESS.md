# Progression de la documentation

## Phase terminée

Groupe 1 — vue globale et applications.

## Documents créés

- `00-README.md`
- `01-ECOSYSTEM-OVERVIEW.md`
- `02-APPLICATIONS.md`
- `diagrams/ecosystem-current.mmd`

## Sources utilisées

- projets et points d’entrée des cinq composants actifs ;
- contrôleurs et services structurants de l’API ;
- clients HTTP et mécanisme de mise à jour HandWStat ;
- programmes, pages et clients du Web ;
- services et view-models d’Integration ;
- DTO et modèles de Core.

## Éléments confirmés

- séparation client/API/Web/Integration/Core ;
- accès MySQL uniquement par l’API ;
- JWT et rôles `Admin`/`Consultation` ;
- registre central de releases et contrôle de mise à jour ;
- absence de serveur MCP et de workflow CI/CD dans les dépôts accessibles.

## Éléments à confirmer

- reverse proxy, certificat, topologie et hôtes de production ;
- statut de retrait des deux applications historiques ;
- résolution en production des chemins comportant un double segment `/api` ;
- mode de packaging et publication des applications clientes.

## Prochaine action

Documenter MySQL, l’infrastructure et la sécurité, puis créer le deuxième commit.

## Budget de contexte économisé

Les packs Repomix existants et les lectures déjà réalisées ont été réutilisés.
Les prototypes historiques et les gros fichiers non nécessaires n’ont pas été
réanalysés.

