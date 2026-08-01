# PLAYER_IDENTITY_DUPLICATE_AUDIT

Date: 2026-08-02 | Source: DB hbdb audit | Branch: fix/player-time-availability-v1

## Mécanisme de détection

On cherche des joueuses qui seraient en réalité la même personne mais enregistrées avec plusieurs PlayerId.

Indicateurs :
- Même nom normalisé + même équipe
- Même date de naissance + nom similaire

## Résultats

### histoplayer

La table `histoplayer` est **vide (0 lignes)** — aucun historique de modification de fiche disponible. Il est impossible de tracer des fusions ou scissions de fiches joueuses via cet outil.

### Doublons potentiels par nom

Seule la famille `KABEYA` apparaît 3 fois dans `players.Name`. Cela représente 3 joueuses différentes portant le même nom de famille (cas réel dans le handball féminin professionnel français).

Sans BirthDate commune ou TeamId commun identique, il n'est pas possible de confirmer une duplication.

### Joueuse supprimée (PlayerId=239)

La joueuse `DEMBELE MAHOUA` (BBH) a été supprimée de `players` (Id=239 absent). Ses 19 lignes timeplayers sont orphelines. Il n'existe pas de nouvelle fiche pour cette joueuse dans la base actuelle → impossible de rattacher automatiquement.

### Transferts

La table `players` contient `TeamId` qui représente l'équipe actuelle uniquement. Une joueuse transférée n'a qu'une seule fiche (son `TeamId` change lors de la modification, mais sans historique via `histoplayer`).

## Classification

| Catégorie | Nombre | Cas |
|---|---|---|
| SAME_PERSON_CONFIRMED_BY_STABLE_ID | 0 | Aucun identifiant stable externe disponible |
| LIKELY_SAME_PERSON | 0 | Aucun doublon détecté |
| AMBIGUOUS | 0 | KABEYA × 3 = personnes différentes (pas de preuve contraire) |
| DISTINCT_PERSON | Tous | Par défaut |
| INSUFFICIENT_DATA | histoplayer | Table vide |

## Conclusion

DUPLICATE_PLAYER_IDENTITIES=NOT_DETECTED
HISTORICAL_IDENTITY_RESOLUTION=UNAVAILABLE (histoplayer vide)
TRANSFER_RESOLUTION=NOT_NEEDED (aucun doublon confirmé)
