#!/usr/bin/env python3
"""
Download official LFH Ligue Butagaz Energie player portraits and optionally
generate SQL updates for the HandballManager players table.

The LFH endpoint exposes current players across LFH competitions. This script
keeps the configured Ligue Butagaz Energie clubs, normalizes portraits to PNG
assets, and matches them against a MySQL export by player name + team.
"""

from __future__ import annotations

import argparse
import concurrent.futures
import csv
import difflib
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
from pathlib import Path
from typing import Any

from PIL import Image


DEFAULT_ENDPOINT = "https://ligue-feminine-handball.fr/wp-json/lfh/joueuses_staff?page_nb={page}"
DEFAULT_OUTPUT_DIR = "wwwroot/images/player-photos/lbe"
DEFAULT_SQL_OUT = r"D:\repos\HandballManagerAPI\scripts\update_player_photos_lbe.sql"

# LFH club ids mapped to the team names currently used in the HandballManager DB.
LBE_CLUB_TEAM_KEYS: dict[int, str] = {
    3987: "ACHENHEIM",
    2394: "BESANCON",
    1791: "BREST",
    2010: "CHAMBRAY",
    1720: "DIJON",
    2897: "HAVRE",
    1506: "MERIGNAC",
    2662: "METZ",
    2129: "NICE OGC",
    3321: "PARIS92",
    3550: "PLAN-DE-CUQUES",
    2331: "SAMBRE",
    2325: "ST-AMAND",
    3172: "STELLA SAINT-MAUR",
    3920: "TOULON",
}


@dataclass(frozen=True)
class SourcePlayer:
    lfh_id: int
    first_name: str
    last_name: str
    club_id: int
    club_label: str
    team_key: str
    number: str
    position: str
    status: str
    source_url: str
    file_name: str
    web_path: str

    @property
    def db_name_key(self) -> str:
        return normalize_key(f"{self.last_name} {self.first_name}")

    @property
    def display_name(self) -> str:
        return " ".join(part for part in [self.first_name, self.last_name] if part).strip()


@dataclass(frozen=True)
class DbPlayer:
    player_id: int
    name: str
    surname: str
    team_name: str
    position_name: str
    position_code: str
    number: str
    is_active: str

    @property
    def db_name_key(self) -> str:
        return normalize_key(f"{self.name} {self.surname}")

    @property
    def reversed_name_key(self) -> str:
        return normalize_key(f"{self.surname} {self.name}")

    @property
    def team_key(self) -> str:
        return normalize_team_key(self.team_name)

    @property
    def display_name(self) -> str:
        return " ".join(part for part in [self.name, self.surname] if part).strip()


def request_bytes(url: str, *, accept: str = "application/json,image/png,*/*", timeout: int = 45) -> bytes:
    request = urllib.request.Request(
        url,
        headers={
            "User-Agent": "HandWStat LFH player photo downloader/1.0",
            "Accept": accept,
        },
    )

    with urllib.request.urlopen(request, timeout=timeout) as response:
        return response.read()


def request_range_bytes(url: str, byte_range: str = "bytes=0-65535", *, timeout: int = 20) -> bytes:
    request = urllib.request.Request(
        url,
        headers={
            "User-Agent": "HandWStat LFH player photo downloader/1.0",
            "Accept": "image/png,image/jpeg,image/webp,*/*",
            "Range": byte_range,
        },
    )

    with urllib.request.urlopen(request, timeout=timeout) as response:
        return response.read()


