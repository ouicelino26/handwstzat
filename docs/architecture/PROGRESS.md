# Progression de la documentation

## Phase terminée

Groupe 3 — CI/CD, releases, runbook et risques.

## Documents créés

- `00-README.md`
- `01-ECOSYSTEM-OVERVIEW.md`
- `02-APPLICATIONS.md`
- `03-DATA-AND-MYSQL.md`
- `04-SERVER-AND-NETWORK.md`
- `05-SECURITY.md`
- `06-CI-CD-AND-RELEASES.md`
- `07-OPERATIONS-RUNBOOK.md`
- `08-RISKS-AND-ROADMAP.md`
- `diagrams/ecosystem-current.mmd`
- `diagrams/data-domains.mmd`
- `diagrams/deployment-current.mmd`
- `diagrams/authentication.mmd`
- `diagrams/release-target.mmd`

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
- registre de releases, versioning, scripts de migration et projets de tests ;
- mécanismes de mise à jour et procédures d’incident.

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
- absence de CI/CD, smoke tests et rollback automatisé ;
- cycle de release actuel partiellement manuel ;
- quinze actions prioritaires de remédiation.

## Éléments à confirmer

- reverse proxy, certificat, topologie et hôtes de production ;
- statut de retrait des deux applications historiques ;
- résolution en production des chemins comportant un double segment `/api` ;
- mode de packaging et publication des applications clientes.
- topologie MySQL, sauvegardes, RPO/RTO et droits SQL ;
- unité systemd, reverse proxy, firewall, certificat et supervision ;
- cible réelle des scripts SQL Oracle.
- services exacts Web/MySQL et contacts d’escalade ;
- RPO/RTO, politique de sauvegarde et procédure N-1 ;
- protections et environnements configurés côté GitHub.

## Prochaine action

Créer la référence de configuration, le portail, valider de façon ciblée et
produire le rapport final.

## Budget de contexte économisé

Les analyses existantes ont été réutilisées. Aucun pipeline n’a été lancé et
aucune application historique n’a été réanalysée. Le runbook utilise uniquement
les services et chemins confirmés, avec des placeholders pour l’inconnu.
