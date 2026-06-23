#!/usr/bin/env python3
"""
Download official FFHandball N1F team logos for selected seasons.

The FFHandball competition pages expose teams in Smartfire component JSON.
Team logo filenames are then transformed by the frontend with this rule:
https://media-logos-clubs.ffhandball.fr/{size}/{logo_stem}.webp

The script stores normalized PNG files in the app wwwroot folder and writes a
manifest that keeps the original source metadata.
"""

from __future__ import annotations

import argparse
import html
import io
import json
import re
import sys
import time
import unicodedata
import urllib.error
import urllib.request
from dataclasses import dataclass
from html.parser import HTMLParser
from pathlib import Path
from typing import Any

from PIL import Image


DEFAULT_SEASON_PAGES = {
    "2024-2025": "https://www.ffhandball.fr/competitions/saison-2024-2025-20/national/nationale-1-feminine-2024-25-25933/poule-148254/",
    "2025-2026": "https://www.ffhandball.fr/competitions/saison-2025-2026-21/national/nationale-1-feminine-2025-26-28626/poule-169734/",
}

DEFAULT_MONCLUB_FALLBACK_PAGES = {
    "LILLE METROPOLE HB LOMME": "https://monclub.ffhandball.fr/clubs/lomme-lille-metropole-hb/",
}

DEFAULT_MEDIA_BASE_URL = "https://media-logos-clubs.ffhandball.fr"
DEFAULT_USER_AGENT = (
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
    "AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126 Safari/537.36"
)


@dataclass(frozen=True)
class TeamLogoSource:
    season: str
    team_id: str
    external_team_id: str
    poule_id: str
    structure_id: str
    external_structure_id: str
    name: str
    source_logo_filename: str
    source_page_url: str

    @property
    def logo_stem(self) -> str:
        return ".".join(self.source_logo_filename.split(".")[:-1])


class SmartfireComponentParser(HTMLParser):
    def __init__(self) -> None:
        super().__init__()
        self.components: list[dict[str, str]] = []

    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        if tag.lower() != "smartfire-component":
            return

        component: dict[str, str] = {}
        for key, value in attrs:
            if value is not None:
                component[key] = value
        self.components.append(component)


def request_bytes(url: str, timeout: int = 30) -> bytes:
    request = urllib.request.Request(url, headers={"User-Agent": DEFAULT_USER_AGENT})
    with urllib.request.urlopen(request, timeout=timeout) as response:
        return response.read()


def slugify(value: str) -> str:
    normalized = unicodedata.normalize("NFKD", value)
    ascii_value = normalized.encode("ascii", "ignore").decode("ascii")
    ascii_value = ascii_value.lower()
    ascii_value = re.sub(r"[^a-z0-9]+", "-", ascii_value)
    ascii_value = re.sub(r"-{2,}", "-", ascii_value).strip("-")
    return ascii_value or "team"


def team_key(value: str) -> str:
    return re.sub(r"\s+", " ", value.upper()).strip()


def extract_monclub_logo_filename(page_url: str) -> str:
    page_html = request_bytes(page_url).decode("utf-8", errors="replace")
    parser = SmartfireComponentParser()
    parser.feed(page_html)

    for component in parser.components:
        attributes_raw = component.get("attributes", "")
        if "logo_club" not in attributes_raw:
            continue

        attributes = json.loads(html.unescape(attributes_raw))
        candidates = [
            attributes.get("post", {}).get("acf", {}).get("logo_club"),
            attributes.get("club", {}).get("acf", {}).get("logo_club"),
        ]

        for candidate in candidates:
            if candidate:
                return str(candidate).strip()

    raise RuntimeError(f"No MonClub logo found in {page_url}")


def parse_teams_from_page(season: str, page_url: str, html_text: str) -> list[TeamLogoSource]:
    parser = SmartfireComponentParser()
    parser.feed(html_text)

    teams: list[TeamLogoSource] = []
    for component in parser.components:
        block_name = component.get("block") or component.get("name") or ""
        attributes_raw = component.get("attributes", "")

        if block_name != "competitions---calendar-button" or "equipes" not in attributes_raw:
            continue

        attributes = json.loads(html.unescape(attributes_raw))
        for item in attributes.get("equipes") or []:
            logo_filename = str(item.get("logo") or "").strip()
            if not logo_filename:
                continue

            teams.append(
                TeamLogoSource(
                    season=season,
                    team_id=str(item.get("id") or ""),
                    external_team_id=str(item.get("ext_equipeId") or ""),
                    poule_id=str(item.get("pouleId") or ""),
                    structure_id=str(item.get("structureId") or ""),
                    external_structure_id=str(item.get("ext_structureId") or ""),
                    name=str(item.get("libelle") or "").strip(),
                    source_logo_filename=logo_filename,
                    source_page_url=page_url,
                )
            )

    return teams


