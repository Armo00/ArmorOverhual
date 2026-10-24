# DRE high-temperature compatibility report — 2026-09-06

## Scope and rule

Scanned every CFG under `GameData/zzzArmorOverhual`.

A top-level PART definition or PART patch is in scope when it directly sets either:

- `maxTemp > 1800`
- `skinMaxTemp > 2100`

Every matching code block now conditionally supplies `ModuleAeroReentry` only when DeadlyReentry is installed:

- `leaveTemp = true`
- `maxOperationalTemp = 0.9 * maxTemp`
- `skinMaxOperationalTemp = 0.9 * skinMaxTemp`

Calculated values are stored as numeric literals.

## Result

- Matching code blocks: **254**
- Matching CFG files: **123**
- `Mods/`: **207 blocks in 100 files**
- `SquadPartsOverhaul/`: **47 blocks in 23 files**
- Existing patched PART blocks use `%MODULE[ModuleAeroReentry]:NEEDS[DeadlyReentry]` so an existing module is updated instead of duplicated.
- Five raw PART definitions use `MODULE:NEEDS[DeadlyReentry]` with an explicit `name = ModuleAeroReentry`.

## Single-sided temperature handling

Most blocks that set only `maxTemp` do not define a separate skin temperature. Their skin operational limit therefore uses the same effective temperature value.

Two skin-only patches were resolved against their source definitions:

- `mk2DroneCore`: source `maxTemp = 2500`, AO `skinMaxTemp = 3600`; operational limits `2250 / 3240`.
- `Grid*Fin*L*Titanium`: source `maxTemp = 1660`, AO `skinMaxTemp = 3200`; operational limits `1494 / 2880`.

## KIU boundary report

This global user-authorized rule touched only three existing AO KIU clone blocks:

- `Mods/KIU/Engines/YF-150.cfg`: one block, `2200 / 2200 -> 1980 / 1980`.
- `Mods/KIU/Engines/SRB_tweak.cfg`: two blocks, each `2000 / 2000 -> 1800 / 1800`.

No KIU source mod file was changed and no KIU-specific fault was repaired.

## Validation

- Independent second scan: **254 targets / 254 valid guarded modules**
- Guarded module count: **254**
- Duplicate guarded modules in target blocks: **0**
- Missing or incorrect values: **0**
- CFG brace-balance errors: **0**
- Target files with stale modification date: **0**
- `git diff --check`: no whitespace errors; only existing LF-to-CRLF conversion warnings were emitted
- KSP was not launched

## File inventory

