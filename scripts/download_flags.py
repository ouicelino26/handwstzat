from __future__ import annotations

import argparse
import csv
import io
import json
import re
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path
from typing import Any

from PIL import Image


ISO_COUNTRIES_URL = "https://raw.githubusercontent.com/lukes/ISO-3166-Countries-with-Regional-Codes/master/all/all.json"
FLAGCDN_URL_TEMPLATE = "https://flagcdn.com/w640/{alpha2}.png"

# The database mixes ISO-3166 alpha-3 codes with French/sport codes.
# Values are ISO alpha-2 codes used by FlagCDN.
CODE_OVERRIDES: dict[str, str] = {
    "ALG": "DZ",
    "ANG": "AO",
    "CAM": "CM",
    "CON": "CG",
    "CRO": "HR",
    "DAN": "DK",
    "EGY": "EG",
    "ESP": "ES",
    "GUI": "GN",
    "HON": "HU",
    "KOR": "KR",
    "MAC": "MK",
    "NDL": "NL",
    "POL": "PL",
    "POR": "PT",
    "SER": "RS",
    "SLO": "SI",
    "SUI": "CH",
    "TCH": "CZ",
}

NAME_OVERRIDES: dict[str, str] = {
    "pays bas": "NL",
    "republique tcheque": "CZ",
    "tchequie": "CZ",
    "macedoine du nord": "MK",
    "coree du sud": "KR",
    "korea": "KR",
    "congo": "CG",
    "republique democratique du congo": "CD",
    "rdc": "CD",
}


def repair_mojibake(value: str | None) -> str:
    if value is None:
        return ""

    text = value.strip()
    if not text or text.upper() == "NULL":
        return ""

    try:
        repaired = text.encode("latin-1").decode("utf-8")
    except UnicodeError:
        return text

    original_markers = text.count("Ã") + text.count("Â")
    repaired_markers = repaired.count("Ã") + repaired.count("Â")
    return repaired if original_markers > repaired_markers else text


def strip_accents(value: str) -> str:
    import unicodedata

    normalized = unicodedata.normalize("NFD", value)
    return "".join(char for char in normalized if unicodedata.category(char) != "Mn")


def normalize_key(value: str | None) -> str:
    text = strip_accents(repair_mojibake(value)).lower()
    text = re.sub(r"[^a-z0-9]+", " ", text)
    return " ".join(text.split())


def safe_file_name(value: str) -> str:
    text = repair_mojibake(value)
    text = re.sub(r'[<>:"/\\|?*\x00-\x1F]', " ", text)
    text = " ".join(text.split()).strip(" .")
    return text or "flag.png"


def request_bytes(url: str, *, timeout: int = 30) -> bytes:
    request = urllib.request.Request(
        url,
        headers={
            "User-Agent": "HandWStat flag asset downloader/1.0",
            "Accept": "application/json,image/png,*/*",
        },
    )

    with urllib.request.urlopen(request, timeout=timeout) as response:
        return response.read()


def load_country_map() -> tuple[dict[str, str], dict[str, str]]:
    payload = json.loads(request_bytes(ISO_COUNTRIES_URL).decode("utf-8"))
    if not isinstance(payload, list):
        raise RuntimeError("ISO country source did not return a country list.")

    by_code: dict[str, str] = {}
    by_name: dict[str, str] = {}

    for country in payload:
        if not isinstance(country, dict):
            continue

        alpha2 = str(country.get("alpha-2") or "").upper()
        alpha3 = str(country.get("alpha-3") or "").upper()

        if not alpha2:
            continue

        if alpha3:
            by_code[alpha3] = alpha2

        names = [country.get("name")]

        for name in names:
            key = normalize_key(name)
            if key:
                by_name[key] = alpha2

    by_code.update(CODE_OVERRIDES)
    by_name.update(NAME_OVERRIDES)

    return by_code, by_name


def resolve_alpha2(row: dict[str, str], by_code: dict[str, str], by_name: dict[str, str]) -> str | None:
    code = repair_mojibake(row.get("Code")).upper()
    if code in by_code:
        return by_code[code]

    for candidate in (row.get("Country"), row.get("NationalityF"), row.get("NationalityM"), row.get("Pictures")):
        key = normalize_key(candidate)
        if key.endswith(" png"):
            key = key[:-4].strip()

        if key in by_name:
            return by_name[key]

    return None


