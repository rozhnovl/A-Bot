#!/usr/bin/env python3
"""
Regenerate module-types.json from EVE Ref reference-data's types.json.

  curl -sL -o reference-data.tar.xz https://data.everef.net/reference-data/reference-data-latest.tar.xz
  python gen-module-types.py reference-data.tar.xz

Only category 7 (Module) is bundled. Runtime operation never depends on the network;
unknown IDs keep working through the numeric fallback in ModuleTypes.NameOrId().
"""
import json
import os
import sys
import tarfile

HERE = os.path.dirname(os.path.abspath(__file__))
TYPES = sys.argv[1] if len(sys.argv) > 1 else os.environ.get("TYPES_JSON", "types.json")
OUT = os.path.join(HERE, "module-types.json")


def load_types():
    if TYPES.lower().endswith((".tar.xz", ".txz")):
        with tarfile.open(TYPES, "r:xz") as archive:
            member = archive.extractfile("types.json")
            if member is None:
                raise FileNotFoundError("types.json is missing from the reference-data archive")
            return json.load(member)
    with open(TYPES, encoding="utf-8") as source:
        return json.load(source)


def main():
    types = load_types()

    modules = {}
    for type_id, item in types.items():
        if item.get("category_id") != 7:
            continue
        name = (item.get("name") or {}).get("en") or ""
        if not name:
            continue
        modules[int(type_id)] = {
            "n": name,
            "g": item.get("group_id") or 0,
            "p": bool(item.get("published")),
        }

    with open(OUT, "w", encoding="utf-8") as output:
        json.dump(modules, output, ensure_ascii=False, separators=(",", ":"), sort_keys=True)
    print(f"wrote {OUT}: {len(modules)} records, {os.path.getsize(OUT) / 1024:.0f} KB")


if __name__ == "__main__":
    main()
