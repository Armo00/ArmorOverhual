# BDB propulsion policy — 2026-09-06

This supersedes the historical mixture/Helium statements in BDB_CFG_AUDIT.md and BDB_Engine_Config_Summary.md. Scope: AO's BDB type-based patches only. Other mods, source BDB assets, existing saves and craft files were not edited.

## Fixed consumption ratios

Ratios are resource-unit/volume ratios, not mass fractions. Values are literal, with no in-game arithmetic.

| Pair (fuel / oxidizer) | Fuel | Oxidizer | Existing AO reference |
| --- | --- | --- | --- |
| Kerosene / LqdOxygen | 0.37694087 | 0.62305913 | SquadPartsOverhaul/Engine/liquidEngineLV-T45.cfg; Mods/NF-LaunchVehicle/Engines/nflv-engine-m1d-1.cfg (37.694087 / 62.305913) |
| MMH / NTO | 0.499 | 0.501 | Mods/NF-Spacecraft/CommandPod.cfg and Engines.cfg |
| LqdMethane / LqdOxygen | 0.4137 | 0.5863 | Mods/CryoEngines/Methalox.cfg |
| LqdHydrogen / LqdOxygen | 0.7276 | 0.2724 | Mods/CryoEngines/CryoEngines.cfg |

No consolidated four-mixture rule was found in the existing Markdown documentation; these values were verified against the existing AO cfg implementations. User's “MMT” is interpreted as MMH.

The retained BDB configurations had 41 Kerolox and 28 Hydrolox pair definitions/edits (including shared fallback conversion patches), but no existing Methalox or MMH/NTO pair. New MMH/NTO RCS options use the table above; no new Methalox engine option is invented.

Historical MMH/MON1 is not silently renamed to MMH/NTO. Aerozine50/MON1, UDMH-based historical engines, fluorine engines, HTP auxiliaries and single-propellant nuclear engines retain their existing mixtures, except for Helium removal.

## Helium

Removed 34 Helium nodes: 31 propulsion consumption nodes, two explicit carried-resource nodes and one TANK allocation. Total tank volumes and all non-Helium allocations are unchanged. No active Helium reference remains in AO/Mods/Bluedog_DB/*.cfg. Shared global resource and tank-type definitions are untouched. Pressure-fed flags and ullage/ignition limitations remain; pressure-fed engines still require a compatible pressurized RF tank.

## Apollo / LM RCS

14 parts covered:
- CommandPod.cfg: Apollo_CrewPod, Apollo_CrewPod_5crew; LM_Ascent_Cockpit, LM_Taxi, LM_Lab, LM_Shelter; LM_LunarFlyingVehicle.
- RCS.cfg: Apollo_RCS_1X, 2X, 3X, 4X; LM_Truck_RCS.
- Engines.cfg: Apollo_RCS_Engine, Apollo_RCS_EngineQuad.

All names above have the bluedog_ prefix. Apollo_RCML has no native RCS module and is intentionally not given a synthetic thruster. LittleJoe2_Body's auxiliary system is not an Apollo spacecraft RCS.

Preserve original default fuel/configuration and add:
- Hydrazine (1); retained rather than duplicated where already the default.
- MMH/NTO (0.499 / 0.501).
- UDMH/NTO (0.4977 / 0.5023), from NF-Spacecraft.
- Aerozine50/MON1 (0.499 / 0.501), matching both AJ10-137 (SPS) and LMAE.

New selectors follow NF-Spacecraft's ModuleEngineConfigs approach. No new performance balancing is introduced: fuel-only CONFIG nodes preserve existing RCS thrust and atmosphere curves; existing R-4D pack configs explicitly retain their 0.445 kN, 278/168 s curve. No new tech-level scaling or pod mass override.

RF 15.15.0 source verification:
- Source/Engines/ModuleEngineConfigs.cs: exact type ModuleRCS enters the dedicated RCS resource-refresh path.
- Source/Engines/EngineConfigPropellants.cs: ClearRCSPropellants updates ALL RCS modules on the part and refreshes consumed-resource lists.
- Therefore EngineQuad gets ONE RCS selector for both transforms, with no thrusterPower or atmosphereCurve override, preserving their distinct performance.
- New selectors have moduleIndex = 0 and engineID = AO-RCS-0 to avoid RF config-cache key collisions with an existing main-engine selector. origMass and tech levels are -1.
- Main-engine MECs, engineIDs, B9 models/transforms and their existing selection logic are not replaced. RCS fuel selection is independent of the main-engine mode; set both as needed in the editor.

Official versioned source:
https://github.com/KSP-RO/RealFuels/blob/v15.15.0.0/Source/Engines/ModuleEngineConfigs.cs
https://github.com/KSP-RO/RealFuels/blob/v15.15.0.0/Source/Engines/EngineConfigPropellants.cs

## Verification and limits

Static verification checks cfg brace structure, active Helium absence, all four fixed pair ratios, RCS option coverage/defaults, unchanged existing engine performance fields and unchanged fuel-tank/life-support capacities. No KSP launch or new ModuleManager cache was produced for this change. Editor switching and flight consumption remain runtime checks for the next user-authorized session. Existing craft/saves may retain old selected resources and should be reviewed in the editor; this change does not migrate them automatically.
