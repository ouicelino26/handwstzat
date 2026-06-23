# 00 — État initial du projet

## 1. État Git

- Branche active : `main`
- Dernier commit : `5c79aed Initial commit`
- Fichiers non suivis : `prompt.md` (hors périmètre applicatif)
- Fichiers modifiés : aucun
- Sous-modules : aucun
- Tags : aucun

## 2. Stack technique exacte

| Couche | Technologie | Version |
|---|---|---|
| Framework applicatif | .NET MAUI Blazor Hybrid | net10.0 |
| Langage | C# 13 (Nullable enable, ImplicitUsings) | .NET 10 |
| UI | Blazor WebView (Razor Components) | MAUI 10 |
| Graphiques | Blazor-ApexCharts-MAUI | 6.0.2 |
| Graphiques 2 | jQWidgets.Blazor (BarGauge) | 1.3.0 |
| CSS | Vanilla CSS custom (app.css + scoped) | — |
| Polices | Open Sans Regular (TTF embarquée) | — |
| Dépendance externe | `HandballManagerCore` (ProjectReference) | D:\repos\HandballManagerCore\ |
| Plateformes cibles | Windows 10.0.19041 (principal), Android 24+, iOS 15+, macCatalyst 15+ | — |

**Dépendance critique :** Le projet référence `D:\repos\HandballManagerCore\HandballManagerCore\HandballManagerCore.csproj` par chemin absolu local. Ce projet fournit tous les DTO (`HandballManagerCore.DTO`) et modèles (`HandballManagerCore.Models`). Il ne peut pas être compilé sans cette dépendance présente localement.

## 3. Commandes de base

```bash
# Restauration des dépendances
dotnet restore

# Build Windows (plateforme principale de développement)
dotnet build -f net10.0-windows10.0.19041.0

# Lancement en mode développement (Windows)
dotnet run -f net10.0-windows10.0.19041.0

# Build Android
dotnet build -f net10.0-android

# Lint
dotnet format --verify-no-changes

# Tests
# Aucun projet de test présent dans ce dépôt
```

## 4. Configuration d'exécution

### appsettings.json (embarqué)

```json
{
  "ApiSettings": {
    "BaseUrl": "https://handballwstat.ddnsfree.com/api/",
    "ClientId": "my-HandApp-id",
    "ClientSecret": "sg321sef6e5sfes321fse3f21"
  }
}
```

> ⚠️ Le `ClientSecret` est présent en clair dans le dépôt. Il s'agit d'un secret applicatif. À déplacer dans un fichier ignoré par Git lors d'une prochaine itération si ce dépôt est public.

### Authentification API

- Mode Bearer JWT via `POST auth/login`
- Pas de refresh token géré côté client
- Session en mémoire uniquement (perd la session au redémarrage)

## 5. État des tests

- **Aucun projet de test** n'est présent dans le dépôt
- Pas de tests unitaires, pas de tests d'intégration, pas de tests end-to-end
- Les calculs KPI sont dans `HandballKpiHelper` (KpiModels.cs) et `MatchScenarioAnalyzer` — non couverts par des tests automatisés

## 6. Fichiers générés / artefacts

| Dossier | Nature |
|---|---|
| `bin/` | Artefacts de build (ignorés par Git) |
| `obj/` | Fichiers intermédiaires MSBuild (ignorés par Git) |
| `.vs/` | Métadonnées Visual Studio (ignorés par Git) |
| `wwwroot/lib/bootstrap/` | Bootstrap CSS embarqué (lib externe) |
| `Resources/Fonts/OpenSans-Regular.ttf` | Police embarquée |
| `Resources/AppIcon/`, `Resources/Splash/` | Assets MAUI générés au build |

## 7. Prérequis au lancement

1. .NET 10 SDK installé
2. Workload MAUI : `dotnet workload install maui`
3. `HandballManagerCore.csproj` présent à `D:\repos\HandballManagerCore\`
4. API backend accessible à `https://handballwstat.ddnsfree.com/api/`
5. Pour Windows : Windows 10 build 19041 minimum
6. Pour Android : Android SDK + émulateur ou appareil Android API 24+