def output_file_name(row: dict[str, str]) -> str:
    picture = repair_mojibake(row.get("Pictures"))
    country = repair_mojibake(row.get("Country"))

    file_name = picture if picture else f"{country}.png"
    file_name = safe_file_name(file_name)

    if not Path(file_name).suffix:
        file_name = f"{file_name}.png"

    if Path(file_name).suffix.lower() != ".png":
        file_name = f"{Path(file_name).stem}.png"

    return file_name


def make_transparent_asset(source_bytes: bytes, output_path: Path, size: tuple[int, int], padding: int) -> None:
    source = Image.open(io.BytesIO(source_bytes)).convert("RGBA")

    canvas_width, canvas_height = size
    available_width = max(canvas_width - (padding * 2), 1)
    available_height = max(canvas_height - (padding * 2), 1)

    source.thumbnail((available_width, available_height), Image.Resampling.LANCZOS)

    canvas = Image.new("RGBA", size, (255, 255, 255, 0))
    x = (canvas_width - source.width) // 2
    y = (canvas_height - source.height) // 2
    canvas.alpha_composite(source, (x, y))
    canvas.save(output_path, "PNG", optimize=True)


def read_rows(csv_path: Path) -> list[dict[str, str]]:
    with csv_path.open("r", encoding="utf-8-sig", newline="") as handle:
        return list(csv.DictReader(handle, delimiter=";"))


def main() -> int:
    parser = argparse.ArgumentParser(description="Download transparent PNG flag assets for HandWStat.")
    parser.add_argument("--csv", default=r"C:\Users\donov\nationalites_202606232219.csv", help="Nationalities CSV path.")
    parser.add_argument("--out", default=r"wwwroot\images\flags", help="Output directory inside the project.")
    parser.add_argument("--width", type=int, default=512, help="PNG canvas width.")
    parser.add_argument("--height", type=int, default=384, help="PNG canvas height.")
    parser.add_argument("--padding", type=int, default=32, help="Transparent padding around each flag.")
    parser.add_argument("--overwrite", action="store_true", help="Overwrite existing flag PNG files.")
    args = parser.parse_args()

    csv_path = Path(args.csv)
    output_dir = Path(args.out)
    output_dir.mkdir(parents=True, exist_ok=True)

    rows = read_rows(csv_path)
    by_code, by_name = load_country_map()

    manifest: list[dict[str, Any]] = []
    unresolved: list[dict[str, str]] = []
    downloaded = 0
    skipped = 0

    for row in rows:
        alpha2 = resolve_alpha2(row, by_code, by_name)
        file_name = output_file_name(row)
        output_path = output_dir / file_name

        entry = {
            "id": repair_mojibake(row.get("Id")),
            "country": repair_mojibake(row.get("Country")),
            "code": repair_mojibake(row.get("Code")),
            "nationalityF": repair_mojibake(row.get("NationalityF")),
            "nationalityM": repair_mojibake(row.get("NationalityM")),
            "alpha2": alpha2,
            "fileName": file_name,
            "relativePath": f"images/flags/{urllib.parse.quote(file_name)}",
            "source": FLAGCDN_URL_TEMPLATE.format(alpha2=alpha2.lower()) if alpha2 else None,
        }
        manifest.append(entry)

        if not alpha2:
            unresolved.append(entry)
            continue

        if output_path.exists() and not args.overwrite:
            skipped += 1
            continue

        try:
            source = request_bytes(FLAGCDN_URL_TEMPLATE.format(alpha2=alpha2.lower()))
            make_transparent_asset(source, output_path, (args.width, args.height), args.padding)
            downloaded += 1
            time.sleep(0.04)
        except (urllib.error.URLError, OSError, ValueError) as exc:
            entry["error"] = str(exc)
            unresolved.append(entry)

    (output_dir / "flags-manifest.json").write_text(
        json.dumps(manifest, ensure_ascii=False, indent=2),
        encoding="utf-8",
    )

    if unresolved:
        (output_dir / "flags-unresolved.json").write_text(
            json.dumps(unresolved, ensure_ascii=False, indent=2),
            encoding="utf-8",
        )

    print(f"Rows: {len(rows)}")
    print(f"Downloaded: {downloaded}")
    print(f"Skipped existing: {skipped}")
    print(f"Unresolved/errors: {len(unresolved)}")
    print(f"Output: {output_dir.resolve()}")

    return 1 if unresolved else 0


if __name__ == "__main__":
    sys.exit(main())
