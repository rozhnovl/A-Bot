# Abyss Intel — distilled from the "Abyssal Lurkers" Discord

Source: a ~1 GB DiscordChatExporter dump of 10 channels (878k messages, 2021–2025) mined
on 2026-08-24. Extraction pipeline: `extract.py` (stream → fits/pinned/reacted/tactics
buckets) → `prioritize.py` (dedup fits, expert/reacted/long tactics) → 19 haiku subagents
(strict bot-actionable schema) → this synthesis. Regenerate with those scripts.

This is the **knowledge base for the bot's combat brain** — target priority, damage-type
selection, positioning, and bail logic — plus reference fits. Statements below are the
*consensus* across many independent extractions; single-source oddities were dropped.

See also: [HAWK-TRIPLEBOX-GUIDE.md](HAWK-TRIPLEBOX-GUIDE.md) — throwaway-Hawk triplebox
guide for T4–T6 Dark abyssals (aggro split, per-spawn tactics); the reference behaviour
for the 3x Hawk spider-tank FleetBrain scenario.

---

## 1. NPC EWAR — the variant-prefix rule (HIGH CONFIDENCE)

Triglavian rats encode their EWAR in the **name prefix**, confirmed 1:1 against SDE dogma in
`npc-stats.json`. This is the single most useful machine-readable fact in the whole dump:

| Prefix        | EWAR applied            | Notes |
|---------------|-------------------------|-------|
| **Starving**  | energy **neut**         | cap drain — kills your tank; top priority |
| **Anchoring** | **scram**               | pins your prop-mod, blocks kite/escape |
| **Tangling**  | **web**                 | enables enemy DPS, catches your drones |
| **Ghosting**  | **tracking disruptor**  | wrecks your turret application (low threat to missiles) |
| **Blinding**  | sensor **damp**         | cuts your lock range; also blocks drone re-lock |
| **Harrowing** | **target paint**        | boosts incoming application |
| **Renewing**  | heavy **remote-rep**    | spider-tank healer — break it to kill the room |
| **Striking / Liminal / Sudenic** | none (raw DPS) | just damage; the ramping disintegrator is the threat |

Almost **every** Trig also carries incidental `rep` (they cross-repair) — so do NOT treat the
generic `rep` flag as "healer". Only the dedicated healers (Renewing, Rodiva, Lucid
Preserver/Firewatcher/Deepwatcher) are worth focusing to break regen.

### Threat archetypes (how to fight each, consensus)
- **Damavik** (frigate): the EWAR carrier. Kill by prefix priority. Vila Damavik = EWAR only, ~no damage.
- **Vedmak** (cruiser): high ramping DPS; Starving neuts. Kills before spool caps. Orbits ~18–20 km.
- **Leshak** (battleship): gun range ~63 km; fully spooled ≈ huge DPS; spool does NOT transfer between
  targets and resets if you break its orbit / use a speed cloud. Starving neuts, Blinding damps.
  **Stay outside 63 km or brawl under it.** Multiple Leshaks = leave.
- **Kikimora** (destroyer): ramping DPS, no EWAR itself but rooms pair it with neut/web Damaviks.
  Chases at max velocity then orbits slowly → **kiteable at 20 km** (slung outside ~15 km rep range).
  Kill Kikis fast; they're the DPS.
- **Karybdis Tyrannos "Karen" / Abyssal Overmind** (drone BS): very high EHP, wants a ~60 km orbit,
  poor tracking. **Brawl at 500 m** (angular tank) OR out-range; gains a ~60% web at T5+. Secondary
  threat — kill its support first, it last.
- **Marshal** (EDENCOM BS): missile damage, ~65–70 km. Speed-tank / kite; kill its web+paint frigate
  support first or the BS applies perfectly. Avoid blue clouds. A "Captain/Attacker" warp-in is a
  known death-spike.