def write_json(path: Path, payload: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8")


def strip_accents(value: str) -> str:
    normalized = unicodedata.normalize("NFD", value)
    return "".join(char for char in normalized if unicodedata.category(char) != "Mn")


def clean_text(value: Any) -> str:
    if value is None:
        return ""

    if value is False:
        return ""

    text = html.unescape(str(value)).replace("\u2019", "'").strip()
    try:
        repaired = text.encode("latin-1").decode("utf-8")
    except UnicodeError:
        repaired = text

    original_markers = text.count("Ã") + text.count("Â") + text.count("â€")
    repaired_markers = repaired.count("Ã") + repaired.count("Â") + repaired.count("â€")
    if original_markers > repaired_markers:
        text = repaired

    return " ".join(text.split())


def normalize_key(value: Any) -> str:
    text = strip_accents(clean_text(value)).lower()
    text = text.replace("oe", "oe")
    text = re.sub(r"[^a-z0-9]+", " ", text)
    return " ".join(text.split())


def normalize_compact(value: Any) -> str:
    return normalize_key(value).replace(" ", "")


def normalize_team_key(value: Any) -> str:
    text = normalize_key(value).upper()
    text = text.replace("NICE O G C", "NICE OGC")
    text = text.replace("PARIS 92", "PARIS92")
    text = text.replace("PLAN DE CUQUES", "PLAN-DE-CUQUES")
    text = text.replace("ST AMAND", "ST-AMAND")
    text = text.replace("SAINT AMAND", "ST-AMAND")
    text = text.replace("STELLA ST MAUR", "STELLA SAINT-MAUR")
    text = text.replace("STELLA SAINT MAUR", "STELLA SAINT-MAUR")
    return text


def slugify(value: str) -> str:
    text = normalize_key(value)
    return re.sub(r"[^a-z0-9]+", "-", text).strip("-") or "joueuse"


def sql_string(value: str) -> str:
    return "'" + value.replace("\\", "\\\\").replace("'", "''") + "'"


def make_web_path(output_path: Path) -> str:
    parts = output_path.resolve().parts
    if "wwwroot" not in parts:
        return output_path.as_posix()

    root_index = parts.index("wwwroot")
    return "/" + "/".join(parts[root_index + 1 :])


def normalize_portrait_png(source_bytes: bytes, output_path: Path, canvas_size: int) -> dict[str, Any]:
    source = Image.open(io.BytesIO(source_bytes)).convert("RGBA")
    original_width, original_height = source.size
    image = source.copy()
    image.thumbnail((canvas_size, canvas_size), Image.Resampling.LANCZOS)

    canvas = Image.new("RGBA", (canvas_size, canvas_size), (255, 255, 255, 0))
    x = (canvas_size - image.width) // 2
    y = (canvas_size - image.height) // 2
    canvas.alpha_composite(image, (x, y))

    output_path.parent.mkdir(parents=True, exist_ok=True)
    canvas.save(output_path, "PNG", optimize=True)

    alpha = canvas.getchannel("A")
    return {
        "originalWidth": original_width,
        "originalHeight": original_height,
        "width": canvas.width,
        "height": canvas.height,
        "hasTransparency": alpha.getextrema()[0] < 255,
    }


def read_image_dimensions(url: str) -> tuple[int, int] | None:
    try:
        header_bytes = request_range_bytes(url)
        with Image.open(io.BytesIO(header_bytes)) as image:
            return image.size
    except Exception:  # noqa: BLE001 - falls back to the original image URL.
        return None


def build_scaled_image_urls(url: str) -> list[str]:
    if "." not in url.rsplit("/", 1)[-1]:
        return [url]

    dimensions = read_image_dimensions(url)
    if dimensions is None:
        return [url]

    width, height = dimensions
    root, extension = url.rsplit(".", 1)
    candidates: list[str] = []

    for target_width in (768, 1024, 1536, 512, 300):
        if width <= target_width:
            continue

        target_height = max(1, round(height * target_width / width))
        candidates.append(f"{root}-{target_width}x{target_height}.{extension}")

    candidates.append(url)
    return candidates


def download_best_image(source_url: str) -> tuple[str, bytes]:
    last_error: Exception | None = None

    for candidate in build_scaled_image_urls(source_url):
        try:
            return candidate, request_bytes(candidate, accept="image/png,image/jpeg,image/webp,*/*")
        except urllib.error.HTTPError as error:
            last_error = error
            if error.code in {404, 403}:
                continue
            raise
        except Exception as error:  # noqa: BLE001 - tries the next generated size.
            last_error = error
            continue

    if last_error is not None:
        raise last_error

    raise RuntimeError(f"No downloadable image candidate for {source_url}")


def fetch_source_players(endpoint: str) -> list[dict[str, Any]]:
    all_docs: list[dict[str, Any]] = []
    page = 1
    total_pages = 1

    while page <= total_pages:
        payload = json.loads(
            request_bytes(endpoint.format(page=page), accept="application/json").decode("utf-8")
        )
        docs = payload.get("docs") or []
        if not isinstance(docs, list):
            raise RuntimeError(f"LFH page {page} did not return a docs list.")

        all_docs.extend(doc for doc in docs if isinstance(doc, dict))
        total_pages = int(payload.get("totalPages") or page)
        page += 1

    return all_docs


def build_source_player(doc: dict[str, Any], output_dir: Path) -> SourcePlayer | None:
    if clean_text(doc.get("type")) != "players":
        return None

    club = doc.get("club") if isinstance(doc.get("club"), dict) else {}
    club_id = int(club.get("id") or 0)
    team_key = LBE_CLUB_TEAM_KEYS.get(club_id)
    source_url = clean_text(doc.get("thumbnail"))

    if not team_key or not source_url:
        return None

    lfh_id = int(doc.get("ID") or 0)
    first_name = clean_text(doc.get("firstname"))
    last_name = clean_text(doc.get("lastname"))
    file_name = f"{slugify(last_name)}-{slugify(first_name)}-{lfh_id}.png"

    return SourcePlayer(
        lfh_id=lfh_id,
        first_name=first_name,
        last_name=last_name,
        club_id=club_id,
        club_label=clean_text(club.get("label")),
        team_key=team_key,
        number=clean_text(doc.get("number")),
        position=clean_text(doc.get("position")),
        status=clean_text(doc.get("player_status")),
        source_url=source_url,
        file_name=file_name,
        web_path=make_web_path(output_dir / file_name),
    )


def build_missing_thumbnail_entry(doc: dict[str, Any]) -> dict[str, Any] | None:
    if clean_text(doc.get("type")) != "players":
        return None

    club = doc.get("club") if isinstance(doc.get("club"), dict) else {}
    club_id = int(club.get("id") or 0)
    team_key = LBE_CLUB_TEAM_KEYS.get(club_id)

    if not team_key or clean_text(doc.get("thumbnail")):
        return None

    return {
        "lfhId": int(doc.get("ID") or 0),
        "firstName": clean_text(doc.get("firstname")),
        "lastName": clean_text(doc.get("lastname")),
        "displayName": " ".join(
            part for part in [clean_text(doc.get("firstname")), clean_text(doc.get("lastname"))] if part
        ).strip(),
        "clubId": club_id,
        "clubLabel": clean_text(club.get("label")),
        "teamKey": team_key,
        "number": clean_text(doc.get("number")),
        "position": clean_text(doc.get("position")),
        "status": clean_text(doc.get("player_status")),
    }


def read_db_players(path: Path) -> list[DbPlayer]:
    players: list[DbPlayer] = []

    with path.open("r", encoding="utf-8-sig", newline="") as handle:
        reader = csv.reader(handle, delimiter="\t")
        for row in reader:
            if len(row) < 8:
                continue

            players.append(
                DbPlayer(
                    player_id=int(row[0]),
                    name=clean_text(row[1]),
                    surname=clean_text(row[2]),
                    team_name=clean_text(row[3]),
                    position_name=clean_text(row[4]),
                    position_code=clean_text(row[5]),
                    number=clean_text(row[6]),
                    is_active=clean_text(row[7]),
                )
            )

    return players


def choose_by_number(candidates: list[DbPlayer], source: SourcePlayer) -> list[DbPlayer]:
    if not source.number:
        return candidates

    numbered = [candidate for candidate in candidates if candidate.number == source.number]
    return numbered or candidates


def token_affinity(source_name: str, candidate_name: str) -> tuple[int, float]:
    source_tokens = [token for token in normalize_key(source_name).split() if len(token) >= 3]
    candidate_tokens = [token for token in normalize_key(candidate_name).split() if len(token) >= 3]
    matched = 0

    for source_token in source_tokens:
        if any(
            source_token == candidate_token
            or (
                len(source_token) >= 4
                and len(candidate_token) >= 4
                and difflib.SequenceMatcher(None, source_token, candidate_token).ratio() >= 0.86
            )
            for candidate_token in candidate_tokens
        ):
            matched += 1

    denominator = max(1, min(len(source_tokens), len(candidate_tokens)))
    return matched, matched / denominator


def match_by_team_number_name_tokens(source: SourcePlayer, team_players: list[DbPlayer]) -> DbPlayer | None:
    if not source.number:
        return None

    candidates = [player for player in team_players if player.number == source.number]
    if not candidates:
        return None

    scored = [
        (*token_affinity(source.display_name, candidate.display_name), candidate)
        for candidate in candidates
    ]
    scored.sort(key=lambda item: (item[0], item[1]), reverse=True)
    best_matches = [
        candidate
        for matched_tokens, affinity, candidate in scored
        if matched_tokens >= 2 and affinity >= 0.5
    ]

    if len(best_matches) == 1:
        return best_matches[0]

    return None


def match_source_player(
    source: SourcePlayer,
    db_players: list[DbPlayer],
    *,
    fuzzy_threshold: float,
) -> tuple[DbPlayer | None, str, list[DbPlayer]]:
    team_players = [player for player in db_players if player.team_key == source.team_key]
    exact = [player for player in team_players if player.db_name_key == source.db_name_key]

    if len(exact) == 1:
        return exact[0], "team_name_exact", exact

    exact = choose_by_number(exact, source)
    if len(exact) == 1:
        return exact[0], "team_name_exact_number", exact

    reversed_matches = [player for player in team_players if player.reversed_name_key == source.db_name_key]
    if len(reversed_matches) == 1:
        return reversed_matches[0], "team_reversed_name_exact", reversed_matches

    reversed_matches = choose_by_number(reversed_matches, source)
    if len(reversed_matches) == 1:
        return reversed_matches[0], "team_reversed_name_exact_number", reversed_matches

    token_number_match = match_by_team_number_name_tokens(source, team_players)
    if token_number_match is not None:
        return token_number_match, "team_number_name_tokens", [token_number_match]

    source_compact = normalize_compact(source.db_name_key)
    scored: list[tuple[float, DbPlayer]] = []
    for player in team_players:
        score = difflib.SequenceMatcher(None, source_compact, normalize_compact(player.db_name_key)).ratio()
        reversed_score = difflib.SequenceMatcher(None, source_compact, normalize_compact(player.reversed_name_key)).ratio()
        scored.append((max(score, reversed_score), player))

    scored.sort(key=lambda item: item[0], reverse=True)
    if scored and scored[0][0] >= fuzzy_threshold:
        best_score = scored[0][0]
        near = [player for score, player in scored if score >= best_score - 0.015]
        near = choose_by_number(near, source)
        if len(near) == 1:
            return near[0], f"team_name_fuzzy_{best_score:.3f}", near
        return None, "ambiguous_fuzzy", near

    all_exact = [player for player in db_players if player.db_name_key == source.db_name_key]
    all_exact = choose_by_number(all_exact, source)
    if len(all_exact) == 1:
        return all_exact[0], "global_name_exact_unique", all_exact

    return None, "unmatched", team_players


def build_sql(matches: list[dict[str, Any]]) -> str:
    lines = [
        "-- Generated by scripts/download_lbe_player_photos.py",
        "-- Adds players.Photo and fills it with local LFH portrait file names.",
        "",
        "SET @column_exists := (",
        "    SELECT COUNT(*)",
        "    FROM INFORMATION_SCHEMA.COLUMNS",
        "    WHERE TABLE_SCHEMA = DATABASE()",
        "      AND TABLE_NAME = 'players'",
        "      AND COLUMN_NAME = 'Photo'",
        ");",
        "SET @sql := IF(",
        "    @column_exists = 0,",
        "    'ALTER TABLE `players` ADD COLUMN `Photo` varchar(500) NULL',",
        "    'SELECT ''players.Photo already exists'' AS message'",
        ");",
        "PREPARE stmt FROM @sql;",
        "EXECUTE stmt;",
        "DEALLOCATE PREPARE stmt;",
        "",
    ]

    for match in sorted(matches, key=lambda item: int(item["playerId"])):
        lines.append(
            f"UPDATE `players` SET `Photo` = {sql_string(match['fileName'])} "
            f"WHERE `Id` = {int(match['playerId'])};"
        )

    lines.extend(
        [
            "",
            "SELECT COUNT(*) AS players_total,",
            "       SUM(CASE WHEN `Photo` IS NULL OR `Photo` = '' THEN 1 ELSE 0 END) AS players_without_photo",
            "FROM `players`;",
            "",
        ]
    )
    return "\n".join(lines)


def download_source_player(
    index: int,
    source: SourcePlayer,
    output_dir: Path,
    canvas_size: int,
    overwrite: bool,
) -> tuple[dict[str, Any] | None, dict[str, Any] | None]:
    output_path = output_dir / source.file_name
    image_info: dict[str, Any] = {}

    try:
        if overwrite or not output_path.exists():
            download_url, raw = download_best_image(source.source_url)
            image_info = normalize_portrait_png(raw, output_path, canvas_size)
            image_info["downloadUrl"] = download_url

        return (
            {
                "index": index,
                "lfhId": source.lfh_id,
                "firstName": source.first_name,
                "lastName": source.last_name,
                "displayName": source.display_name,
                "clubId": source.club_id,
                "clubLabel": source.club_label,
                "teamKey": source.team_key,
                "number": source.number,
                "position": source.position,
                "status": source.status,
                "sourceUrl": source.source_url,
                "fileName": source.file_name,
                "localPath": output_path.as_posix(),
                "webPath": source.web_path,
                **image_info,
            },
            None,
        )
    except Exception as error:  # noqa: BLE001 - batch script should keep reporting all failures.
        return (
            None,
            {
                "lfhId": source.lfh_id,
                "displayName": source.display_name,
                "teamKey": source.team_key,
                "sourceUrl": source.source_url,
                "error": str(error),
            },
        )


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Download LFH Ligue Butagaz Energie player portraits and generate DB update SQL.",
    )
    parser.add_argument("--endpoint", default=DEFAULT_ENDPOINT, help="LFH endpoint template with {page}.")
    parser.add_argument("--out", default=DEFAULT_OUTPUT_DIR, help="Output directory for normalized PNG portraits.")
    parser.add_argument("--players-tsv", help="MySQL TSV export used to match portraits to DB players.")
    parser.add_argument("--sql-out", default=DEFAULT_SQL_OUT, help="SQL output path when --players-tsv is provided.")
    parser.add_argument("--canvas-size", type=int, default=512, help="Output PNG canvas size in pixels.")
    parser.add_argument("--overwrite", action="store_true", help="Overwrite existing portrait files.")
    parser.add_argument("--sleep", type=float, default=0.05, help="Delay between portrait downloads, in seconds.")
    parser.add_argument("--workers", type=int, default=8, help="Parallel portrait download workers.")
    parser.add_argument("--fuzzy-threshold", type=float, default=0.94, help="Minimum fuzzy match score.")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    output_dir = Path(args.out).resolve()
    output_dir.mkdir(parents=True, exist_ok=True)

    docs = fetch_source_players(args.endpoint)
    missing_thumbnails = [
        missing
        for doc in docs
        if (missing := build_missing_thumbnail_entry(doc)) is not None
    ]
    source_players = [
        source
        for doc in docs
        if (source := build_source_player(doc, output_dir)) is not None
    ]

    downloaded: list[dict[str, Any]] = []
    errors: list[dict[str, Any]] = []

    with concurrent.futures.ThreadPoolExecutor(max_workers=max(1, args.workers)) as executor:
        future_map = {
            executor.submit(
                download_source_player,
                index,
                source,
                output_dir,
                args.canvas_size,
                args.overwrite,
            ): source
            for index, source in enumerate(source_players, start=1)
        }

        for future in concurrent.futures.as_completed(future_map):
            entry, error = future.result()
            if entry is not None:
                downloaded.append(entry)
            if error is not None:
                errors.append(error)
            if args.sleep:
                time.sleep(args.sleep)

    downloaded.sort(key=lambda item: item["index"])

    manifest = {
        "source": "Ligue Feminine de Handball",
        "endpoint": args.endpoint,
        "canvasSize": args.canvas_size,
        "sourceDocumentCount": len(docs),
        "filteredLbePlayerCount": len(source_players),
        "missingThumbnailCount": len(missing_thumbnails),
        "downloadedCount": len(downloaded),
        "errorCount": len(errors),
        "players": downloaded,
    }
    write_json(output_dir / "lbe-player-photos-manifest.json", manifest)
    write_json(output_dir / "lbe-player-photos-missing-thumbnails.json", missing_thumbnails)

    if errors:
        write_json(output_dir / "lbe-player-photos-errors.json", errors)
    else:
        errors_path = output_dir / "lbe-player-photos-errors.json"
        if errors_path.exists():
            errors_path.unlink()

    if args.players_tsv:
        db_players = read_db_players(Path(args.players_tsv))
        matches: list[dict[str, Any]] = []
        ambiguous: list[dict[str, Any]] = []
        unmatched_sources: list[dict[str, Any]] = []
        matched_db_ids: set[int] = set()

        for source in source_players:
            match, reason, candidates = match_source_player(
                source,
                db_players,
                fuzzy_threshold=args.fuzzy_threshold,
            )
            if match is None:
                payload = {
                    "lfhId": source.lfh_id,
                    "displayName": source.display_name,
                    "teamKey": source.team_key,
                    "number": source.number,
                    "reason": reason,
                    "candidates": [
                        {
                            "playerId": candidate.player_id,
                            "displayName": candidate.display_name,
                            "teamName": candidate.team_name,
                            "number": candidate.number,
                            "position": candidate.position_name,
                        }
                        for candidate in candidates[:10]
                    ],
                }
                if reason.startswith("ambiguous"):
                    ambiguous.append(payload)
                else:
                    unmatched_sources.append(payload)
                continue

            if match.player_id in matched_db_ids:
                ambiguous.append(
                    {
                        "lfhId": source.lfh_id,
                        "displayName": source.display_name,
                        "teamKey": source.team_key,
                        "number": source.number,
                        "reason": "duplicate_db_match",
                        "playerId": match.player_id,
                    }
                )
                continue

            matched_db_ids.add(match.player_id)
            matches.append(
                {
                    "playerId": match.player_id,
                    "dbName": match.display_name,
                    "dbTeam": match.team_name,
                    "dbNumber": match.number,
                    "lfhId": source.lfh_id,
                    "lfhName": source.display_name,
                    "lfhClub": source.club_label,
                    "lfhNumber": source.number,
                    "matchReason": reason,
                    "fileName": source.file_name,
                    "webPath": source.web_path,
                    "sourceUrl": source.source_url,
                }
            )

        unmatched_db = [
            {
                "playerId": player.player_id,
                "displayName": player.display_name,
                "teamName": player.team_name,
                "number": player.number,
                "position": player.position_name,
                "isActive": player.is_active,
            }
            for player in db_players
            if player.player_id not in matched_db_ids
        ]

        write_json(output_dir / "lbe-player-photos-db-matches.json", matches)
        write_json(output_dir / "lbe-player-photos-unmatched-source.json", unmatched_sources)
        write_json(output_dir / "lbe-player-photos-unmatched-db.json", unmatched_db)
        if ambiguous:
            write_json(output_dir / "lbe-player-photos-ambiguous.json", ambiguous)
        else:
            ambiguous_path = output_dir / "lbe-player-photos-ambiguous.json"
            if ambiguous_path.exists():
                ambiguous_path.unlink()

        sql_out = Path(args.sql_out)
        sql_out.parent.mkdir(parents=True, exist_ok=True)
        sql_out.write_text(build_sql(matches), encoding="utf-8")

        print(
            f"DB matching: {len(matches)} matched, {len(unmatched_db)} DB players without photo, "
            f"{len(unmatched_sources)} LFH players unmatched, {len(ambiguous)} ambiguous"
        )
        print(f"SQL: {sql_out}")

    print(f"Output: {output_dir}")
    print(f"LFH LBE players: {len(source_players)} portraits, {len(errors)} errors")

    if errors:
        print("Some portraits failed to download. See lbe-player-photos-errors.json", file=sys.stderr)
        return 1

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
