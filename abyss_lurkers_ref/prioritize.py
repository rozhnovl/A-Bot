# Second pass: shrink the distilled buckets to high-signal, agent-sized chunks.
#  - fits    : dedup by (hull, normalized module set); keep unique EFT blocks only.
#  - tactics : keep only expert authors OR reacted>=2 OR long writeups (>=350 chars).
# Then split each into ~equal chunk files under distilled/chunks/ for haiku agents.

import json, os, re, glob

HERE = os.path.dirname(os.path.abspath(__file__))
D = os.path.join(HERE, "distilled")
C = os.path.join(D, "chunks")
os.makedirs(C, exist_ok=True)
for f in glob.glob(os.path.join(C, "*")):
    os.remove(f)

EXPERTS = {  # known fit/guide authors in this community
    "caldarijoans", "hyperion9919", "protomolecular", "nomirre", "lobachevsky",
    "torvalduruz", "gustavmannfred", "daaducktator", "morbihan", "johnghettot3",
    "tahiro", "arak", "atlaskitsun",
}

RE_EFT = re.compile(r"\[\s*([A-Za-z][A-Za-z ]+?)\s*,")

def norm_modules(content):
    # pull the hull + a normalized signature of module lines for dedup
    hull = None
    mh = RE_EFT.search(content)
    if mh: hull = mh.group(1).strip().lower()
    lines = []
    for ln in content.splitlines():
        ln = ln.strip().strip("`").strip()
        if not ln or ln.startswith("[") or ln.startswith("**") or ln.startswith(">"):
            continue
        # drop ammo after comma, drop counts
        base = ln.split(",")[0].strip().lower()
        base = re.sub(r"\s+x?\d+$", "", base)
        if len(base) > 3 and not base.startswith("http"):
            lines.append(base)
    sig = (hull or "?", tuple(sorted(set(lines))[:20]))
    return hull, sig

def chunkify(records, prefix, per):
    for i in range(0, len(records), per):
        part = records[i:i+per]
        p = os.path.join(C, f"{prefix}_{i//per:02d}.jsonl")
        with open(p, "w", encoding="utf-8") as f:
            for r in part:
                f.write(json.dumps(r, ensure_ascii=False) + "\n")
        yield p

# --- fits: dedup ---
seen = set()
fits = []
for line in open(os.path.join(D, "fits.jsonl"), encoding="utf-8"):
    m = json.loads(line)
    c = m["c"]
    if "[" not in c:  # link-only fit, keep if has fit url
        if m.get("u"):
            fits.append(m)
        continue
    hull, sig = norm_modules(c)
    if not hull or len(sig[1]) < 4:  # not a real fit block
        continue
    if sig in seen:
        continue
    seen.add(sig)
    fits.append(m)
print(f"fits: {len(fits)} unique (from 4635)")

# --- tactics: expert / reacted / long ---
tac = []
for line in open(os.path.join(D, "tactics.jsonl"), encoding="utf-8"):
    m = json.loads(line)
    who = (m.get("who") or "").lower().replace(" ", "").replace("_", "").replace(".", "")
    keep = (any(e in who for e in EXPERTS)
            or m.get("r", 0) >= 2
            or len(m.get("c", "")) >= 350)
    if keep:
        tac.append(m)
print(f"tactics: {len(tac)} high-signal (from 27455)")

# reacted stays whole (already small)
reacted = [json.loads(l) for l in open(os.path.join(D, "reacted.jsonl"), encoding="utf-8")]

fit_chunks = list(chunkify(fits, "fits", 700))
tac_chunks = list(chunkify(tac, "tac", 900))
rea_chunks = list(chunkify(reacted, "rea", 600))
print("fit chunks :", len(fit_chunks))
print("tac chunks :", len(tac_chunks))
print("rea chunks :", len(rea_chunks))
for p in fit_chunks + tac_chunks + rea_chunks:
    print(f"  {os.path.basename(p):16s} {os.path.getsize(p)//1024:5d} KB")
