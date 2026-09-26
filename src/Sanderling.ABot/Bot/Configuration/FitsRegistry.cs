using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsInput.Native;

namespace Sanderling.ABot.Bot.Configuration
{
	internal static class FitsRegistry
	{
		public static ShipFit Gila(Bot bot) =>
			new ShipFit(bot.MemoryMeasurementAtTime?.Value?.ShipUi,
				new[]
				{
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Weapon, VirtualKeyCode.F1),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.ShieldBooster, VirtualKeyCode.F2),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.ShieldBooster, VirtualKeyCode.F3),
					},
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc)
					},
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener, VirtualKeyCode.CONTROL,
							VirtualKeyCode.F1),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener, VirtualKeyCode.CONTROL,
							VirtualKeyCode.F2),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.MWD, VirtualKeyCode.CONTROL, VirtualKeyCode.F3),
					}
				})
			{
				MaxDronesInSpace = 2,
			};

		// [Worm, Wyrm MWD Exotic t1] — drone/missile shield-buffer frigate for T1 abyss.
		//   High (1): Arbalest Compact Light Missile Launcher   -> Weapon
		//   Mid  (4): Multispectrum Shield Hardener II          -> Hardener (always-on)
		//             5MN Quad LiF Restrained Microwarpdrive     -> MWD
		//             M51 Benefactor Compact Shield Recharger    -> Etc (passive)
		//             Medium Shield Extender II                  -> Etc (passive)
		//   Low  (2): 2x Drone Damage Amplifier II               -> Etc (passive)
		//   Drones : Hornet II x5 (primary DPS)
		// Module hotkeys are left empty on purpose: activation falls back to clicking the
		// module button, so the fit does not depend on the player's custom keybinds. The
		// mid-slot ORDER here must match the order the modules occupy left-to-right in the
		// HUD (i.e. the slot order they were fitted in): Hardener, MWD, Recharger, Extender.
		public static ShipFit Worm(Bot bot) =>
			new ShipFit(bot.MemoryMeasurementAtTime.Value?.ShipUi,
				new[]
				{
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Weapon),
					},
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.MWD),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
					},
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
					}
				})
			{
				MaxTargetingRange = 30000,
				MaxTargets = 5,
				// The Worm fields FEWER than 5 drones by design (bandwidth / drone-control limit):
				// it flies 2. Setting this to 5 made ShouldLaunch stay true forever (inSpace 2 < 5),
				// so the bot re-sent "launch drones" every tick and the client kept rejecting it.
				MaxDronesInSpace = 2,
				UsesTractor = false,
			};

		// [Stormbringer] — EDENCOM battlecruiser, Vorton Projector (kinetic, chains between targets), NO drones.
		// A roaming Guristas-anomaly ratter (hyperspatial rigs = fast warp). Slot layout must match the HUD
		// left-to-right order (= fitting order):
		//   High (1): Medium Vorton Projector II                 -> Weapon
		//   Mid  (6): Sensor Booster II                          -> AlwaysOn (kept running)
		//             Caldari Navy Large Shield Extender         -> Etc (passive buffer)
		//             Pithum B-Type Kinetic Shield Amplifier     -> Etc (passive)
		//             10MN Afterburner II                        -> MWD (the bot's generic prop-mod slot)
		//             Multispectrum Shield Hardener II           -> Hardener (adaptive: on only when incoming DPS > PassiveRegenDps)
		//             Multispectrum Shield Hardener II           -> Hardener (adaptive)
		//   Low  (3): 2x Vorton Tuning System II, Damage Control II -> Etc (passive)
		// Behaviour: keep the sensor booster on; the combat core turns the hardeners on ONLY when estimated
		// incoming DPS beats PassiveRegenDps (trivial Guristas spawns are tanked passively); lock ONE target
		// (MaxTargets 1) since the Vorton CHAINS to nearby targets; DON'T kite — sit in the pack
		// (PreferredOrbitMeters) so the chain covers everything. No drones (MaxDronesInSpace 0) -> drone tasks no-op.
		public static ShipFit Stormbringer(Bot bot) =>
			new ShipFit(bot.MemoryMeasurementAtTime.Value?.ShipUi,
				new[]
				{
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Weapon) { OptimalRange = 24000 },
					},
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.AlwaysOn), // Sensor Booster — kept on
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),      // Large Shield Extender (passive)
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),      // Kinetic Shield Amplifier (passive)
						new ShipFit.ModuleInfo(ShipFit.ModuleType.MWD),      // 10MN Afterburner (prop)
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener), // Multispectrum Shield Hardener — adaptive
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener), // Multispectrum Shield Hardener — adaptive
					},
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
					}
				})
			{
				MaxTargetingRange = 50000,
				MaxTargets = 1,
				MaxDronesInSpace = 0,
				PreferredOrbitMeters = 5000,   // sit in the pack, no kiting
				PassiveRegenDps = 60,          // below this estimated incoming, the hardeners stay off
				UsesTractor = false,
			};

		// [Hawk, ttt] — throwaway triplebox Hawk for T4 "Raging Dark" abyssals
		// (tactics: abyss_lurkers_ref/HAWK-TRIPLEBOX-GUIDE.md). The 4x Light Missile Launcher II MUST be
		// GROUPED into one HUD button (the 75mm rail stays its own button), so the high row shows 2 buttons.
		//   High (2 buttons): [launcher group] F1               -> Weapon (one volley repeats; no HUD click)
		//                     75mm Gauss Gun                    -> Etc (cache-popping; fired manually)
		//   Mid  (5, fitted order = HUD left-to-right):
		//                     Small C5-L Shield Booster         -> ShieldBooster
		//                     Small Compact Pb-Acid Cap Battery -> Etc (passive)
		//                     Small F-RX Cap Booster (Navy 400) -> CapBooster (inject when cap low)
		//                     1MN Afterburner II                -> AlwaysOn (speed tank: never off in a room)
		//                     Small C5-L Shield Booster         -> ShieldBooster
		//   Low  (2): 2x Ballistic Control System II            -> Etc (passive)
		// No drones. Overheat is deliberately NOT automated (burnout risk) — the supervisor shift-clicks
		// boosters manually when needed.
		/// <param name="npcVelocityMultiplier">
		/// Dark Matter Field speed bonus on the rats, which scales with the filament's tier (≈1.3 at T4,
		/// up to ≈1.5 at T5). Only the ammo model reads it, and it does not flip any charge decision
		/// across that range (the targets Fury wins on are capped by signature, not speed) — so an
		/// imprecise value is safe, it just sharpens the margin.
		/// </param>
		public static ShipFit HawkTTT(Bot bot, double npcVelocityMultiplier = 1.3) =>
			new ShipFit(bot.MemoryMeasurementAtTime.Value?.ShipUi,
				new[]
				{
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Weapon, VirtualKeyCode.F1) { OptimalRange = 60000 },
						// 75mm Gauss Gun — free extra damage on whatever is locked (cache-popping included).
						new ShipFit.ModuleInfo(ShipFit.ModuleType.SecondaryWeapon) { OptimalRange = 8000 },
					},
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.ShieldBooster),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.CapBooster),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.AlwaysOn),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.ShieldBooster),
					},
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
					}
				})
			{
				// Light missiles apply out to ~60 km on this fit; never cease-fire on a 25 km kite line.
				MaxTargetingRange = 60000,
				AttackRange = 60000,
				// 4 grouped LML: one F1 volley pops a 300-EHP cache; used to cap shooters on one-hit kills.
				VolleyDamage = 500,
				MaxTargets = 4,
				MaxDronesInSpace = 0,
				UsesTractor = false,
				// Dual C5-L ≈ 41 HP/s of shield against the Trig th/exp mix, i.e. well over 100 raw DPS
				// countered. Under this the room cannot break the tank and looting can start early.
				SustainableIncomingDps = 120,
				// Verified 2026-09-04 against the pilot's Pyfa + ESI dogma (cap skills at V):
				//   cap  (406.3 hull + 132 Pb-Acid battery) × 1.25 Cap Management V   = 672 GJ
				//   tau  187.5 s × 0.75 Capacitor Systems Operation V                 = 140 s
				//   SB   20 GJ / 2 s cycle × 0.9 Shield Compensation V                = 9 GJ/s each
				//   AB   22 GJ / 10 s cycle × 0.5 Fuel Conservation V                 = 1.1 GJ/s
				//   inj  Navy Cap Booster 400: 400 GJ per 12.75 s injector cycle
				// Shield boost (ESI dogma): C5-L 30 HP / 2 s × 1.375 (Hawk AF bonus 7.5%/lvl, AF V)
				//   = 20.6 HP/s per booster (41.25 dual); pool 935 × 1.25 Shield Management V = 1169 HP
				//   => ~17.6‰/s per booster in the per-mille units the tank thresholds use.
				//   Hull resonances EM 1.0 / Th 0.2 / Kin 0.3 / Ex 0.5 (+EM rig): one booster ≈ 60 raw
				//   Triglavian (th/ex mix) DPS countered, ≈ 100+ vs kin — dual covers a typical T4 wave.
				CapProfile = new CapacitorProfile
				{
					TotalGj = 672,
					RechargeTimeSec = 140,
					PropGjPerSec = 1.1,
					BoosterGjPerSec = 9,
					InjectionGj = 400,
					FloorFraction = 0.15,
					LeadTimeSec = 15,          // > the injector's 12.75 s cycle
					NeutResistanceFactor = 0.78, // Pb-Acid battery: −22% energy warfare resistance
				},
				Ammo = HawkAmmoPlan(npcVelocityMultiplier),
			};

		/// <summary>
		/// The Hawk's charge plan, shared by the live fit and the offline ammo scenarios.
		///
		/// Damage numbers: the pilot's info window showed Navy 232.30 / Fury 288.91 kinetic on
		/// 2026-09-18 under the old 10%/level kinetic hull bonus (×1.5 at Assault Frigates V). Patch
		/// 24.01 of 2026-09-22 replaced it with 7.5%/level to ALL damage types (×1.375), so every Navy
		/// light missile now hits for 232.30 × 1.375 / 1.5 = 212.9 and every Fury for 264.8 whatever
		/// its damage type — the numbers below are that derivation, RE-READ them from the client on
		/// the next docked check. Application (radius / explosion velocity / drf / range) is identical
		/// across damage types within a tier, so the charges differ only by the target's resonance:
		/// Triglavian armor sits at 0.64 to explosive vs 0.47 kinetic, Drifter hulls likewise, which
		/// is why Nova is the default and Scourge is a leftover-stock fallback.
		///   Navy: radius 33 m · expl vel 238 m/s · 7.5 s × 7875 m/s = 59.1 km · drf 0.604
		///   Fury: radius 54 m · expl vel 200.2 m/s · 5.6 s × 8437.5 m/s = 47.2 km · drf 0.682
		/// 4 LML II, 53 rounds each (0.795 m³ / 0.015 m³). Cycle 4.8 s = the effective rate of fire the
		/// client shows this pilot (read 2026-09-26; SDE base 12.8 s, Hawk 5%/lvl RoF, skills, BCS/rigs).
		/// The 3.0 s used before that date was wrong and made every kill look 1.6× faster than it is.
		/// </summary>
		public static ShipFit.AmmoPlan HawkAmmoPlan(double npcVelocityMultiplier = 1.3) =>
			new ShipFit.AmmoPlan
			{
				DefaultChargeTypeId = 27381,
				LauncherCount = 4,
				CycleTimeSec = 4.8,
				MagazineRounds = 53,
				NpcVelocityMultiplier = npcVelocityMultiplier,
				Charges = new[]
				{
					new ShipFit.AmmoCharge
					{
						TypeId = 27381,
						Name = "Caldari Navy Nova Light Missile",
						DamageType = "exp",
						DamagePerMissile = 212.9,
						ExplosionRadiusM = 33,
						ExplosionVelocityMs = 238,
						Drf = 0.604,
						MaxRangeM = 59062,
					},
					new ShipFit.AmmoCharge
					{
						TypeId = 24497,
						Name = "Nova Fury Light Missile",
						DamageType = "exp",
						DamagePerMissile = 264.8,
						ExplosionRadiusM = 54,
						ExplosionVelocityMs = 200.2,
						Drf = 0.682,
						MaxRangeM = 47250,
					},
					// Optional extras: loaded only once seen in the hold. Inferno edges Nova by 1–3% on
					// Damavik/Kikimora/Vedmak/Drekavac; Scourge is for leftover stock and Strikeneedle-type
					// rats with a kinetic hole.
					new ShipFit.AmmoCharge
					{
						TypeId = 27371,
						Name = "Caldari Navy Inferno Light Missile",
						DamageType = "th",
						DamagePerMissile = 212.9,
						ExplosionRadiusM = 33,
						ExplosionVelocityMs = 238,
						Drf = 0.604,
						MaxRangeM = 59062,
						Optional = true,
					},
					new ShipFit.AmmoCharge
					{
						TypeId = 27361,
						Name = "Caldari Navy Scourge Light Missile",
						DamageType = "kin",
						DamagePerMissile = 212.9,
						ExplosionRadiusM = 33,
						ExplosionVelocityMs = 238,
						Drf = 0.604,
						MaxRangeM = 59062,
						Optional = true,
					},
				},
			};

		public static ShipFit Hawk(Bot bot) =>
			new ShipFit(bot.MemoryMeasurementAtTime.Value?.ShipUi,
				new[]
				{
					new[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Weapon, VirtualKeyCode.F1),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Weapon, VirtualKeyCode.F2),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.ShieldBooster, VirtualKeyCode.F3),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.MWD, VirtualKeyCode.F4),
					},
					new ShipFit.ModuleInfo[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc)
					},
					new ShipFit.ModuleInfo[]
					{
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc),
						new ShipFit.ModuleInfo(ShipFit.ModuleType.Etc)
						//new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener, VirtualKeyCode.CONTROL,
						//	VirtualKeyCode.F1),
						//new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener, VirtualKeyCode.CONTROL,
						//	VirtualKeyCode.F2),
						//new ShipFit.ModuleInfo(ShipFit.ModuleType.MWD, VirtualKeyCode.CONTROL, VirtualKeyCode.F3),
					}
				})
			{
				MaxTargetingRange = 20000,
				MaxTargets = 4,
			};

	}
}
