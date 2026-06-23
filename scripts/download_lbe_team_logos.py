#!/usr/bin/env python3
"""
Download official FFHandball/LFH Ligue Butagaz Energie team logos.

The FFHandball competition component can include teams from secondary phases,
such as accession barrages. For Ligue Butagaz Energie, this script keeps the
main championship pool by default, identified as the most represented pouleId.
"""

from __future__ import annotations

import argparse
import collections
import json
import sys
import time
from pathlib import Path
from typing import Any

from download_n1f_team_logos import (
    DEFAULT_MEDIA_BASE_URL,
    TeamLogoSource,
    build_logo_urls,
    download_logo_with_official_fallbacks,
    normalize_logo_png,
    parse_teams_from_page,
    parse_teams_without_logo,
    request_bytes,
    unique_output_path,
    write_json,
)


DEFAULT_SEASON_PAGES = {
    "2024-2025": "https://www.ffhandball.fr/competitions/saison-2024-2025-20/national/ligue-butagaz-energie-2024-25-25625/poule-147028/",
    "2025-2026": "https://www.ffhandball.fr/competitions/saison-2025-2026-21/national/ligue-butagaz-energie-2025-26-28227/poule-168256/",
}


def select_main_pool(teams: list[TeamLogoSource]) -> tuple[str, list[TeamLogoSource], list[TeamLogoSource]]:
    counts = collections.Counter(team.poule_id for team in teams if team.poule_id)
    if not counts:
        return "", teams, []

    main_poule_id = counts.most_common(1)[0][0]
    included = [team for team in teams if team.poule_id == main_poule_id]
    excluded = [team for team in teams if team.poule_id != main_poule_id]
    return main_poule_id, included, excluded


def make_web_path(output_path: Path) -> str:
    parts = output_path.resolve().parts
    if "wwwroot" not in parts:
        return output_path.as_posix()

    root_index = parts.index("wwwroot")
    return "/" + "/".join(parts[root_index + 1 :])