- **Tessera / Tessella** (rogue-drone): perfect tracking, you cannot get under its guns → **kite at max
  ammo range**. Deletes light drones with wrecking shots (one-shots a Worm's drone if unlucky). Sub-drone
  damage type is in the name: Spark=EM, Blast=explosive, Strike=kinetic, Ember=thermal.
- **Cynabal (Angel)**: fast, tracks well; **snap to 500 m orbit** and burn tank / focus-fire until all
  down. Elite Cynabals in groups of 4+ = bail check.
- **Devoted Knight/Hunter (Sansha)**: slow brawlers; 500 m orbit. Hunters have tracking-disrupt + high
  tracking → keep-at-range in your optimal.

---

## 2. TARGET PRIORITY (consensus kill order) — encoded in the bot

Clear whatever cripples **your** ship first, in the order it hurts most, then break healers, then race
the ramping damage before it spools:

1. **Neut** (Starving, Ephialtes Dissipator) — cap loss turns off your tank/prop.
   *Caveat:* skip if you're genuinely cap-stable (crystal-Gila with a single small rep).
2. **Scram** (Anchoring) — you can't kite or bail while pointed.
3. **Web** (Tangling, Snarecaster, Entangler) + **jam/ECM** — enables enemy DPS & catches drones.
4. **Application denial**: damp (Blinding) / tracking-disrupt (Ghosting) / paint (Harrowing).
5. **Dedicated remote-rep** (Renewing, Rodiva, Lucid Preserver/Deepwatcher) — break the spider-tank.
6. **Plain damage dealers** — highest applied DPS first, **ramping rats first** (they only get worse),
   lower-EHP breaks ties (faster kill).
7. **Never**: the loot cache (Bioadaptive/Biocombinative), Extraction nodes; **avoid** Drifter BS.

> Implemented in `NpcInfoProvider.CalcTargetPriority` (bands 1000 apart, driven by the `npc-stats.json`
> ewar flags + name prefixes). This replaced the old ad-hoc name list.

Because the EWAR carriers are almost always frigates, "EWAR-first" naturally realizes the community's
other rule of thumb — **frigs before cruisers before battleships** — without special-casing hull class.

---

## 3. DAMAGE TYPE / AMMO by weather × faction

Your Worm/Cerberus are **kinetic-locked** (RLML/HAM Scourge) — no swap. Flexible boats (Muninn,
Sacrilege, turret ships, drone boats) should swap by the target's resist hole:

| Enemy faction | Default | Exceptions |
|---------------|---------|------------|
| **Triglavian** (Vedmak/Leshak/Damavik/Kiki) | **Nova** (explosive) — weakest resist, pulls ahead in Dark | kinetic acceptable at low tier |
| **Sansha** (Knights/Overmind shield) | **Mjolnir** (EM) | — |
| **EDENCOM** (Marshal/Skybreaker) | **Inferno** (thermal) | Nova on some armor NPCs |
| **Angel** | EM/thermal mix | don't drop DPS for a dedicated EM hardener |
| **Rogue drones / Concord** | match the Tessera sub-drone's own damage-type hole | — |

**Weather resist hole** (drones/ammo to load): Exotic → **kinetic** (Caldari Navy/Augmented Vespa);
Electrical → **EM** (Infiltrator/Mjolnir); Gamma → **explosive** (Berserker/Nova); Firestorm →
**thermal** (Hammerhead/Inferno, but the ship is fragile — thermal-tank check first); **Dark → missiles
win** (−50% turret & drone range + enemy speed boost; avoid drone/turret boats in Dark).

Overmind phase-swap (flexible boats): Mjolnir on shield → Nova on armor → Scourge on structure.

---

## 4. POSITIONING / RANGE / PROP discipline

Three engagement archetypes — pick by the primary's type:

- **Brawl 500 m orbit** (angular tank): Karybdis/Karen, Overmind, Cynabal, Knight, Drekavac, Leshak
  under its guns. Spiral in at an angle, accept a few hits settling the orbit, then it holds.
- **Kite ~20 km**: Kikimora (slung outside rep range), Vedmak (~18–20), Tessera (**max ammo range**,
  never closer — perfect tracking), Sansha/Angel frigate rooms.
- **Stand off 60 km+ / avoid**: Leshak (outside 63 km gun range), Marshal (~65–70 km missiles),
  Drifter BS (don't engage).

**Web**: single web outside Dark, **double web inside Dark** (enemies get a speed boost in Dark; a web
drops a Damavik from ~1450 m/s to ~580 with one, ~277 with two). Web range (~15 km) ≈ HAM range — fight
inside it. Rage ammo needs web support to apply.

**NPC orbit-velocity is capped (~1450 m/s)** in the abyss — that's why webs are so strong and why you
can out-run/kite ramping rats to reset their spool.

**MWD/AB discipline**: the fits are NOT cap-stable — **don't hold the prop**. Turn MWD **off** when
webbed or at point-blank (sig bloom makes you easier to hit and eats cap); pulse it only to close a big
gap or reset a spool. AB preferred on Gila (no sig bloom). Under heavy neut, drop the prop and rep-pulse.

---

## 5. TIMER & BAIL

- **Room timer ≈ 6:40** (room 1 starts with 60 s of tunnel, rooms 2–3 with 30 s). A room not clearing
  by ~6:10–6:40 is a functional loss. Full-clear benchmark to progress a tier: **~5 min gate-to-gate**.
- **Death combos** (bail on sight — the gate is open, just leave): 3× Overmind in ≤50% weather
  (mathematically un-clearable without top DPS); multiple Starving Leshaks / a Leshak "shipyard" room;
  4+ Elite Cynabals; a Karen + heavy neut/web + blue cloud; a mass Starving-Damavik + Starving-Vedmak
  neut wave with no cap buffer. Karybdis double-wrecking-shot can alpha a Cerberus (rare).
- **Cap rule**: enter each room at ~70% cap; don't perma-run boosters — pulse/feather them; heat the
  **hardener first** (most cap-efficient), then prop, then guns. Cap buffer > cap-stable-on-paper,
  because a neut wave ignores paper stability.

---

## 6. MODULE / HEAT / DRONE micro (bot-actionable)

- **Launch drones before breaking the gate iframe** so they take initial aggro, not you; keep weapons
  cycling — one RLML/HAM reload mid-room costs ~20 s.
- **Overheat**: guns near-100% uptime on priority targets; hardener proactively in high-DPS rooms;
  repair overheat during the gate-transition animation.
- **Drone preservation** (the operator's rule): bracket colour = who they shoot — **red = shooting you,
  yellow = shooting your drones** (in a solo run), no box = full drone aggro. Pull drones from Tessera
  (one-shots them) and from strong enemies that yellow-box them; faction drones for hostile rooms.
- **Reload** only in safe windows; a known client bug shows a split ammo stack as full (30/30 but really
  27/30) — avoid reloading in the room-3 exit window.

---

## 7. Reference fits (for building RunProfiles)

Verbatim EFT blocks the community treats as canonical. Damage is kinetic (Scourge) unless noted.

### Gila — Povertila (T4–T5 baseline, cheap, drone boat)
```
[Gila, Povertila]
Damage Control II
Drone Damage Amplifier II
Drone Damage Amplifier II
Pithum C-Type Medium Shield Booster
Republic Fleet Large Cap Battery
Multispectrum Shield Hardener II
10MN Y-S8 Compact Afterburner
Shield Boost Amplifier II
Pithum C-Type Medium Shield Booster
Rapid Light Missile Launcher II, Scourge Fury Light Missile
Rapid Light Missile Launcher II, Scourge Fury Light Missile
Rapid Light Missile Launcher II, Scourge Fury Light Missile
Rapid Light Missile Launcher II, Scourge Fury Light Missile
Medium Ghoul Compact Energy Nosferatu
Medium Capacitor Control Circuit II
Medium Capacitor Control Circuit II
Medium EM Shield Reinforcer I
Caldari Navy Vespa x10
```
Dual-MSB "semi" cap-management fit; nos for cap; 10× Caldari Navy Vespa; AB only. Load Vespa by weather
hole (see §3).

### Gila — T5/T6 Exotic (dual cap-battery, active)
```
[Gila, T5 Exotic]
Federation Navy Drone Damage Amplifier
Federation Navy Drone Damage Amplifier
Damage Control II
Multispectrum Shield Hardener II
Thukker Large Cap Battery
Pithum A-Type Medium Shield Booster
Federation Navy 10MN Afterburner
Thukker Large Cap Battery
Multispectrum Shield Hardener II
Rapid Light Missile Launcher II, Scourge Fury Light Missile x3
Drone Link Augmentor I
Rapid Light Missile Launcher II, Scourge Fury Light Missile
Caldari Navy Vespa x6 / Vespa II x4
```
Dual-battery = best sustained tank vs neut; DLA extends drone control for standoff on Leshak/Marshal rooms.

### Ishtar — T6 Gamma (drone boat, armor of drones)
```
[Ishtar, T6 Gamma Baseline]
Shadow Serpentis Assault Damage Control
Dread Guristas Drone Damage Amplifier x4
Dread Guristas Omnidirectional Tracking Enhancer
Pithum B-Type Multispectrum Shield Hardener
Thukker Large Cap Battery
Federation Navy 10MN Afterburner
Gist B-Type X-Large Shield Booster
280mm Howitzer Artillery II, Republic Fleet Fusion S x4
Medium EM Shield Reinforcer II
Medium Semiconductor Memory Cell II
Gecko x2 / Republic Fleet Valkyrie x4 / Republic Fleet Warrior x2
```
Sentries/heavies do the work; arty for can-popping. Swap drone type by weather hole.

### Cerberus — T6 Dark (kinetic-locked HAM, web tank)
```
[Cerberus, T6 Dark]
Ballistic Control System II x3
Shadow Serpentis Damage Control
Large Shield Booster / Large Cap Battery
Stasis Webifier II
Pithum A-Type Multispectrum Shield Hardener
10MN Afterburner II
Caldari Navy Heavy Assault Missile Launcher, Scourge Rage HAM x6
Medium Warhead Rigor Catalyst II
Medium EM Shield Reinforcer II
Caldari Navy Hornet x3
```
Kinetic-locked (Scourge only, ~99% of spawns). Double-web in Dark.

### Worm — T1/T2 Exotic (frigate, the bot's current profile)
```
[Worm, T1 exotic]
Drone Damage Amplifier II x2
Republic Fleet Small Cap Battery
Coreli A-Type 5MN Microwarpdrive
Small Shield Booster II
Small Shield Extender II
Arbalest Compact Light Missile Launcher x3
Small Thermal/EM/Cap rigs
Hornet II x3 (Worm has <5 drones by design)
Caldari Navy Scourge Light Missile
```

### Curated guide/fit sites (from the pins)
- Caldari Joans hub (room guides, enemy DB, reward calc): https://caldarijoans.streamlit.app/
- Starter cruiser fits: https://caldarijoans.streamlit.app/Starter_Abyssal_Fits
- Missile damage-type calc: https://caldarijoans.streamlit.app/Missile_Damage_Calc
- Torvald Uruz fits: https://torvalduruz.streamlit.app/  · Gustav Mannfred Gamma Gila: https://gustavmannfred.streamlit.app/Gamma_Gila
- High-tier Gila/Ishtar overview: https://evenomad.com/t6-abyss-gila/
- abysstracker.com/fit/<id> — community fit tracker

---

## 8. What this changed in the bot & what's next

**Done:**
- `NpcInfoProvider.CalcTargetPriority` → §2 consensus order, driven by real ewar flags + name prefixes.
- `NpcInfoProvider.RangeArchetypeFor` (Brawl/Kite/Standoff) + `AnomalyStrategy.Positioning` rewrite:
  brawl-500 m orbit (Karen/Cynabal/Knight/Drekavac/Hunter/Overmind), kite-12 km orbit (Kiki/Vedmak/
  Damavik/Tessera default), standoff keep-at-range-30 km (Leshak/Marshal/Drifter-BS).
- MWD discipline (§4): MWD ON only to close a >20 km gap, OFF once in range (sig bloom + cap). Idempotent.
- Diagnostic now prints a per-enemy bracket breakdown — `Name[tag+ewar]@dist`, tag R=attacking-me /
  Y=locked-me-not-attacking (≈ on my drones solo) / -, P=warp-disrupting-me, J=jamming-me — plus the
  primary's range archetype. Purpose: capture live ground truth for drone-preservation before wiring it.
All builds clean (0 errors).

**Next candidates (all grounded above):**
- Drone preservation (§6): recall drones off strong enemies that yellow-box them (Y tag) / off a Tessera
  primary. Needs ONE live run to confirm the yellow-bracket semantics from the new diagnostic; also
  wants profile-awareness (a Worm's drones survive Tessera; light drones on other boats don't).
- Damage-type selector (§3) for flexible boats when we add a Muninn/Ishtar profile.
- Bail logic (§5): 6:40 room ceiling + death-combo detection for the abyss wrapper (uses IsWarpDisruptingMe/
  IsJammingMe flags now visible per enemy).