def parse_teams_without_logo(season: str, page_url: str, html_text: str) -> list[dict[str, Any]]:
    parser = SmartfireComponentParser()
    parser.feed(html_text)

    missing: list[dict[str, Any]] = []
    for component in parser.components:
        block_name = component.get("block") or component.get("name") or ""
        attributes_raw = component.get("attributes", "")

        if block_name != "competitions---calendar-button" or "equipes" not in attributes_raw:
            continue

        attributes = json.loads(html.unescape(attributes_raw))
        for item in attributes.get("equipes") or []:
            logo_filename = str(item.get("logo") or "").strip()
            if logo_filename:
                continue

            missing.append(
                {
                    "season": season,
                    "name": str(item.get("libelle") or "").strip(),
                    "teamId": str(item.get("id") or ""),
                    "externalTeamId": str(item.get("ext_equipeId") or ""),
                    "structureId": str(item.get("structureId") or ""),
                    "externalStructureId": str(item.get("ext_structureId") or ""),
                    "sourcePageUrl": page_url,
                    "reason": "Aucun logo n'est fourni dans les donnees FFHandball pour cette equipe.",
                }
            )

    return missing


def build_logo_urls(media_base_url: str, source: TeamLogoSource, sizes: list[int]) -> list[str]:
    return [
        f"{media_base_url.rstrip('/')}/{size}/{source.logo_stem}.webp"
        for size in sizes
    ]


def download_first_available(urls: list[str]) -> tuple[str, bytes]:
    last_error: Exception | None = None
    for url in urls:
        try:
            return url, request_bytes(url)
        except (urllib.error.HTTPError, urllib.error.URLError, TimeoutError) as error:
            last_error = error

    raise RuntimeError(f"No logo URL could be downloaded. Last error: {last_error}")


def download_logo_with_official_fallbacks(
    media_base_url: str,
    team: TeamLogoSource,
    source_sizes: list[int],
) -> tuple[str, bytes, str, str, str]:
    urls = build_logo_urls(media_base_url, team, source_sizes)
    try:
        source_url, raw_logo = download_first_available(urls)
        return source_url, raw_logo, team.source_logo_filename, team.source_page_url, "competition"
    except RuntimeError as competition_error:
        fallback_page_url = DEFAULT_MONCLUB_FALLBACK_PAGES.get(team_key(team.name))
        if not fallback_page_url:
            raise competition_error

        fallback_logo_filename = extract_monclub_logo_filename(fallback_page_url)
        fallback_source = TeamLogoSource(
            season=team.season,
            team_id=team.team_id,
            external_team_id=team.external_team_id,
            poule_id=team.poule_id,
            structure_id=team.structure_id,
            external_structure_id=team.external_structure_id,
            name=team.name,
            source_logo_filename=fallback_logo_filename,
            source_page_url=fallback_page_url,
        )
        fallback_urls = build_logo_urls(media_base_url, fallback_source, source_sizes)
        source_url, raw_logo = download_first_available(fallback_urls)
        return source_url, raw_logo, fallback_logo_filename, fallback_page_url, "monclub-fallback"


