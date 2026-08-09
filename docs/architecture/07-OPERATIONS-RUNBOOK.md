# Runbook d’exploitation

## Objectif et garde-fous

Fournir un premier niveau de diagnostic. Les noms entre chevrons sont à
remplacer par des valeurs approuvées. Ne jamais coller un secret dans une ligne
de commande ou un ticket.

Les actions de redémarrage, modification de base, suppression de fichier,
révocation de release ou restauration sont des changements de production :
elles exigent autorisation, sauvegarde et traçabilité.

## Architecture actuelle

Les seuls éléments d’exploitation confirmés sont le service API probablement
nommé `handapi`, les journaux systemd pour ce service, le port API `5000`, les
répertoires de releases et les logs locaux d’Integration. Les services Web/MySQL
et l’outil de supervision sont **À CONFIRMER**.

## Incidents

### API indisponible

- **Symptômes** : erreurs réseau dans HandWStat/Integration, Web sans release.
- **Vérifications** : DNS/TLS, `GET /api/system/version`, écoute du port interne,
  état `systemctl status handapi`.
- **Logs à consulter** : `journalctl -u handapi`, reverse proxy à confirmer.
- **Actions sûres** : relever l’erreur et l’heure, vérifier disque/configuration
  présente, comparer avec le dernier déploiement.
- **Action de rollback** : revenir au binaire/configuration N-1 selon procédure
  approuvée ; ne pas migrer la base à l’aveugle.
- **Escalade** : exploitant API puis DBA si la connexion MySQL échoue.

### Web indisponible

- **Symptômes** : page racine inaccessible ou erreur Blazor.
- **Vérifications** : tester Web puis API séparément, TLS, port/service Web.
- **Logs à consulter** : sortie ASP.NET Core du service Web et reverse proxy.
- **Actions sûres** : confirmer que `HandballApi:BaseUrl` pointe vers l’API
  attendue ; vérifier l’espace disque.
- **Action de rollback** : redéployer la version Web N-1 approuvée.
- **Escalade** : exploitant Web/reverse proxy. Nom du service à confirmer.

### MySQL indisponible

- **Symptômes** : API en erreur 5xx, échec au démarrage, timeouts.
- **Vérifications** : état de `<mysql-service>`, connectivité depuis l’hôte API,
  capacité disque et connexions ; ne pas afficher la chaîne.
- **Logs à consulter** : journal MySQL et journal API.
- **Actions sûres** : collecter les erreurs, vérifier hôte/DNS/certificat et
  saturation.
- **Action de rollback** : aucune ; un redémarrage ou failover doit suivre le
  runbook DBA approuvé.
- **Escalade** : DBA immédiatement si corruption, disque plein ou réplication.

### Échec de migration

- **Symptômes** : erreur SQL, API incompatible ou service resté arrêté.
- **Vérifications** : moteur réel, version/historique EF, sauvegarde disponible,
  état `handapi`.
- **Logs à consulter** : sortie de migration, MySQL, API.
- **Actions sûres** : arrêter la séquence de déploiement, conserver les logs,
  comparer le schéma à la migration attendue sur un clone.
- **Action de rollback** : restauration ou migration compensatrice uniquement
  si elle a été testée. **Ne jamais exécuter les scripts Oracle sur MySQL.**
- **Escalade** : DBA + développeur API.

### Erreur d’import

- **Symptômes** : fichier en erreur, match partiel, joueurs non résolus.
- **Vérifications** : statut du fichier, présence du match et nombre
  d’événements/temps déjà créés avant toute relance.
- **Logs à consulter** : `integration_errors.log`, `integration_skips.log`,
  `integration_halftime.log`, `integration_time_errors.log`, logs API.
- **Actions sûres** : conserver le fichier source, corriger mapping/référentiel
  via les écrans prévus, comparer avec les doublons détectés.
- **Action de rollback** : ne pas supprimer en masse ; faire valider la
  compensation des lignes par un administrateur métier.
- **Escalade** : administrateur de données puis développeur Integration/API.

### HandWStat incompatible

- **Symptômes** : écran de mise à jour obligatoire ou aucune donnée.
- **Vérifications** : `/api/system/version`, release publiée compatible,
  plateforme/architecture/build et URL de téléchargement.
- **Logs à consulter** : logs API update ; logs client disponibles à confirmer.
- **Actions sûres** : publier/corriger uniquement une release validée, vérifier
  les bornes API/base et le build minimum.
