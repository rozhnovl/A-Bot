# Stream every DiscordChatExporter JSON in this dir and pull out SIGNAL:
#   - fits      : messages that look like an EFT fit block or link to a fit site
#   - pinned    : moderator-curated pins (highest trust)
#   - reacted   : messages the community up-voted (reactions)
#   - tactics   : messages matching NPC / room / behaviour keywords
# Output: compact JSONL per bucket (id, channel, author, ts, reacts, url, content).
# Content is trimmed; fits are kept whole. Junk (one-liners, no signal) is dropped.

import ijson, json, glob, os, re

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "distilled")
os.makedirs(OUT, exist_ok=True)

# --- EFT / fit detection ------------------------------------------------------
SHIPS = (
    "gila|vexor|ishtar|caracal|cerberus|worm|hawk|sacrilege|deimos|eagle|"
    "stabber|bellicose|osprey|stormbringer|orthrus|jackdaw|hecate|sunesis|"
    "cyclone|ferox|drake|nighthawk|barghest|rattlesnake|golem|leshak|"
    "vagabond|muninn|nergal|kikimora|retribution|coercer|dragoon|algos|"
    "gnosis|praxis|phantasm|cynabal|ashimmu|vindicator|loki|tengu"
)
# EFT header:  [Ship, Name of fit]
RE_EFT_HEADER = re.compile(r"\[\s*(" + SHIPS + r")\s*,", re.I)
RE_FITLINK = re.compile(r"(abysstracker\.com/fit/|osmium\.co|o\.smium|caldarijoans|streamlit\.app)", re.I)
# a code block that contains several module-ish lines
RE_CODEBLOCK = re.compile(r"```")

# --- tactics keywords ---------------------------------------------------------
TACTICS = re.compile(
    r"\b(orbit|kiting|kite|keep at range|transvers|angular|web|scram|point|"
    r"neut|neutraliz|track disrupt|tracking|damp|paint|target paint|jam|ecm|"
    r"aggro|threat|primary|focus fire|split|drone(s)? (die|dying|lost|aggro|pull)|"
    r"cap stable|capacitor|overheat|heat|reload|align|warp out|bail|"
    r"spawn|wave|room|trigger|neut tower|damavik|tessella|kikimora|leshak|"
    r"vila|karybdis|renewing|ambrogius|drifter|overmind|lucid|blenny|"
    r"marshal|striking|deviant|automata|tessera|vedmak|starving|"
    r"resist|resistance|omni|damage type|kinetic|thermal|emp|explosive)\b",
    re.I,
)

MAXLEN = 4000  # keep fits whole-ish, trim rants

def react_count(m):
    rs = m.get("reactions") or []
    n = 0
    emojis = []
    for r in rs:
        c = r.get("count", 0) or 0
        n += c
        e = (r.get("emoji") or {}).get("name") or (r.get("emoji") or {}).get("code") or "?"
        emojis.append(f"{e}x{c}")
    return n, emojis

def looks_like_fit(content):
    if RE_EFT_HEADER.search(content):
        return True
    if RE_FITLINK.search(content):
        return True
    # a code block with several lines that mention modules/ammo
    if RE_CODEBLOCK.search(content) and content.count("\n") >= 5:
        return True
    return False

def main():
    files = sorted(glob.glob(os.path.join(HERE, "*.json")))
    buckets = {k: open(os.path.join(OUT, k + ".jsonl"), "w", encoding="utf-8")
               for k in ("fits", "pinned", "reacted", "tactics")}
    stats = {}
    for path in files:
        chan = os.path.basename(path)
        m = re.search(r"- ([^\[]+)\[", chan)
        chan = m.group(1).strip() if m else chan
        counts = dict(total=0, fits=0, pinned=0, reacted=0, tactics=0)
        with open(path, "rb") as f:
            for msg in ijson.items(f, "messages.item"):
                counts["total"] += 1
                content = msg.get("content") or ""
                author = (msg.get("author") or {}).get("name") or "?"
                ts = (msg.get("timestamp") or "")[:10]
                nreact, emojis = react_count(msg)
                # collect embed/attachment fit-ish urls
                urls = []
                for a in (msg.get("attachments") or []):
                    u = a.get("url")
                    if u: urls.append(u)
                for e in (msg.get("embeds") or []):
                    u = e.get("url")
                    if u: urls.append(u)

                rec = dict(id=msg.get("id"), ch=chan, who=author, ts=ts,
                           r=nreact, em=emojis[:6],
                           u=[x for x in urls if RE_FITLINK.search(x)][:4],
                           c=content[:MAXLEN])

                is_fit = looks_like_fit(content)
                # route to buckets (a message can land in several)
                if msg.get("isPinned"):
                    buckets["pinned"].write(json.dumps(rec, ensure_ascii=False) + "\n")
                    counts["pinned"] += 1
                if is_fit:
                    buckets["fits"].write(json.dumps(rec, ensure_ascii=False) + "\n")
                    counts["fits"] += 1
                if nreact >= 3 and len(content) >= 40:
                    buckets["reacted"].write(json.dumps(rec, ensure_ascii=False) + "\n")
                    counts["reacted"] += 1
                if not is_fit and len(content) >= 60 and TACTICS.search(content):
                    # only keep tactics msgs with some substance or up-votes
                    if len(content) >= 120 or nreact >= 1:
                        buckets["tactics"].write(json.dumps(rec, ensure_ascii=False) + "\n")
                        counts["tactics"] += 1
        stats[chan] = counts
        print(f"{chan:45s} total={counts['total']:6d} fits={counts['fits']:4d} "
              f"pin={counts['pinned']:3d} react={counts['reacted']:4d} tac={counts['tactics']:5d}")
    for b in buckets.values():
        b.close()
    with open(os.path.join(OUT, "_stats.json"), "w") as f:
        json.dump(stats, f, indent=2)
    # sizes
    print("--- distilled sizes ---")
    for k in ("fits", "pinned", "reacted", "tactics"):
        p = os.path.join(OUT, k + ".jsonl")
        print(f"  {k:10s} {os.path.getsize(p)//1024:6d} KB")

if __name__ == "__main__":
    main()