def normalize_logo_png(raw_bytes: bytes, output_path: Path, canvas_size: int) -> dict[str, int | str]:
    with Image.open(io.BytesIO(raw_bytes)) as original:
        image = original.convert("RGBA")
        source_width, source_height = image.size

        image.thumbnail((canvas_size, canvas_size), Image.Resampling.LANCZOS)
        canvas = Image.new("RGBA", (canvas_size, canvas_size), (255, 255, 255, 0))
        offset = ((canvas_size - image.width) // 2, (canvas_size - image.height) // 2)
        canvas.alpha_composite(image, offset)
        canvas.save(output_path, format="PNG", optimize=True)

        return {
            "sourceWidth": source_width,
            "sourceHeight": source_height,
            "outputWidth": canvas_size,
            "outputHeight": canvas_size,
            "outputFormat": "png",
        }


def unique_output_path(season_dir: Path, team: TeamLogoSource) -> Path:
    base_slug = slugify(team.name)
    discriminator = team.external_structure_id or team.structure_id or team.team_id
    filename = f"{base_slug}-{discriminator}.png" if discriminator else f"{base_slug}.png"
    return season_dir / filename


def download_season(
    season: str,
    page_url: str,
    output_root: Path,
    media_base_url: str,
    source_sizes: list[int],
    canvas_size: int,
    overwrite: bool,
    sleep_seconds: float,
) -> tuple[list[dict[str, Any]], list[dict[str, Any]], list[dict[str, Any]]]:
    page_html = request_bytes(page_url).decode("utf-8", errors="replace")
    teams = parse_teams_from_page(season, page_url, page_html)
    missing = parse_teams_without_logo(season, page_url, page_html)

    season_dir = output_root / season
    season_dir.mkdir(parents=True, exist_ok=True)

    downloaded: list[dict[str, Any]] = []
    errors: list[dict[str, Any]] = []

    for index, team in enumerate(teams, start=1):
        output_path = unique_output_path(season_dir, team)
        relative_path = output_path.as_posix()

        urls = build_logo_urls(media_base_url, team, source_sizes)
        source_url = urls[0]
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
            elif output_path.exists():
                with Image.open(output_path) as existing:
                    image_info = {
                        "sourceWidth": "",
                        "sourceHeight": "",
                        "outputWidth": existing.width,
                        "outputHeight": existing.height,
                        "outputFormat": "png",
                    }

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
                    "localPath": relative_path,
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
                    "sourceLogoUrlsTried": urls,
                    "sourcePageUrl": team.source_page_url,
                    "error": str(error),
                }
            )

    return downloaded, missing, errors


def write_json(path: Path, payload: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Download official FFHandball N1F team logos for the app.",
    )
    parser.add_argument(
        "--out",
        default="wwwroot/images/team-logos/n1f",
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
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    output_root = Path(args.out).resolve()
    seasons = args.season or sorted(DEFAULT_SEASON_PAGES)
    source_sizes = args.source_size or [512, 256, 128, 64]

    manifest: dict[str, Any] = {
        "source": "FFHandball",
        "mediaBaseUrl": args.media_base_url,
        "canvasSize": args.canvas_size,
        "sourceSizesTried": source_sizes,
        "seasons": {},
    }
    all_missing: list[dict[str, Any]] = []
    all_errors: list[dict[str, Any]] = []

    for season in seasons:
        page_url = DEFAULT_SEASON_PAGES[season]
        downloaded, missing, errors = download_season(
            season=season,
            page_url=page_url,
            output_root=output_root,
            media_base_url=args.media_base_url,
            source_sizes=source_sizes,
            canvas_size=args.canvas_size,
            overwrite=args.overwrite,
            sleep_seconds=args.sleep,
        )

        manifest["seasons"][season] = {
            "sourcePageUrl": page_url,
            "downloadedCount": len(downloaded),
            "missingLogoCount": len(missing),
            "errorCount": len(errors),
            "teams": downloaded,
        }
        all_missing.extend(missing)
        all_errors.extend(errors)

    write_json(output_root / "n1f-team-logos-manifest.json", manifest)
    missing_path = output_root / "n1f-team-logos-missing.json"
    errors_path = output_root / "n1f-team-logos-errors.json"
    if all_missing:
        write_json(missing_path, all_missing)
    elif missing_path.exists():
        missing_path.unlink()

    if all_errors:
        write_json(errors_path, all_errors)
    elif errors_path.exists():
        errors_path.unlink()

    print(f"Output: {output_root}")
    for season in seasons:
        season_data = manifest["seasons"][season]
        print(
            f"{season}: {season_data['downloadedCount']} logos, "
            f"{season_data['missingLogoCount']} missing, "
            f"{season_data['errorCount']} errors"
        )

    if all_errors:
        print("Some logos failed to download. See n1f-team-logos-errors.json", file=sys.stderr)
        return 1

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