- **Action de rollback** : révoquer la release fautive et confirmer qu’une
  release précédente compatible reste publiée.
- **Escalade** : responsable release + développeur HandWStat/API.

### Artefact de téléchargement absent

- **Symptômes** : Hub « artefact manquant », 404 ou lien désactivé.
- **Vérifications** : release `PUBLISHED`, artefact `Active`, URL HTTPS, taille,
  SHA-256, présence et droits du fichier.
- **Logs à consulter** : API et Web.
- **Actions sûres** : comparer le fichier au hash déclaré ; déposer l’artefact
  avant de publier ses métadonnées.
- **Action de rollback** : révoquer la release défectueuse après approbation.
- **Escalade** : responsable release et exploitant stockage.

### Certificat expiré

- **Symptômes** : erreur TLS dans tous les clients.
- **Vérifications** : dates, chaîne, nom DNS, horloge et processus de
  renouvellement du certificat public.
- **Logs à consulter** : reverse proxy/ACME, composants non trouvés dans le dépôt.
- **Actions sûres** : renouveler via le mécanisme officiel puis vérifier la
  chaîne complète.
- **Action de rollback** : restaurer le dernier certificat valide seulement s’il
  est encore valide et autorisé.
- **Escalade** : propriétaire DNS/TLS.

### Disque plein

- **Symptômes** : écritures SQL/logs impossibles, upload ou démarrage en échec.
- **Vérifications** : `df -h`, `df -i`, volumes MySQL, releases et journaux.
- **Logs à consulter** : système, MySQL, API.
- **Actions sûres** : identifier les gros consommateurs et appliquer la politique
  de rétention.
- **Action de rollback** : aucune. **Ne supprimer aucun fichier de base, artefact
  publié ou log sans validation et sauvegarde.**
- **Escalade** : système + DBA.

### Erreur d’authentification

- **Symptômes** : 401/403, Integration refuse un non-Admin.
- **Vérifications** : utilisateur actif, rôle, horloge, présence du secret JWT,
  expiration du token et HTTPS.
- **Logs à consulter** : API Auth ; ne pas journaliser mot de passe/JWT.
- **Actions sûres** : se reconnecter, vérifier l’identité et le rôle, invalider
  les sessions selon la procédure disponible.
- **Action de rollback** : restaurer la configuration N-1 uniquement si le
  secret correspondant est maîtrisé ; préférer une rotation contrôlée.
- **Escalade** : sécurité + administrateur API.

### Échec CI/CD

- **Symptômes** : sans objet actuellement ; aucune CI active n’a été trouvée.
- **Vérifications** : identifier la procédure manuelle, le commit, SDK et
  artefacts utilisés.
- **Logs à consulter** : sortie locale de restore/build/test/déploiement.
- **Actions sûres** : arrêter la publication, conserver les artefacts déjà
  validés et reproduire sur un environnement propre.
- **Action de rollback** : ne pas promouvoir ; conserver production N-1.
- **Escalade** : DevOps/responsable release.

### Rollback d’une release

- **Symptômes** : crash, incompatibilité, artefact invalide ou régression.
- **Vérifications** : impact API/base/client, release N-1 publiée et sauvegarde.
- **Logs à consulter** : API, Web, update events et supervision disponible.
- **Actions sûres** : geler le rollout, révoquer la release fautive après
  approbation, vérifier N-1 et communiquer.
- **Action de rollback** : redéployer N-1 ; restaurer la base seulement si le
  schéma l’exige et qu’une restauration testée existe.
- **Escalade** : responsable incident, DevOps, DBA et métier.

## Architecture cible recommandée

Chaque procédure devra recevoir un propriétaire, un service exact, des
commandes validées, des seuils, un canal d’escalade, un RTO/RPO et une preuve
d’exercice. Organiser au moins un test de restauration et un exercice de
rollback par trimestre.

## Sources

- `<HandballManagerAPI>/HandballManagerAPI/Program.cs`
- `<HandballManagerAPI>/apply-migration.sh`, `check-schema.sh`
- `<HandballManagerAPI>/HandballManagerAPI/Controllers/*Release*`
- `<HandballManagerAPI>/HandballManagerAPI/Services/Updates/*`
- `<HandballIntegration>/HandballIntegration/ViewModels/*`
- `<HandWStat>/Services/Updates/*`