def download_lbe_season(
    season: str,
    page_url: str,
    output_root: Path,
    media_base_url: str,
    source_sizes: list[int],
    canvas_size: int,
    overwrite: bool,
    sleep_seconds: float,
    include_secondary_poules: bool,
) -> dict[str, Any]:
    page_html = request_bytes(page_url).decode("utf-8", errors="replace")
    all_teams = parse_teams_from_page(season, page_url, page_html)
    missing = parse_teams_without_logo(season, page_url, page_html)
    main_poule_id, teams, excluded = select_main_pool(all_teams)

    if include_secondary_poules:
        teams = all_teams
        excluded = []

    season_dir = output_root / season
    season_dir.mkdir(parents=True, exist_ok=True)

    downloaded: list[dict[str, Any]] = []
    errors: list[dict[str, Any]] = []

    for index, team in enumerate(teams, start=1):
        output_path = unique_output_path(season_dir, team)
        source_url = build_logo_urls(media_base_url, team, source_sizes)[0]
        effective_logo_filename = team.source_logo_filename
        effective_source_page_url = team.source_page_url
        source_kind = "competition"
        image_info: dict[str, int | str] = {}

        try:
            if overwrite or not output_path.exists():
                (
                    source_url,
                    raw_logo,
                    effective_logo_filename,
                    effective_source_page_url,
                    source_kind,
                ) = download_logo_with_official_fallbacks(media_base_url, team, source_sizes)
                image_info = normalize_logo_png(raw_logo, output_path, canvas_size)
                if sleep_seconds:
                    time.sleep(sleep_seconds)

            downloaded.append(
                {
                    "season": season,
                    "name": team.name,
                    "teamId": team.team_id,
                    "externalTeamId": team.external_team_id,
                    "pouleId": team.poule_id,
                    "structureId": team.structure_id,
                    "externalStructureId": team.external_structure_id,
                    "sourceLogoFilename": effective_logo_filename,
                    "sourceLogoUrl": source_url,
                    "sourcePageUrl": effective_source_page_url,
                    "sourceKind": source_kind,
                    "localPath": output_path.as_posix(),
                    "webPath": make_web_path(output_path),
                    "index": index,
                    **image_info,
                }
            )
        except Exception as error:  # noqa: BLE001 - script should report all failures.
            errors.append(
                {
                    "season": season,
                    "name": team.name,
                    "sourceLogoFilename": team.source_logo_filename,
                    "sourceLogoUrlsTried": build_logo_urls(media_base_url, team, source_sizes),
                    "sourcePageUrl": team.source_page_url,
                    "error": str(error),
                }
            )

    return {
        "sourcePageUrl": page_url,
        "mainPouleId": main_poule_id,
        "includeSecondaryPoules": include_secondary_poules,
        "downloadedCount": len(downloaded),
        "missingLogoCount": len(missing),
        "errorCount": len(errors),
        "excludedSecondaryPouleCount": len(excluded),
        "teams": downloaded,
        "missing": missing,
        "errors": errors,
        "excludedSecondaryPouleTeams": [
            {
                "season": team.season,
                "name": team.name,
                "pouleId": team.poule_id,
                "teamId": team.team_id,
                "externalTeamId": team.external_team_id,
                "sourceLogoFilename": team.source_logo_filename,
                "sourcePageUrl": team.source_page_url,
            }
            for team in excluded
        ],
    }


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Download official Ligue Butagaz Energie team logos for the app.",
    )
    parser.add_argument(
        "--out",
        default="wwwroot/images/team-logos/lbe",
        help="Output directory for normalized PNG logos and manifests.",
    )
    parser.add_argument(
        "--season",
        action="append",
        choices=sorted(DEFAULT_SEASON_PAGES),
        help="Season to download. Can be repeated. Defaults to all configured seasons.",
    )
    parser.add_argument(
        "--media-base-url",
        default=DEFAULT_MEDIA_BASE_URL,
        help="FFHandball media base URL for club logos.",
    )
    parser.add_argument(
        "--source-size",
        type=int,
        action="append",
        default=None,
        help="Source logo size to try. Can be repeated. Defaults to 512, 256, 128, 64.",
    )
    parser.add_argument(
        "--canvas-size",
        type=int,
        default=512,
        help="Output PNG canvas size in pixels.",
    )
    parser.add_argument(
        "--overwrite",
        action="store_true",
        help="Overwrite existing files.",
    )
    parser.add_argument(
        "--sleep",
        type=float,
        default=0.05,
        help="Delay between downloads, in seconds.",
    )
    parser.add_argument(
        "--include-secondary-poules",
        action="store_true",
        help="Also download teams listed in secondary phases, such as accession barrages.",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    output_root = Path(args.out).resolve()
    seasons = args.season or sorted(DEFAULT_SEASON_PAGES)
    source_sizes = args.source_size or [512, 256, 128, 64]

    manifest: dict[str, Any] = {
        "source": "FFHandball / Ligue Feminine de Handball",
        "competition": "Ligue Butagaz Energie",
        "mediaBaseUrl": args.media_base_url,
        "canvasSize": args.canvas_size,
        "sourceSizesTried": source_sizes,
        "seasons": {},
    }
    all_missing: list[dict[str, Any]] = []
    all_errors: list[dict[str, Any]] = []
    all_excluded: list[dict[str, Any]] = []

    for season in seasons:
        season_data = download_lbe_season(
            season=season,
            page_url=DEFAULT_SEASON_PAGES[season],
            output_root=output_root,
            media_base_url=args.media_base_url,
            source_sizes=source_sizes,
            canvas_size=args.canvas_size,
            overwrite=args.overwrite,
            sleep_seconds=args.sleep,
            include_secondary_poules=args.include_secondary_poules,
        )

        all_missing.extend(season_data.pop("missing"))
        all_errors.extend(season_data.pop("errors"))
        all_excluded.extend(season_data.get("excludedSecondaryPouleTeams", []))
        manifest["seasons"][season] = season_data

    write_json(output_root / "lbe-team-logos-manifest.json", manifest)

    missing_path = output_root / "lbe-team-logos-missing.json"
    errors_path = output_root / "lbe-team-logos-errors.json"
    excluded_path = output_root / "lbe-team-logos-excluded-secondary-poules.json"

    if all_missing:
        write_json(missing_path, all_missing)
    elif missing_path.exists():
        missing_path.unlink()

    if all_errors:
        write_json(errors_path, all_errors)
    elif errors_path.exists():
        errors_path.unlink()

    if all_excluded:
        write_json(excluded_path, all_excluded)
    elif excluded_path.exists():
        excluded_path.unlink()

    print(f"Output: {output_root}")
    for season in seasons:
        season_data = manifest["seasons"][season]
        print(
            f"{season}: {season_data['downloadedCount']} logos, "
            f"{season_data['missingLogoCount']} missing, "
            f"{season_data['errorCount']} errors, "
            f"{season_data['excludedSecondaryPouleCount']} secondary-poule excluded"
        )

    if all_errors:
        print("Some logos failed to download. See lbe-team-logos-errors.json", file=sys.stderr)
        return 1

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
