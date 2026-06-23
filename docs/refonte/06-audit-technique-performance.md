# 06 — Audit technique et performance

## 1. Architecture

### 1.1 Points forts
- Architecture en couches claire : pages → services → clients API → base HTTP
- Séparation frontend / backend correcte (pas de logique backend dans l'UI)
- Injection de dépendances bien configurée dans `MauiProgram.cs`
- Clients API bien isolés par domaine (`CompetitionsApiClient`, `TeamsApiClient`, etc.)
- `ApiClientBase` centralise la gestion HTTP, l'authentification et les erreurs

### 1.2 Problèmes identifiés

| ID | Problème | Fichier | Sévérité |
|---|---|---|---|
| TECH-01 | `StatsDashboardService` : 17 appels API dans un seul `Task.WhenAll`, dont 10 supplémentaires pour le spotlight | `StatsDashboardService.cs:824-918` | Haute |
| TECH-02 | Pas de cancellation token propagé depuis les composants vers les services (les CancellationToken des méthodes ne sont pas utilisés dans les pages) | Pages Razor | Moyenne |
| TECH-03 | Services API registrés en Singleton + HttpClient en Singleton → partage d'état non sécurisé en environnement multi-thread | `MauiProgram.cs:21` | Haute |
| TECH-04 | `DemoDataFactory` construit des objets `PlayerOffenseStatsDto`, `PlayerDefenseStatsDto`, etc. directement → couplage fort avec les contrats API | `DemoDataFactory.cs` | Moyenne |
| TECH-05 | Référence absolue à `D:\repos\HandballManagerCore\` dans le `.csproj` → non portable | `HandWStat.csproj:55` | Critique |
| TECH-06 | `Counter.razor` résiduel du template — code mort | `Components/Pages/Counter.razor` | Faible |
| TECH-07 | La logique de sélection du spotlight est dans le service orchestrateur (`ResolveSelectedPlayerId`) — responsabilité UI dans la couche service | `StatsDashboardService.cs:978` | Faible |
| TECH-08 | Aucune gestion du cache HTTP (pas d'ETag, de Cache-Control, de durée d'expiration) | `ApiClientBase.cs` | Moyenne |
| TECH-09 | Pas de retry ni de backoff sur les appels API | `ApiClientBase.cs` | Faible |
| TECH-10 | Le secret API (`ClientSecret`) est en clair dans `appsettings.json` embarqué | `appsettings.json` | Haute (sécurité) |

---

## 2. Performance

### 2.1 Chargement initial
- **Problème principal** : 17 requêtes HTTP simultanées au premier chargement du dashboard → peut saturer le pool de connexions ou être ralenti par le serveur distant
- Pas de chargement différé (lazy loading) des sections non visibles immédiatement
- Pas de skeleton UI — l'utilisateur voit un écran vide ou partiellement vide pendant le chargement

### 2.2 Tableaux
- Chargement de 250 éléments par défaut (`pageSize: 250`) sans virtualisation
- Pour 250 lignes × N colonnes, le rendu est direct dans le DOM — peut provoquer des lenteurs sur mobile ou Android bas de gamme
- Tri côté client sur les données déjà chargées — performant mais dépend du volume

### 2.3 Graphiques
- ApexCharts : bibliothèque JavaScript chargée dans le WebView — taille correcte
- jQWidgets (BarGauge) : bibliothèque complète chargée pour un seul type de composant → candidat à remplacement
- Les graphiques se chargent en même temps que le reste de la page — pas de lazy loading

### 2.4 Re-rendus
- Pas de `ShouldRender()` implémenté dans les composants — tous les changements d'état déclenchent un re-rendu complet
- `StateHasChanged()` appelé directement depuis les événements de service — peut provoquer des re-rendus non nécessaires

### 2.5 Bundle
- Pas de minification CSS personnalisée (MAUI gère la sortie)
- Bootstrap CSS embarqué mais non utilisé (~30 Ko inutiles)
- jQWidgets : taille estimée > 300 Ko minifiée

---

## 3. Gestion des erreurs

| Cas | Comportement | Qualité |
|---|---|---|
| 401 Unauthorized | `InvalidOperationException("Connexion non autorisee")` | OK mais message générique |
| 403 Forbidden | Même traitement que 401 | OK |
| Autre erreur HTTP | `InvalidOperationException` avec code + détails | OK |
| Timeout réseau | `OperationCanceledException` non capturée spécifiquement | Faible |
| Exception dans `LoadDashboardAsync` | Re-levée avec contexte | OK |
| Données manquantes (null) | Fallbacks créés dans le service | OK mais complexe |

---

## 4. Sécurité

| Problème | Fichier | Impact |
|---|---|---|
| `ClientSecret` en clair dans `appsettings.json` embarqué dans le binaire | `appsettings.json` | Les secrets peuvent être extraits d'un APK/IPA |
| Token JWT stocké en mémoire uniquement — pas de SecureStorage | `ApiAuthService.cs` | Sécurisé (volatil) mais perd la session au redémarrage |
| Pas de validation du certificat SSL personnalisée | `ApiClientBase.cs` | Standard MAUI (OK par défaut) |
| Pas de timeout sur l'authentification (timeout global 30s) | `MauiProgram.cs:22` | Acceptable |

---

## 5. Maintenabilité

| Aspect | Constat |
|---|---|
| Conventions de nommage | Cohérentes (PascalCase, suffixe `Dto`, `Service`, `Client`) |
| Taille des fichiers | `StatsDashboardService.cs` : ~1400 lignes — candidat à découpage |
| `DemoDataFactory.cs` | ~680 lignes — données hardcodées volumineuses |
| `app.css` | ~1600+ lignes estimées — pas de découpage par module |
| Tests | **Aucun** — zéro couverture |
| Documentation interne | Bonne (KPI_REFERENCE.md, KPI_REFERENCE_DETAILED.md) |
| Duplication | Les 7 méthodes `CreateXxxFallback` dans `StatsDashboardService` sont similaires — DRY non respecté |

---

## 6. Observabilité

| Aspect | Constat |
|---|---|
| Logging | `Microsoft.Extensions.Logging.Debug` en mode DEBUG uniquement |
| Télémétrie | Aucune |
| Monitoring performance | Aucun |
| Traçage des erreurs | Pas d'erreur reporting centralisé |

---

## 7. Recommandations prioritaires

| # | Recommandation | Priorité |
|---|---|---|
| 1 | Externaliser `ClientSecret` hors du binaire (variable d'environnement ou SecureStorage) | P0 |
| 2 | Rendre la référence `HandballManagerCore` portable (NuGet package ou chemin relatif configurable) | P0 |
| 3 | Ajouter chargement progressif sur le dashboard (skeleton UI, sections lazy) | P1 |
| 4 | Changer l'enregistrement des services DI (Scoped plutôt que Singleton pour les clients API) | P1 |
| 5 | Supprimer Bootstrap CSS inutilisé | P2 |
| 6 | Ajouter au moins des tests unitaires sur `HandballKpiHelper` et `MatchScenarioAnalyzer` | P1 |
| 7 | Envisager de remplacer jQWidgets BarGauge par un composant CSS natif | P2 |
| 8 | Découper `StatsDashboardService` en deux : orchestrateur + résolveur de spotlight | P2 |