| AO-relative CFG | Matching blocks |
|---|---:|
| `Mods/BoringCrewServices/RO_Starliner.cfg` | 8 |
| `Mods/CST-100/AO_CST_Aero.cfg` | 1 |
| `Mods/CST-100/AO_CST_Command.cfg` | 1 |
| `Mods/Eisenhower-Astronautics_Angara/Angara/Tanks_Structures.cfg` | 1 |
| `Mods/Eisenhower-Astronautics_Angara/Engines/rd0124.cfg` | 1 |
| `Mods/Eisenhower-Astronautics_Angara/Engines/rd191.cfg` | 1 |
| `Mods/Eisenhower-Astronautics_Angara/Engines/rd191v.cfg` | 1 |
| `Mods/Eisenhower-Astronautics_Angara/Engines/rd192.cfg` | 1 |
| `Mods/Eisenhower-Astronautics_Angara/Engines/rd192v.cfg` | 1 |
| `Mods/Endurance/DockingPorts.cfg` | 2 |
| `Mods/Endurance/Endurance.cfg` | 2 |
| `Mods/Endurance/Lander.cfg` | 1 |
| `Mods/Endurance/Ranger.cfg` | 3 |
| `Mods/Endurance/rangerEngineX.cfg` | 1 |
| `Mods/FASA/FASA.cfg` | 1 |
| `Mods/FuelTankPlus/PriceConfig.cfg` | 1 |
| `Mods/Hephaistos_ULA_Vulcan/Aero.cfg` | 1 |
| `Mods/Hephaistos_ULA_Vulcan/Engine.cfg` | 2 |
| `Mods/Hephaistos_ULA_Vulcan/FuelTank.cfg` | 3 |
| `Mods/Hephaistos_ULA_Vulcan/Structural.cfg` | 3 |
| `Mods/Hephaistos_ULA_Vulcan/Thermal.cfg` | 1 |
| `Mods/KerbalFoundries/ALG-Gear.cfg` | 1 |
| `Mods/KerbalReusablitity/ColdGasThruster.cfg` | 3 |
| `Mods/KerbalReusablitity/FuelTank.cfg` | 1 |
| `Mods/KerbalReusablitity/GridFin.cfg` | 1 |
| `Mods/KFS_Reborn/CZ5_RF.cfg` | 1 |
| `Mods/KIU/Engines/SRB_tweak.cfg` | 2 |
| `Mods/KIU/Engines/YF-150.cfg` | 1 |
| `Mods/KK_SpXCD/KK_SpXCD_capsule_cargo.cfg` | 1 |
| `Mods/KK_SpXCD/KK_SpXCD_capsule.cfg` | 1 |
| `Mods/Klockheed_Martian_SSE/RS-25.cfg` | 1 |
| `Mods/LaunchersPack/Atlas V/RD-180.cfg` | 1 |
| `Mods/LaunchersPack/Atlas V/RL-10.cfg` | 1 |
| `Mods/LaunchersPack/Delta/AO_KK_DeltaIV_Adapters_ULA.cfg` | 14 |
| `Mods/LaunchersPack/Delta/AO_KK_DeltaIV_Tanks_ULA.cfg` | 3 |
| `Mods/LaunchersPack/Delta/RL-10.cfg` | 1 |
| `Mods/LaunchersPack/Delta/RS-27A.cfg` | 1 |
| `Mods/LaunchersPack/Delta/RS-68.cfg` | 1 |
| `Mods/LaunchersPack/Delta/SRB.cfg` | 3 |
| `Mods/LaunchersPack/Falcon1.cfg` | 4 |
| `Mods/LaunchersPack/Falcon9/Falcon9_v1.0.cfg` | 7 |
| `Mods/LaunchersPack/Falcon9/Falcon9_v1.1.cfg` | 6 |
| `Mods/LaunchersPack/Falcon9/Falcon9_v1.2.cfg` | 4 |
| `Mods/LaunchersPack/SpaceX Engine/Kestrel.cfg` | 1 |
| `Mods/LaunchersPack/SpaceX Engine/KK_LEM.cfg` | 1 |
| `Mods/LaunchersPack/SpaceX Engine/KK_Raptor_Vac_Mini.cfg` | 1 |
| `Mods/LaunchersPack/SpaceX Engine/Merlin_1C.cfg` | 1 |
| `Mods/LaunchersPack/SpaceX Engine/Merlin_1D.cfg` | 1 |
| `Mods/LaunchersPack/SpaceX Engine/Raptor.cfg` | 3 |
| `Mods/LaunchersPack/Starship.cfg` | 2 |
| `Mods/MarkIVSystem/general_tweak.cfg` | 1 |
| `Mods/MK2Expansion/Engines/M2X_Pluto.cfg` | 1 |
| `Mods/MK2Expansion/MK2E_Temp_Tweak.cfg` | 1 |
| `Mods/MK2Expansion/RCS_Tweak.cfg` | 4 |
| `Mods/NF-LaunchVehicle/Engines/nflv-engine-ar1-1.cfg` | 1 |
| `Mods/NF-LaunchVehicle/Engines/nflv-engine-ar1c-1.cfg` | 1 |
| `Mods/NF-LaunchVehicle/Engines/nflv-engine-m1d-1.cfg` | 1 |
| `Mods/NF-LaunchVehicle/Engines/nflv-engine-m1d-vac-1.cfg` | 1 |
| `Mods/NF-LaunchVehicle/Engines/nflv-engine-rd701-1.cfg` | 1 |
| `Mods/NF-LaunchVehicle/Engines/nflv-engine-rd704-1.cfg` | 1 |
| `Mods/NF-LaunchVehicle/Engines/nflv-engine-rs84-1.cfg` | 1 |
| `Mods/NF-LaunchVehicle/Engines/nflv-engine-rutherford-1.cfg` | 1 |
| `Mods/NF-LaunchVehicle/Engines/nflv-engine-rutherford-vac-1.cfg` | 1 |
| `Mods/NF-LaunchVehicle/Engines/nflv-engine-stbe-kero-1.cfg` | 1 |
| `Mods/NF-LaunchVehicle/Engines/nflv-engine-tr107-1.cfg` | 1 |
| `Mods/NF-LaunchVehicle/Fueltank.cfg` | 3 |
| `Mods/NF-LaunchVehicle/Payload.cfg` | 2 |
| `Mods/NF-LaunchVehicle/Structural.cfg` | 1 |
| `Mods/NF-Spacecraft/CommandPod.cfg` | 4 |
| `Mods/NF-Spacecraft/FuelTank.cfg` | 3 |
| `Mods/NF-Spacecraft/RCS.cfg` | 7 |
| `Mods/NovaPunch/Boosters.cfg` | 2 |
| `Mods/NovaPunch/Engines/Nuclear.cfg` | 2 |
| `Mods/NovaPunch/LES.cfg` | 1 |
| `Mods/NovaPunch/LiquidBooster.cfg` | 1 |
| `Mods/NovaPunch/Odin2.cfg` | 2 |
| `Mods/NovaPunch/Tanks.cfg` | 1 |
| `Mods/Phoenix Industry/ArcReactor.cfg` | 1 |
| `Mods/Phoenix Industry/booster.cfg` | 4 |
| `Mods/Phoenix Industry/CRS.cfg` | 1 |
| `Mods/Phoenix Industry/deepspace.cfg` | 3 |
| `Mods/Phoenix Industry/mav.cfg` | 1 |
| `Mods/PlanetaryBaseInc-KPBS/Container_Greenhouse_Armor.cfg` | 1 |
| `Mods/PlanetaryBaseInc-KPBS/Container_LS_convertor.cfg` | 1 |
| `Mods/ShuttleLiftingBody-CormorantAeronology/cormorantShuttleOverhaul.cfg` | 2 |
| `Mods/ShuttleLiftingBody-CormorantAeronology/Engines.cfg` | 3 |
| `Mods/ShuttleLiftingBody-CormorantAeronology/mk3ShuttleOverhaul.cfg` | 1 |
| `Mods/ShuttleLiftingBody-CormorantAeronology/ShuttleStackOverhaul.cfg` | 4 |
| `Mods/SpaceTux/SpaceTux_MK2_Scale.cfg` | 7 |
| `Mods/Starship Expansion Project/SEP_Deprecated.cfg` | 3 |
| `Mods/Starship Expansion Project/SEP_Engines.cfg` | 4 |
| `Mods/Starship Expansion Project/SEP_Starship_BL2.cfg` | 1 |
| `Mods/Starship Expansion Project/SEP_Structural.cfg` | 3 |
| `Mods/Starship Expansion Project/SEP_SuperHeavy.cfg` | 3 |
| `Mods/Tundra/dragon1.cfg` | 2 |
| `Mods/Tundra/dragon2.cfg` | 3 |
| `Mods/VanStockRevamp/Engine/LVT15.cfg` | 1 |
| `Mods/VanStockRevamp/Engine/PoodleM.cfg` | 1 |
| `Mods/VanStockRevamp/Engine/size2nuclearEngine.cfg` | 1 |
| `Mods/VanStockRevamp/RCS_general.cfg` | 3 |
| `SquadPartsOverhaul/Command/mk1 pod.cfg` | 1 |
| `SquadPartsOverhaul/Command/mk1-3.cfg` | 1 |
| `SquadPartsOverhaul/Command/mk2CockpitInline.cfg` | 1 |
| `SquadPartsOverhaul/Command/mk2Dronecore.cfg` | 1 |
| `SquadPartsOverhaul/Command/Mk2Pod.cfg` | 1 |
| `SquadPartsOverhaul/Command/mk3CockpitShuttle.cfg` | 1 |
| `SquadPartsOverhaul/Engine/liquidEngineLV-909_v2.cfg` | 1 |
| `SquadPartsOverhaul/Engine/liquidEngineLV-N.cfg` | 1 |
| `SquadPartsOverhaul/Engine/liquidEngineMainsail.cfg` | 1 |
| `SquadPartsOverhaul/Engine/liquidEngineMk55.cfg` | 1 |
| `SquadPartsOverhaul/Engine/rapierEngine.cfg` | 1 |
| `SquadPartsOverhaul/Engine/Size2LFB.cfg` | 1 |
| `SquadPartsOverhaul/FuelConversion.cfg` | 1 |
| `SquadPartsOverhaul/heatshield.cfg` | 3 |
| `SquadPartsOverhaul/MakingHistory/LiquidEngineLV-T91.cfg` | 1 |
| `SquadPartsOverhaul/MakingHistory/LiquidEngineLV-TX87.cfg` | 1 |
| `SquadPartsOverhaul/MakingHistory/LiquidEngineRE-J10.cfg` | 1 |
| `SquadPartsOverhaul/MakingHistory/LiquidEngineRK-7.cfg` | 1 |
| `SquadPartsOverhaul/MakingHistory/LiquidEngineRV-1.cfg` | 1 |
| `SquadPartsOverhaul/mk2_general.cfg` | 14 |
| `SquadPartsOverhaul/mk3_general.cfg` | 5 |
| `SquadPartsOverhaul/RCS_general.cfg` | 4 |
| `SquadPartsOverhaul/Utility/1875DockingPort.cfg` | 3 |
