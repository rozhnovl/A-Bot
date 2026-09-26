#!/usr/bin/env python3
"""
Regenerate npc-stats.json (the bundled NPC combat-stat DB) from the EVE Static Data Export.

Source: EVE Ref reference-data (SDE + ESI merged, ESI-shaped JSON).
  1. Download:  curl -sL -o refdata.tar.xz https://data.everef.net/reference-data/reference-data-latest.tar.xz
  2. Extract:   tar -xJf refdata.tar.xz types.json
  3. Run:       python gen-npc-stats.py   (writes npc-stats.json next to it)

What it does: for every NPC type (category 11 Entity) plus the Deviant Automata hazard
structures (category 2, group 1981 = Suppressors / Tracking Pylons), it reads the dogma
attributes and precomputes:
  - raw HP and per-layer damage resonances, plus compatibility EHP summaries
    (applied damage = raw damage * resonance; lower resonance = more resist)
  - base weapon DPS = sum(damage components) * damageMultiplier / rateOfFire  (UNRAMPED)
  - ramp flag (Triglavian disintegrators climb the longer they fire one target)
  - EWAR flags from behaviour/entity dogma attrs: web / scram / neut / rep / damp / td / paint / ecm
  - vmax, sig
Keyed by the exact English type name, which is what the in-game overview shows, so an
overview enemy resolves 1:1 (the EWAR-prefix variants — Tangling/Starving/Renewing/... — are
distinct typeIDs with the prefix baked into the name). NOTE: a few Drifter abyssal bosses
(Karybdis/Scylla Tyrannos) are shown under a display name that differs from the SDE name and
won't resolve here; the bot special-cases those by name.
"""
import json, os

HERE = os.path.dirname(os.path.abspath(__file__))
TYPES = os.environ.get("TYPES_JSON", "types.json")
OUT = os.path.join(HERE, "npc-stats.json")

A_SHIELD, A_ARMOR, A_HULL = 263, 265, 9
RES = {'s': {'em': 271, 'th': 274, 'kin': 273, 'exp': 272},
       'a': {'em': 267, 'th': 270, 'kin': 269, 'exp': 268},
       'h': {'em': 113, 'th': 110, 'kin': 109, 'exp': 111}}
DMG = {'em': 114, 'th': 118, 'kin': 117, 'exp': 116}
A_DMGMULT, A_TURRET_ROF, A_MISSILE_ROF, A_MISSILE_MULT = 64, 51, 506, 212
A_VMAX, A_SIG, A_RAMP_PER = 37, 552, 2733
EWAR = {'web': [968, 2499, 2735, 2747], 'scram': [504, 103, 2503, 2506, 2509],
        'neut': [97, 2519, 2630, 2522], 'rep': [630, 636, 1454, 1458, 2491, 2495, 2725, 2633],
        'damp': [943, 2527], 'td': [944, 2515, 2511], 'paint': [945, 2523], 'ecm': [929, 2822, 2531, 1658]}


def attrs_of(t):
    return {int(k): v['value'] for k, v in (t.get('dogma_attributes') or {}).items()}


def resl(d, L):
    return {k: d.get(v, 1.0) for k, v in RES[L].items()}


def ehp_vs(hp, res, dt):
    return round(sum(h / res[L].get(dt, 1.0) for L, h in hp.items() if h > 0 and res[L].get(dt, 1.0) > 0))


def ehp_omni(hp, res):
    tot = 0.0
    for L, h in hp.items():
        rs = list(res[L].values())
        m = sum(rs) / len(rs) if rs else 1.0
        if h > 0 and m > 0:
            tot += h / m
    return round(tot)


def want(t):
    c = t.get('category_id')
    return c == 11 or (c == 2 and t.get('group_id') == 1981)


def main():
    types = json.load(open(TYPES, encoding='utf-8'))
    out = {}
    for tid, t in types.items():
        if not want(t):
            continue
        name = (t.get('name') or {}).get('en') or ''
        if not name:
            continue
        d = attrs_of(t)
        hp = {'s': d.get(A_SHIELD, 0.0), 'a': d.get(A_ARMOR, 0.0), 'h': d.get(A_HULL, 0.0)}
        res = {L: resl(d, L) for L in ('s', 'a', 'h')}
        dsum = sum(d.get(v, 0.0) for v in DMG.values())
        mult = d.get(A_DMGMULT, 1.0) or 1.0
        mmult = d.get(A_MISSILE_MULT, 1.0) or 1.0
        rof = d.get(A_TURRET_ROF) or d.get(A_MISSILE_ROF) or 0.0
        dps, dmgtype = 0.0, None
        if dsum > 0 and rof > 0:
            dps = dsum * mult * mmult / (rof / 1000.0)
            comp = {k: d.get(v, 0.0) for k, v in DMG.items()}
            dmgtype = max(comp, key=comp.get) if any(comp.values()) else None
        ew = [k for k, ids in EWAR.items() if any(i in d for i in ids)]
        rec = {'id': int(tid), 'g': t.get('group_id'), 'cat': t.get('category_id')}
        if sum(hp.values()) > 0:
            rec['hp'] = {k: round(v) for k, v in hp.items() if v}
            # Keep the layer resonances instead of throwing them away after calculating the
            # compatibility EHP fields.  A mixed-damage weapon (lasers in particular) needs these
            # values to estimate how many volleys its *current crystal* needs for this NPC.
            rec['res'] = {
                L: {k: round(v, 6) for k, v in res[L].items()}
                for L in ('s', 'a', 'h') if hp[L] > 0
            }
            rec['ehpKin'] = ehp_vs(hp, res, 'kin')
            rec['ehpTh'] = ehp_vs(hp, res, 'th')
            rec['ehpOmni'] = ehp_omni(hp, res)
        if dps > 0:
            rec['dps'] = round(dps, 1)
            if dmgtype:
                rec['dmg'] = dmgtype
            if A_RAMP_PER in d:
                rec['ramp'] = True
        if d.get(A_VMAX):
            rec['vmax'] = round(d[A_VMAX])
        if d.get(A_SIG):
            rec['sig'] = round(d[A_SIG])
        if ew:
            rec['ewar'] = ew
        out[name] = rec
    json.dump(out, open(OUT, 'w', encoding='utf-8'), ensure_ascii=False, separators=(',', ':'))
    print(f"wrote {OUT}: {len(out)} records, {os.path.getsize(OUT)/1024:.0f} KB")


if __name__ == '__main__':
    main()
