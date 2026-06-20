---
name: KPBS-scale-rules
description: KPBS overall size adjustment baseline — original mod path, full PART inventory, and pending scaling-rule decisions (2026-06-18)
metadata:
  type: project
---

# KPBS Scale Rules

Status: baseline inventory created; scale strategy confirmed on 2026-06-19. Implementation has not started.

## Confirmed Decisions (2026-06-19)

### Existing Patch Cleanup
- All existing patches must have `:NEEDS[PlanetaryBaseInc]` added
- All `@maxAmount *= N` arithmetic must be replaced with direct assigned values (see §Direct Assignment Calculations)
- Corridor clone variants (`AO_`, `AO2_`, `AO3_`) are preserved as-is, no changes

### Docking Ports
- `dock_gangway` and `dock_habitat`: bulkhead → `size2`
- Append to description: `@description ^= :$:  Node uses size2.:`
- See [[description-append-convention]] for MM syntax

### Adapters
- KPBS side: `rescaleFactor = 2` only
- Mixed-profile interfaces: do not manually adjust; let rescaleFactor auto-scale nodes

### Landing Legs/Gear/Wheels
- First round: `rescaleFactor` + `mass` only
- Spring/damper/load values deferred to later testing

### ModSupport
- ALL 75 ModSupport parts included in first pass
- Each patch guarded with appropriate `:NEEDS` for the target mod

### Production Parts — Resource Rate Scaling

| Category | Rate multiplier | Notes |
|---|---|---|
| Mining (Drills, Harvesters) | ×4 | input/output rates |
| Greenhouses (all types) | **no change** | Includes `KKAOSS_Greenhouse_g`, `KKAOSS_LS_container_greenhouse`, `KKAOSS_LS_container_Armor_greenhouse` |
| Other conversion/production | ×4 | ISRU, Smelter, Workshop, OSE, Recycler, etc. |
| Life Support containers (converters) | ×4 | Airfilter, Algae, CarbonExtractor, Elektron, Sabatier, WaterPurifier, USI Recycler |
| MKS | ×4 | MKS Workshop, WOLF Terminal |
| Reactor EC output | ×4 | As previously established |
| Reactor fuel capacity | ×8 | As previously established |

**Important**: Production resource parameters SHOULD NOT be adjusted yet. All 28 production parts are listed for individual review and confirmation before any CFG changes.

### Description Append Convention
See [[description-append-convention]] — `@description ^= :$:  text:` appends to end of description.

## Source Mod

Original mod directory on Mainframe-0:

`D:\KSP\KSP_1.12.3\TestRun\GameData\PlanetaryBaseInc`

Existing ArmorOverhual patches:

`Mods/PlanetaryBaseInc-KPBS/`

## Inventory Scope

All top-level `PART` blocks under `PlanetaryBaseInc` were scanned on 2026-06-18.

Excluded from the count:
- `PART_REQUEST` blocks in contract config files.
- Nested `PART` blocks inside KAS compatibility/upgrade data.
- `INTERNAL`/IVA configs.

Part count:

| Source section | Count |
|---|---:|
| BaseSystem | 52 |
| ContainerSystem | 16 |
| ModSupport | 75 |
| **Total** | **143** |

## Confirmed Scale Strategy

Use **Scheme B: profile-aware ×2 scaling**. KPBS is Kerbal-scale, roughly a 1.25m-class base profile; the target pass scales the visible part size with `rescaleFactor = 2`, while treating `PlanetaryBase`, `Container`, and mixed-profile adapter parts separately.

Important inherited rule from `ksp-node-autoscale.md`: do not manually scale `node_stack_*` or `node_attach` coordinates when changing `rescaleFactor`; KSP scales attachment nodes automatically.

IVA is explicitly out of scope for the first pass. Do not scale `INTERNAL` configs, IVA models, props, or internal camera/seat positions yet.

### Parameter Multipliers

| Parameter class | Multiplier | Notes |
|---|---:|---|
| `rescaleFactor` | set to 2 | Primary geometry change. |
| `node_stack_*`, `node_attach` | no change | KSP auto-scales nodes with `rescaleFactor`. |
| `mass` | ×4 default | Area/structure-based default, not volume ×8. Override manually for special cases. |
| `cost` | ×4 default | Mirrors mass/area growth. |
| `entryCost` | ×4 default | Mirrors mass/area growth. |
| `CrewCapacity` | no change | Physical module grows, but role/capacity stays stable in first pass. |
| `ModuleFuelTanks volume` | ×8 | Applies to RF/MFT storage. |
| Stock `RESOURCE amount/maxAmount` | ×8 for storage resources | Fuel, ore, EC storage, LS supplies, KIS/KAS inventories, etc. |
| `InventorySlots`, `packedVolumeLimit`, KIS volume fields | ×8 | Cargo volume follows cubic scaling. |
| Converter input/output rates | case-by-case | See §Production Parts — Resource Rate Scaling above. Mining ×4, Greenhouses unchanged, others ×4. |
| Reactor EC output | ×4 | Output follows equipment area/installed systems, not fuel volume. |
| Reactor fuel capacity | ×8 | Stored nuclear fuel volume follows cubic scaling. |
| Solar panel `chargeRate` | ×4 | Surface area scaling. |
| Radiator/heat transfer capacity | ×4 | Surface area scaling. |
| `thermalMassModifier` | ×4 default | Follows mass default. |
| Reaction wheel torque | case-by-case | Avoid turning base modules into large spacecraft attitude-control cores. |
| Decoupler/separator `ejectionForce` | no change by default | Revisit only if in-game separation is weak. |
| `crashTolerance`, `breakingForce`, `breakingTorque` | no change by default | Keep joint behavior conservative unless testing shows problems. |
| Docking node type | size2 | `dock_gangway` and `dock_habitat` use size2. Append to description via `@description ^= :$:  Node uses size2.:` |
| `TweakScale defaultScale` | ×2 where present | Match the scaled default visual size. |
| `DRAG_CUBE` | no change | FAR ignores stock drag cubes; do not hand-maintain stale cubes. |
| Engine `maxThrust` | ×4 | Follows nozzle area scaling (rescaleFactor² = 2² = 4). Reference: MK2 ×1.5 → thrust ×2.25. |
| Heat shield `ablator amount` | ×4 | Resource quantity follows area. Other ablation parameters (lossExp, lossConst, etc.) unchanged. |
| Garage parts (`KKAOSS_garage_*`) | ×2 | Confirmed. mk3 bulkhead parts scale with everything else. |

### Patch File Responsibilities

**`KPBS_Scale.cfg` (NEW)**: Handles `rescaleFactor` ONLY. Sets `@rescaleFactor = 2` for all 143 parts.

**Existing functional patches**: Handle everything else:
- `mass`, `cost`, `entryCost` → ×4
- `ModuleFuelTanks volume` → ×8
- Stock `RESOURCE amount/maxAmount` → ×8 (or as specified)
- Converter rates → per production-part rules
- Engine thrust → ×4 (in whatever file handles the landing engine)
- Docking node → size2 + description append
- TweakScale → add/update

**Files requiring modification** (add NEEDS guard + direct values + scale parameters):

| File | Changes needed |
|---|---|
| `Habitat.cfg` | NEEDS guard, direct values, MFT volume ×8 |
| `Control.cfg` | NEEDS guard, direct values, MFT volume ×8 |
| `FuelTank.cfg` | NEEDS guard, MFT volume ×8, Battery EC ×8 (→600000) |
| `GreenHouse.cfg` | NEEDS guard (rates unchanged per greenhouse rule) |
| `science_lab.cfg` | NEEDS guard, cost/entryCost ×4 |
| `Life_Support_Container.cfg` | NEEDS guard, direct values, MFT volume ×8 |
| `corridor.cfg` | NEEDS guard (clones preserved as-is) |
| `adapters.cfg` | NEEDS guard |
| `Container_LS_convertor.cfg` | NEEDS guard, mass ×4 |
| `Container_Greenhouse_Armor.cfg` | NEEDS guard, mass ×4 |

### Additional Confirmed Items

- **Nuclear Fuel Tank** (`KKAOSS_Nuclear_Fuel`): ×8 resource amount (EnrichedUranium/DepletedFuel)
- **Battery** (`KKAOSS_Battery_Tank`): EC ×8 (75000→600000) in FuelTank.cfg
- **Custom Armor Parts** (`KKAOSS_LS_container_Armor_greenhouse`, `KKAOSS_LS_container_Armor_convertor`): ×2 rescaleFactor + ×4 mass
- **Landing Engine** (`KKAOSS_engine_g`): thrust ×4 (= rescaleFactor²)
- **Heat Shield** (`KKAOSS_base_heatshield`): ablator resource ×4, all other params unchanged

### Scope Layers

1. **PlanetaryBase profile parts** are the main target. Apply `rescaleFactor = 2`, mass/cost/entryCost ×4, and storage/resource volume ×8 where applicable.
2. **Container profile parts** follow `rescaleFactor = 2`; storage capacity/resources use ×8, while container dry mass/cost use ×4.
3. **Mixed-profile adapters** such as `size1`, `size2`, `mk2`, `mk3`, `srf`, and `PlanetaryBase` combinations require per-part handling. Scale the KPBS-side geometry, but preserve gameplay compatibility with the external profile unless a custom Armor bulkhead is introduced.
4. **Landing legs, landing gear, wheels, and ground support parts** require manual tuning after geometry scaling. Spring/damper/load values should not be blindly multiplied without testing.
5. **ModSupport parts** are included in the inventory and should be patched with matching `:NEEDS` guards. Apply the same profile-aware rules as their base profile suggests.

### Proposed Patch Organization

Create a dedicated scaling patch later:

`Mods/PlanetaryBaseInc-KPBS/KPBS_Scale.cfg`

Recommended section order inside that file:

1. `PlanetaryBase` core parts
2. `ContainerSystem` parts
3. ModSupport containers and storage
4. Adapters, corridors, docking ports, and mixed-profile parts
5. Landing legs, wheels, and garage support parts
6. Special systems: reactor, solar/EC, drills, converters, science, KAS/KIS

Keep existing functional patches such as `Habitat.cfg`, `GreenHouse.cfg`, `FuelTank.cfg`, and LS container patches focused on their current systems unless a value must be coordinated with the scale pass.

### Implementation Guardrails

- Do not edit cfg files yet; this document records the approved strategy only.
- When implementation begins, every new or changed cfg patch must use `:NEEDS[PlanetaryBaseInc]` or a stricter dependency guard.
- Any touched `.cfg` file must update line 1 to `// Modified YYYY-MM-DD`.
- Use direct assigned final values where ordering with other patches is sensitive; avoid relying on chained ModuleManager arithmetic when another pass may also edit the same field.

## BaseSystem Parts

| Part name | Title token | Category | Mass | Bulkhead | Source cfg |
|---|---|---:|---:|---|---|
| `KKAOSS_INV_FuelTank_small` | `#LOC_KPBS.inventorystorage.title` | none | 0.8 | zDEPRECATED | `BaseSystem\Parts\Cargo\_storage_Inv_big_legacy.cfg` |
| `KKAOSS_INV_FuelTank` | `#LOC_KPBS.inventorystoragesmall.title` | none | 0.4 | zDEPRECATED | `BaseSystem\Parts\Cargo\_storage_Inv_legacy.cfg` |
| `KKAOSS_INV_Tank` | `#LOC_KPBS.containerinventory.title` | Cargo | 0.2 | PlanetaryBase | `BaseSystem\Parts\Cargo\container_Inv.cfg` |
| `KKAOSS_INV_Tank_big` | `#LOC_KPBS.containerinventorybig.title` | Cargo | 0.4 | PlanetaryBase | `BaseSystem\Parts\Cargo\container_Inv_big.cfg` |
| `KKAOSS_INV_FuelTank_small_2` | `#LOC_KPBS.inventorystoragesmall.title` | Cargo | 0.4 | PlanetaryBase | `BaseSystem\Parts\Cargo\storage_Inv.cfg` |
| `KKAOSS_INV_FuelTank_2` | `#LOC_KPBS.inventorystorage.title` | Cargo | 0.8 | PlanetaryBase | `BaseSystem\Parts\Cargo\storage_Inv_big.cfg` |
| `KKAOSS_Central_Hub` | `#LOC_KPBS.centralhub.title` | Pods | 7.5 | PlanetaryBase | `BaseSystem\Parts\Command\CentralHub\Central_Hub.cfg` |
| `KKAOSS_Automatic_Control_g` | `#LOC_KPBS.hal.title` | Pods | 0.25 | PlanetaryBase | `BaseSystem\Parts\Command\Control\Automatic_Control_g.cfg` |
| `KKAOSS_Control_g` | `#LOC_KPBS.command.title` | Pods | 2.0 | PlanetaryBase | `BaseSystem\Parts\Command\Control\Control_g.cfg` |
| `KKAOSS_Cupola_g` | `#LOC_KPBS.cupola.title` | Pods | 1.75 | PlanetaryBase | `BaseSystem\Parts\Command\Control\Cupola_g.cfg` |
| `KKAOSS_Landing_Control_g` | `#LOC_KPBS.mal.title` | Pods | 0.12 | srf | `BaseSystem\Parts\Command\Control\Landing_Control_g.cfg` |
| `KKAOSS_Service_g` | `#LOC_KPBS.service.title` | Payload | 0.35 | PlanetaryBase | `BaseSystem\Parts\Command\Control\Service_g.cfg` |
| `KKAOSS_Centrifuge` | `#LOC_KPBS.centrifuge.title` | Utility | 5.25 | PlanetaryBase | `BaseSystem\Parts\Electrical\Centrifuge.cfg` |
| `KKAOSS_Nuclear_Fuel` | `#LOC_KPBS.nuclearfuel.title` | FuelTank | 3.0 | PlanetaryBase | `BaseSystem\Parts\Electrical\NuclearFuel.cfg` |
| `KKAOSS_Nuclear_Reactor` | `#LOC_KPBS.reactor.title` | Electrical | 0.6 | PlanetaryBase | `BaseSystem\Parts\Electrical\Reactor.cfg` |
| `KKAOSS_Fuel_Tank` | `#LOC_KPBS.fueltank.title` | FuelTank | 1.1 | PlanetaryBase | `BaseSystem\Parts\FuelTank\FuelTank_g.cfg` |
| `KKAOSS_Fuel_Tank_small` | `#LOC_KPBS.fueltanksmall.title` | FuelTank | 0.6 | PlanetaryBase | `BaseSystem\Parts\FuelTank\FuelTank_small_g.cfg` |
| `KKAOSS_drill` | `#LOC_KPBS.drill.title` | Utility | 0.9 | PlanetaryBase | `BaseSystem\Parts\Resources\Drill_g.cfg` |
| `KKAOSS_ISRU_g` | `#LOC_KPBS.isru.title` | Utility | 3.56 | PlanetaryBase | `BaseSystem\Parts\Resources\ISRU_g.cfg` |
| `KKAOSS_Science_g` | `#LOC_KPBS.lab.title` | Science | 3.55 | PlanetaryBase | `BaseSystem\Parts\Science\ScienceLab_g.cfg` |
| `KKAOSS_adapter_base_to_MK2_g` | `#LOC_KPBS.adapterMK2.title` | Structural | 0.14 | PlanetaryBase, mk2 | `BaseSystem\Parts\Structural\AdapterAndHeatShield\AdapterBaseToMK2.cfg` |
| `KKAOSS_adapter_base_to_Size1_g` | `#LOC_KPBS.adaptersize1.title` | Structural | 0.1 | size1, PlanetaryBase | `BaseSystem\Parts\Structural\AdapterAndHeatShield\AdapterBaseToSize1.cfg` |
| `KKAOSS_base_bicupler` | `#LOC_KPBS.bicoupler.title` | Structural | 0.11 | size2, PlanetaryBase | `BaseSystem\Parts\Structural\AdapterAndHeatShield\BaseBiCupler.cfg` |
| `KKAOSS_base_separator` | `#LOC_KPBS.separator.title` | Coupling | 0.35 | PlanetaryBase | `BaseSystem\Parts\Structural\AdapterAndHeatShield\BaseSeparator.cfg` |
| `KKAOSS_base_tricupler` | `#LOC_KPBS.tricpoupler.title` | Structural | 0.3 | size2, PlanetaryBase | `BaseSystem\Parts\Structural\AdapterAndHeatShield\BaseTriCupler.cfg` |
| `KKAOSS_base_heatshield` | `#LOC_KPBS.heatshield.title` | Thermal | 0.35 | PlanetaryBase | `BaseSystem\Parts\Structural\AdapterAndHeatShield\Heatshield_g.cfg` |
| `KKAOSS_gangway_2_adapter` | `#LOC_KPBS.twoendcorridorleg.title` | Structural | 0.15 | size1 | `BaseSystem\Parts\Structural\Corridors\corridor_2end_adapter.cfg` |
| `KKAOSS_corridor_4` | `#LOC_KPBS.fourendcorridor.title` | Structural | 0.17 | size1 | `BaseSystem\Parts\Structural\Corridors\corridor_4end.cfg` |
| `KKAOSS_corridor_6` | `#LOC_KPBS.sixendcorridor.title` | Structural | 0.17 | size1 | `BaseSystem\Parts\Structural\Corridors\corridor_6end.cfg` |
| `KKAOSS_gangway_airlock` | `#LOC_KPBS.corridorairlock.title` | Structural | 0.1 | size1 | `BaseSystem\Parts\Structural\Corridors\corridor_airlock.cfg` |
| `KKAOSS_gangway_end` | `#LOC_KPBS.corridorend.title` | Structural | 0.05 | size1 | `BaseSystem\Parts\Structural\Corridors\corridor_end.cfg` |
| `KKAOSS_CrossSection_g` | `#LOC_KPBS.crosssection.title` | Structural | 0.3 | PlanetaryBase | `BaseSystem\Parts\Structural\CrossSection\CrossSection_g.cfg` |
| `KKAOSS_dock_gangway` | `#LOC_KPBS.corridordock.title` | Coupling | 0.06 | size1 | `BaseSystem\Parts\Structural\DockingPorts\dock_gangway.cfg` |
| `KKAOSS_dock_habitat` | `#LOC_KPBS.basedock.title` | Coupling | 0.1 | size1 | `BaseSystem\Parts\Structural\DockingPorts\dock_habitat.cfg` |
| `KKAOSS_engine_g` | `#LOC_KPBS.landingengine.title` | Propulsion | 0.3 | PlanetaryBase | `BaseSystem\Parts\Structural\DockingPorts\engine_g.cfg` |
| `KKAOSS_adapter_g` | `#LOC_KPBS.structural.title` | Structural | 0.1 | PlanetaryBase | `BaseSystem\Parts\Structural\DockingPorts\structural_g.cfg` |
| `KKAOSS_Flatbed` | `#LOC_KPBS.flatbed.title` | Structural | 1.1 | PlanetaryBase | `BaseSystem\Parts\Structural\Flatbed\Flatbed.cfg` |
| `KKAOSS_airlock_end_g` | `#LOC_KPBS.airlockend.title` | Utility | 0.13 | PlanetaryBase | `BaseSystem\Parts\Utility\Airlocks\airlock_end_g.cfg` |
| `KKAOSS_airlock_mid_g` | `#LOC_KPBS.airlockmid.title` | Utility | 0.13 | PlanetaryBase | `BaseSystem\Parts\Utility\Airlocks\airlock_mid_g.cfg` |
| `KKAOSS_garage_adapter_g_2` | `#LOC_KPBS.garageadapter.title` | Payload | 0.7 | mk3, PlanetaryBase | `BaseSystem\Parts\Utility\Garages\garage_adapter_g.cfg` |
| `KKAOSS_garage_cover_g_2` | `#LOC_KPBS.garagecover.title` | Payload | 0.1 | mk3 | `BaseSystem\Parts\Utility\Garages\garage_cover_g.cfg` |
| `KKAOSS_garage_decoupler_g` | `#LOC_KPBS.garagedecoupler.title` | Payload | 0.3 | mk3, PlanetaryBase | `BaseSystem\Parts\Utility\Garages\garage_decoupler_g.cfg` |
| `KKAOSS_garage_front_g_2` | `#LOC_KPBS.garagefront.title` | Payload | 1.5 | mk3 | `BaseSystem\Parts\Utility\Garages\garage_front_g.cfg` |
| `KKAOSS_garage_side_g_2` | `#LOC_KPBS.garageside.title` | Payload | 4.0 | mk3 | `BaseSystem\Parts\Utility\Garages\garage_side_g.cfg` |
| `KKAOSS_garage_struct_g_2` | `#LOC_KPBS.garagestruct.title` | Payload | 1.0 | mk3 | `BaseSystem\Parts\Utility\Garages\garage_struct_g.cfg` |
| `KKAOSS_garage_adapter_size3_g` | `#LOC_KPBS.garagesize3.title` | Payload | 0.7 | mk3, size3, PlanetaryBase | `BaseSystem\Parts\Utility\Garages\garage_to_size3.cfg` |
| `KKAOSS_Greenhouse_g` | `#LOC_KPBS.greenhouse.title` | Utility | 3.0 | PlanetaryBase | `BaseSystem\Parts\Utility\Greenhouse\Greenhouse_g.cfg` |
| `KKAOSS_Habitat_MK1_g` | `#LOC_KPBS.habitatmk1.title` | Utility | 1.7 | PlanetaryBase | `BaseSystem\Parts\Utility\Habitats\Habitat_MK1_g.cfg` |
| `KKAOSS_Habitat_MK2_g` | `#LOC_KPBS.habitatmk2.title` | Utility | 2.6 | PlanetaryBase | `BaseSystem\Parts\Utility\Habitats\Habitat_MK2_g.cfg` |
| `KKAOSS_Landing_Gear2_g` | `#LOC_KPBS.landinggear.title` | Ground | 0.09 | srf | `BaseSystem\Parts\Wheels\LandingGear.cfg` |
| `KKAOSS_Landing_Leg_g` | `#LOC_KPBS.bigfoot.title` | Ground | 0.08 | srf | `BaseSystem\Parts\Wheels\LandingLeg.cfg` |
| `KKAOSS_Landing_Leg2_g` | `#LOC_KPBS.littlefoot.title` | Ground | 0.075 | srf | `BaseSystem\Parts\Wheels\LandingLeg2.cfg` |

## ContainerSystem Parts

| Part name | Title token | Category | Mass | Bulkhead | Source cfg |
|---|---|---:|---:|---|---|
| `KKAOSS_Storage_End_Cap_Storage` | `#LOC_KPBS.storageend.title` | Payload | 0.15 | PlanetaryBase | `ContainerSystem\End_cap.cfg` |
| `KKAOSS_Storage_g` | `#LOC_KPBS.storagebig.title` | Payload | 0.4 | PlanetaryBase | `ContainerSystem\Storage_g.cfg` |
| `KKAOSS_Storage_mid_g` | `#LOC_KPBS.storagemedium.title` | Payload | 0.25 | PlanetaryBase | `ContainerSystem\Storage_mid_g.cfg` |
| `KKAOSS_Storage_mini_g` | `#LOC_KPBS.storagesmall.title` | Payload | 0.15 | PlanetaryBase | `ContainerSystem\Storage_mini_g.cfg` |
| `KKAOSS_Storage_size2_m` | `#LOC_KPBS.storagesize2big.title` | Payload | 0.3 | size2 | `ContainerSystem\Storage_Size2_m.cfg` |
| `KKAOSS_Storage_size2_s` | `#LOC_KPBS.storagesize2small.title` | Payload | 0.25 | PlanetaryBase | `ContainerSystem\Storage_Size2_s.cfg` |
| `KKAOSS_Battery_Tank` | `#LOC_KPBS.containerbattery.title` | Electrical | 0.35 | Container | `ContainerSystem\tank_Battery.cfg` |
| `KKAOSS_Fuelcell_Tank` | `#LOC_KPBS.containerfuelcell.title` | Electrical | 0.35 | Container | `ContainerSystem\tank_fuelcell.cfg` |
| `KKAOSS_Liquid_Fuel_Tank` | `#LOC_KPBS.containerliquidfuel.title` | FuelTank | 0.25 | Container | `ContainerSystem\tank_liquidFuel.cfg` |
| `KKAOSS_Ore_Tank` | `#LOC_KPBS.containerore.title` | FuelTank | 0.55 | Container | `ContainerSystem\tank_ore.cfg` |
| `KKAOSS_Small_Ore_Tank` | `#LOC_KPBS.containeroresmall.title` | FuelTank | 0.3 | Container | `ContainerSystem\tank_ore_small.cfg` |
| `KKAOSS_RCS_Tank` | `#LOC_KPBS.containerRCS.title` | FuelTank | 0.26 | Container | `ContainerSystem\tank_RCS.cfg` |
| `KKAOSS_Rocket_Fuel_Tank` | `#LOC_KPBS.containerrocktfuelbig.title` | FuelTank | 0.38 | Container | `ContainerSystem\tank_rocketFuel.cfg` |
| `KKAOSS_small_Rocket_Fuel_Tank` | `#LOC_KPBS.containerrocktfuelsmall.title` | FuelTank | 0.2 | Container | `ContainerSystem\tank_rocketFuel_small.cfg` |
| `KKAOSS_ScienceJr_Tank` | `#LOC_KPBS.containerscience.title` | Science | 0.24 | Container | `ContainerSystem\tank_science_jr.cfg` |
| `KKAOSS_Xenon_Tank` | `#LOC_KPBS.containerxenon.title` | FuelTank | 0.24 | Container | `ContainerSystem\tank_xenon.cfg` |

## ModSupport Parts

| Part name | NEEDS/header | Title token | Category | Mass | Bulkhead | Source cfg |
|---|---|---|---:|---:|---|---|
| `CRY-5000Freezer` | `PART:NEEDS[DeepFreeze|USILifeSupport]` | `#LOC_KPBS.cry5000.title` | Utility | 2.7 | PlanetaryBase | `ModSupport\Parts\DeepFreeze\Cryo5000Freezer\Cryo5000Freezer.cfg` |
| `DF_Glykerol_Tank` | `PART:NEEDS[DeepFreeze|USILifeSupport]` | `#LOC_KPBS.clykerolcontainer.title` | FuelTank | 0.24 | Container | `ModSupport\Parts\DeepFreeze\DF_TankGlykerol\tank_Glykerol.cfg` |
| `KKAOSS_MetalOreDrill` | `PART:NEEDS[Launchpad]` | `#LOC_KPBS.metaldrill.title` | Utility | 0.9 | PlanetaryBase | `ModSupport\Parts\Extraplanetary Launchpads\Drill_MetalOre.cfg` |
| `KKAOSS_Storage_Metal` | `PART:NEEDS[Launchpad]` | `#LOC_KPBS.metalstorage.title` | FuelTank | 1.6 | PlanetaryBase | `ModSupport\Parts\Extraplanetary Launchpads\FuelTank_Metal.cfg` |
| `KKAOSS_Storage_MetalOre` | `PART:NEEDS[Launchpad]` | `#LOC_KPBS.metalorestorage.title` | FuelTank | 1.6 | PlanetaryBase | `ModSupport\Parts\Extraplanetary Launchpads\FuelTank_MetalOre.cfg` |
| `KKAOSS_Storage_RocketParts` | `PART:NEEDS[Launchpad]` | `#LOC_KPBS.rocketpartsstorage.title` | FuelTank | 1.6 | PlanetaryBase | `ModSupport\Parts\Extraplanetary Launchpads\FuelTank_RocketParts.cfg` |
| `KKAOSS_Launchpad` | `PART:NEEDS[Launchpad]` | `#LOC_KPBS.launchpad.title` | Structural | 4 | PlanetaryBase | `ModSupport\Parts\Extraplanetary Launchpads\Launchpad.cfg` |
| `KKAOSS_PartRecycler` | `PART:NEEDS[Launchpad]` | `#LOC_KPBS.partrecycler.title` | Utility | 0.25 | PlanetaryBase | `ModSupport\Parts\Extraplanetary Launchpads\Recycler.cfg` |
| `KKAOSS_ScrapMetal` | `PART:NEEDS[Launchpad]` | `#LOC_KPBS.scrapmetal.title` | FuelTank | 0.6 | PlanetaryBase | `ModSupport\Parts\Extraplanetary Launchpads\ScrapMetal.cfg` |
| `KKAOSS_Smelter` | `PART:NEEDS[Launchpad]` | `#LOC_KPBS.smelter.title` | Utility | 2 | PlanetaryBase | `ModSupport\Parts\Extraplanetary Launchpads\Smelter.cfg` |
| `KKAOSS_Workshop` | `PART:NEEDS[Launchpad|GroundConstruction]` | `#LOC_KPBS.workshop.title` | Utility | 3 | PlanetaryBase | `ModSupport\Parts\Extraplanetary Launchpads\Workshop.cfg` |
| `KKAOSS_KAS_Flexible_Corridor` | `PART:NEEDS[KAS]` | `#LOC_KPBS.flexiblecorridor.title` | Structural | 0.3 | size1 | `ModSupport\Parts\KAS\gangway_flexible.cfg` |
| `KKAOSS_KAS_Flexible_Gangway` | `PART:NEEDS[KAS]` | `LEGACY KAS Corridor` | none | 0.25 | size1 | `ModSupport\Parts\KAS\gangway_flexible_LEGACY.cfg` |
| `KKAOSS_KIS_FuelTank` | `PART:NEEDS[KIS]` | `#LOC_KPBS.kisbasestorage.title` | Payload | 1.1 | PlanetaryBase | `ModSupport\Parts\KIS\FuelTank_g.cfg` |
| `KKAOSS_KIS_FuelTank_small` | `PART:NEEDS[KIS]` | `#LOC_KPBS.kisbasestoragesmall.title` | Payload | 0.6 | PlanetaryBase | `ModSupport\Parts\KIS\FuelTank_small_g.cfg` |
| `KKAOSS_KIS_ground_plate` | `PART:NEEDS[KIS]` | `#LOC_KPBS.groundplate.title` | Structural | 0.6 | size1, srf | `ModSupport\Parts\KIS\ground_plate.cfg` |
| `KKAOSS_KIS_Tank` | `PART:NEEDS[KIS]` | `#LOC_KPBS.kisstorage.title` | Payload | 0.2 | PlanetaryBase | `ModSupport\Parts\KIS\tank_KIS.cfg` |
| `KKAOSS_KIS_Tank_big` | `PART:NEEDS[KIS]` | `#LOC_KPBS.kisstoragebig.title` | Payload | 0.37 | PlanetaryBase | `ModSupport\Parts\KIS\tank_KIS_big.cfg` |
| `KKAOSS_LS_container_airfilter` | `PART:NEEDS[TacLifeSupport|IoncrossCrewSupport|Kerbalism]` | `#LOC_KPBS.airfilter.title` | none | 0.7 | Container | `ModSupport\Parts\LifeSupport\Container_Airfilter.cfg` |
| `KKAOSS_LS_container_algae` | `PART:NEEDS[TacLifeSupport|USILifeSupport]` | `#LOC_KPBS.algaefarm.title` | none | 0.5 | Container | `ModSupport\Parts\LifeSupport\Container_Algae.cfg` |
| `KKAOSS_LS_container_carbon_extractor` | `PART:NEEDS[TacLifeSupport|IoncrossCrewSupport|LifeSupport]` | `#LOC_KPBS.carbonextractorcontainer.title` | none | 0.7 | Container | `ModSupport\Parts\LifeSupport\Container_CarbonExtractor.cfg` |
| `KKAOSS_LS_container_co2_big` | `PART:NEEDS[TacLifeSupport|IoncrossCrewSupport|LifeSupport|Kerbalism]` | `#LOC_KPBS.co2containerbig.title` | none | 0.16 | Container | `ModSupport\Parts\LifeSupport\Container_CO2_big.cfg` |
| `KKAOSS_LS_container_co2_small` | `PART:NEEDS[TacLifeSupport|IoncrossCrewSupport|LifeSupport|Kerbalism]` | `#LOC_KPBS.co2containersmall.title` | none | 0.08 | Container | `ModSupport\Parts\LifeSupport\Container_CO2_small.cfg` |
| `KKAOSS_LS_container_eclss_big` | `PART:NEEDS[LifeSupport]` | `#LOC_KPBS.eclsscontainerbig.title` | none | 0.16 | Container | `ModSupport\Parts\LifeSupport\Container_ECLSS_big.cfg` |
| `KKAOSS_LS_container_eclss_small` | `PART:NEEDS[LifeSupport]` | `#LOC_KPBS.eclsscontainersmall.title` | none | 0.08 | Container | `ModSupport\Parts\LifeSupport\Container_ECLSS_small.cfg` |
| `KKAOSS_LS_container_elektron` | `PART:NEEDS[TacLifeSupport]` | `#LOC_KPBS.elektroncontainer.title` | none | 0.7 | Container | `ModSupport\Parts\LifeSupport\Container_Elektron.cfg` |
| `KKAOSS_LS_container_fertilizer_big` | `PART:NEEDS[TacLifeSupport|USILifeSupport]` | `#LOC_KPBS.fertilizercontainer.title` | none | 0.2 | Container | `ModSupport\Parts\LifeSupport\Container_Fertilizer_big.cfg` |
| `KKAOSS_LS_container_fertilizer_small` | `PART:NEEDS[TacLifeSupport|USILifeSupport]` | `#LOC_KPBS.fertilizercontainersmall.title` | none | 0.2 | Container | `ModSupport\Parts\LifeSupport\Container_Fertilizer_small.cfg` |
| `KKAOSS_LS_container_food_big` | `PART:NEEDS[TacLifeSupport|Kerbalism]` | `#LOC_KPBS.foodcontainerbig.title` | none | 0.2 | Container | `ModSupport\Parts\LifeSupport\Container_Food_big.cfg` |
| `KKAOSS_LS_container_food_small` | `PART:NEEDS[TacLifeSupport|Kerbalism]` | `#LOC_KPBS.foodcontainersmall.title` | none | 0.2 | Container | `ModSupport\Parts\LifeSupport\Container_Food_small.cfg` |
| `KKAOSS_LS_container_greenhouse` | `PART:NEEDS[TacLifeSupport|USILifeSupport|IFILifeSupport|Snacks|Kerbalism]` | `#LOC_KPBS.greenhousecontainer.title` | none | 0.5 | Container | `ModSupport\Parts\LifeSupport\Container_Greenhouse.cfg` |
| `KKAOSS_LS_container_hydrogen` | `PART:NEEDS[TacLifeSupport|Kerbalism]` | `#LOC_KPBS.hydrogencontainer.title` | none | 0.28 | Container | `ModSupport\Parts\LifeSupport\Container_Hydrogen.cfg` |
| `KKAOSS_LS_container_ifils_big` | `PART:NEEDS[IFILifeSupport]` | `#LOC_KPBS.IFILSbig.title` | none | 0.16 | Container | `ModSupport\Parts\LifeSupport\Container_IFILS_big.cfg` |
| `KKAOSS_LS_container_ifils_small` | `PART:NEEDS[IFILifeSupport]` | `#LOC_KPBS.IFILSsmall.title` | none | 0.08 | Container | `ModSupport\Parts\LifeSupport\Container_IFILS_small.cfg` |
| `KKAOSS_LS_container_ioncross_big` | `PART:NEEDS[IoncrossCrewSupport]` | `#LOC_KPBS.ioncrossbig.title` | none | 0.16 | Container | `ModSupport\Parts\LifeSupport\Container_IONCROSS_big.cfg` |
| `KKAOSS_LS_container_ioncross_small` | `PART:NEEDS[IoncrossCrewSupport]` | `#LOC_KPBS.ioncrosssmall.title` | none | 0.08 | Container | `ModSupport\Parts\LifeSupport\Container_IONCROSS_small.cfg` |
| `KKAOSS_LS_container_kerbalism_big` | `PART:NEEDS[Kerbalism]` | `#LOC_KPBS.kerbalismcontainerbig.title` | none | 0.16 | Container | `ModSupport\Parts\LifeSupport\Container_Kerbalism_big.cfg` |
| `KKAOSS_LS_container_kerbalism_small` | `PART:NEEDS[Kerbalism]` | `#LOC_KPBS.kerbalismcontainersmall.title` | none | 0.08 | Container | `ModSupport\Parts\LifeSupport\Container_Kerbalism_small.cfg` |
| `KKAOSS_LS_container_mulch_big` | `PART:NEEDS[USILifeSupport]` | `#LOC_KPBS.mulchcontainerbig.title` | none | 0.2 | Container | `ModSupport\Parts\LifeSupport\Container_Mulch_big.cfg` |
| `KKAOSS_LS_container_mulch_small` | `PART:NEEDS[USILifeSupport]` | `#LOC_KPBS.mulchcontainersmall.title` | none | 0.2 | Container | `ModSupport\Parts\LifeSupport\Container_Mulch_small.cfg` |
| `KKAOSS_LS_container_nitrogen` | `PART:NEEDS[Kerbalism]` | `#LOC_KPBS.nitrogencontainer.title` | none | 0.28 | Container | `ModSupport\Parts\LifeSupport\Container_Nitrogen.cfg` |
| `KKAOSS_LS_container_USILS_noms_big` | `PART:NEEDS[USILifeSupport]` | `#LOC_KPBS.nomscontainerbig.title` | none | 0.2 | Container | `ModSupport\Parts\LifeSupport\Container_Noms_big.cfg` |
| `KKAOSS_LS_container_USILS_noms_small` | `PART:NEEDS[USILifeSupport]` | `#LOC_KPBS.nomscontainersmall.title` | none | 0.2 | Container | `ModSupport\Parts\LifeSupport\Container_Noms_small.cfg` |
| `KKAOSS_LS_container_oxygen_big` | `PART:NEEDS[TacLifeSupport|IoncrossCrewSupport|LifeSupport|Kerbalism]` | `#LOC_KPBS.oxygencontainerbig.title` | none | 0.16 | Container | `ModSupport\Parts\LifeSupport\Container_Oxygen_big.cfg` |
| `KKAOSS_LS_container_oxygen_small` | `PART:NEEDS[TacLifeSupport|IoncrossCrewSupport|LifeSupport|Kerbalism]` | `#LOC_KPBS.oxygencontainersmall.title` | none | 0.08 | Container | `ModSupport\Parts\LifeSupport\Container_Oxygen_small.cfg` |
| `KKAOSS_LS_container_sabatier` | `PART:NEEDS[TacLifeSupport|Kerbalism]` | `#LOC_KPBS.sabatiercontainer.title` | none | 0.7 | Container | `ModSupport\Parts\LifeSupport\Container_Sabatier.cfg` |
| `KKAOSS_LS_container_snacks_big` | `PART:NEEDS[Snacks]` | `#LOC_KPBS.snackscontainerbig.title` | none | 0.16 | Container | `ModSupport\Parts\LifeSupport\Container_Snacks_big.cfg` |
| `KKAOSS_LS_container_snacks_small` | `PART:NEEDS[Snacks]` | `#LOC_KPBS.snackscontainersmall.title` | none | 0.08 | Container | `ModSupport\Parts\LifeSupport\Container_Snacks_small.cfg` |
| `KKAOSS_LS_container_tacls_big` | `PART:NEEDS[TacLifeSupport]` | `#LOC_KPBS.taclscontainerbig.title` | none | 0.16 | Container | `ModSupport\Parts\LifeSupport\Container_TACLS_big.cfg` |
| `KKAOSS_LS_container_tacls_small` | `PART:NEEDS[TacLifeSupport]` | `#LOC_KPBS.taclscontainersmall.title` | none | 0.08 | Container | `ModSupport\Parts\LifeSupport\Container_TACLS_small.cfg` |
| `KKAOSS_LS_container_tacls_waste_big` | `PART:NEEDS[TacLifeSupport]` | `#LOC_KPBS.taclswasteproductcontainerbig.title` | none | 0.16 | Container | `ModSupport\Parts\LifeSupport\Container_TACLS_Waste_big.cfg` |
| `KKAOSS_LS_container_tacls_waste_small` | `PART:NEEDS[TacLifeSupport]` | `#LOC_KPBS.taclswasteproductcontainersmall.title` | none | 0.08 | Container | `ModSupport\Parts\LifeSupport\Container_TACLS_Waste_small.cfg` |
| `KKAOSS_LS_container_air_scrubber` | `PART:NEEDS[USILifeSupport]` | `#LOC_KPBS.airscrubbercontainer.title` | none | 0.5 | Container | `ModSupport\Parts\LifeSupport\Container_USILS_AirScrubber.cfg` |
| `KKAOSS_LS_container_USILS_big` | `PART:NEEDS[USILifeSupport]` | `#LOC_KPBS.usilscontainerbig.title` | none | 0.4 | Container | `ModSupport\Parts\LifeSupport\Container_USILS_big.cfg` |
| `KKAOSS_LS_container_USILS_recycler` | `PART:NEEDS[USILifeSupport]` | `#LOC_KPBS.usilsrecyclercontainer.title` | none | 0.7 | Container | `ModSupport\Parts\LifeSupport\Container_USILS_Recycler.cfg` |
| `KKAOSS_LS_container_USILS_small` | `PART:NEEDS[USILifeSupport]` | `#LOC_KPBS.usilscontainersmall.title` | none | 0.2 | Container | `ModSupport\Parts\LifeSupport\Container_USILS_small.cfg` |
| `KKAOSS_LS_container_waste_big` | `PART:NEEDS[TacLifeSupport|Kerbalism]` | `#LOC_KPBS.wastecontainerbig.title` | none | 0.2 | Container | `ModSupport\Parts\LifeSupport\Container_Waste_big.cfg` |
| `KKAOSS_LS_container_waste_small` | `PART:NEEDS[TacLifeSupport|Kerbalism]` | `#LOC_KPBS.wastecontainersmall.title` | none | 0.2 | Container | `ModSupport\Parts\LifeSupport\Container_Waste_small.cfg` |
| `KKAOSS_LS_container_wastewater_big` | `PART:NEEDS[TacLifeSupport]` | `#LOC_KPBS.wastewatercontainerbig.title` | none | 0.2 | Container | `ModSupport\Parts\LifeSupport\Container_WasteWater_big.cfg` |
| `KKAOSS_LS_container_wastewater_small` | `PART:NEEDS[TacLifeSupport]` | `#LOC_KPBS.wastewatercontainersmall.title` | none | 0.2 | Container | `ModSupport\Parts\LifeSupport\Container_WasteWater_Small.cfg` |
| `KKAOSS_LS_container_water_big` | `PART:NEEDS[TacLifeSupport|USILifeSupport]` | `#LOC_KPBS.watercontainerbig.title` | none | 0.2 | Container | `ModSupport\Parts\LifeSupport\Container_Water_big.cfg` |
| `KKAOSS_LS_container_water_small` | `PART:NEEDS[TacLifeSupport|USILifeSupport]` | `#LOC_KPBS.watercontainersmall.title` | none | 0.2 | Container | `ModSupport\Parts\LifeSupport\Container_Water_Small.cfg` |
| `KKAOSS_LS_container_waterpurifier` | `PART:NEEDS[TacLifeSupport|USILifeSupport]` | `#LOC_KPBS.waterpurifiercontainer.title` | none | 0.7 | Container | `ModSupport\Parts\LifeSupport\Container_WaterPurifier.cfg` |
| `KKAOSS_LS_drill_water` | `PART:NEEDS[TacLifeSupport|USILifeSupport|Kerbalism]` | `#LOC_KPBS.waterdrill.title` | Utility | 0.9 | PlanetaryBase | `ModSupport\Parts\LifeSupport\Drill_Water_g.cfg` |
| `KKAOSS_Water_OrbitalScanner` | `PART:NEEDS[TacLifeSupport|USILifeSupport]` | `#LOC_KPBS.orbitalscanner.title` | Science | 0.1 | srf | `ModSupport\Parts\LifeSupport\OrbitalScanner.cfg` |
| `KKAOSS_Water_SurfaceScanner` | `PART:NEEDS[TacLifeSupport|USILifeSupport]` | `#LOC_KPBS.surfacescanner.title` | Science | 0.005 | srf | `ModSupport\Parts\LifeSupport\SurfaceScanner.cfg` |
| `KKAOSS_USI_Recicler_g` | `PART:NEEDS[USILifeSupport|Kerbalism&USILifeSupport|ProfileDefault]` | `#LOC_KPBS.usirecycler.title` | none | 1.05 | PlanetaryBase | `ModSupport\Parts\LifeSupport\USI-Recycler.cfg` |
| `KKAOSS_MKS_Workshop` | `PART:NEEDS[MKS]` | `#LOC_KPBS.MKS.workshop.title` | Utility | 2.5 | PlanetaryBase | `ModSupport\Parts\MKS\MKSWorkshop.cfg` |
| `KKAOSS_MKS_WOLFTerminal` | `PART:NEEDS[MKS&USI_WOLF]` | `#LOC_KPBS.MKS.wolfterminal.title` | Payload | 4 | PlanetaryBase | `ModSupport\Parts\MKS\WOLFTerminal.cfg` |
| `KKAOSS_DirtDrill` | `PART:NEEDS[Workshop]` | `#LOC_KPBS.dirtdrill.title` | Utility | 0.9 | PlanetaryBase | `ModSupport\Parts\OSE Workshop\Drill_Dirt.cfg` |
| `KKAOSS_MaterialKits` | `PART:NEEDS[Workshop|GroundConstruction|MKS]` | `#LOC_KPBS.materialkits.title` | FuelTank | 1.1 | PlanetaryBase | `ModSupport\Parts\OSE Workshop\MaterialKits.cfg` |
| `KKAOSS_OSEConverter` | `PART:NEEDS[Workshop]` | `#LOC_KPBS.oseconverter.title` | Utility | 3.5 | PlanetaryBase | `ModSupport\Parts\OSE Workshop\OSE_Converter.cfg` |
| `KKAOSS_OSEResources` | `PART:NEEDS[Workshop]` | `#LOC_KPBS.oseresources.title` | FuelTank | 0.6 | PlanetaryBase | `ModSupport\Parts\OSE Workshop\OSE_Resources.cfg` |
| `KKAOSS_OSEworkshop` | `PART:NEEDS[Workshop]` | `#LOC_KPBS.oseworkshop.title` | Utility | 2.5 | PlanetaryBase | `ModSupport\Parts\OSE Workshop\OSE_workshop.cfg` |
| `KKAOSS_container_SEP` | `PART:NEEDS[SEPScience]` | `#LOC_KPBS.sepstation.title` | Science | 0.07 | size1, srf | `ModSupport\Parts\SurfaceExperimentPackage\Container_SEP.cfg` |

## Direct Assignment Calculations

Replace all `@maxAmount *= N` with final calculated values:

| File | Part | Resource | Base Value | ×N | Direct Value |
|---|---|---|---|---|---|
| Habitat.cfg | KKAOSS_Habitat_MK2_g | Food | 5.84928 | 80 | 467.9424 |
| Habitat.cfg | KKAOSS_Habitat_MK2_g | Water | 3.87072 | 80 | 309.6576 |
| Habitat.cfg | KKAOSS_Habitat_MK2_g | Oxygen | 591.84 | 80 | 47347.2 |
| Habitat.cfg | KKAOSS_Habitat_MK1_g | Food | 5.84928 | 40 | 233.9712 |
| Habitat.cfg | KKAOSS_Habitat_MK1_g | Water | 3.87072 | 40 | 154.8288 |
| Habitat.cfg | KKAOSS_Habitat_MK1_g | Oxygen | 591.84 | 40 | 23673.6 |
| Control.cfg | KKAOSS_Central_Hub | Food | 5.84928 | 120 | 701.9136 |
| Control.cfg | KKAOSS_Central_Hub | Water | 3.87072 | 120 | 464.4864 |
| Control.cfg | KKAOSS_Central_Hub | Oxygen | 591.84 | 120 | 71020.8 |
| Life_Support_Container.cfg | Big (×140) | Food | 5.84928 | 140 | 818.8992 |
| Life_Support_Container.cfg | Big (×140) | Water | 3.87072 | 140 | 541.9008 |
| Life_Support_Container.cfg | Big (×140) | Oxygen | 591.84 | 140 | 82857.6 |
| Life_Support_Container.cfg | Small (×70) | Food | 5.84928 | 70 | 409.4496 |
| Life_Support_Container.cfg | Small (×70) | Water | 3.87072 | 70 | 270.9504 |
| Life_Support_Container.cfg | Small (×70) | Oxygen | 591.84 | 70 | 41428.8 |

## Full Resource Storage ×8 Table

77 parts have stock RESOURCE blocks. Note: many parts already have ArmorOverhaul patches that replace RESOURCE with ModuleFuelTanks — for those, ×8 applies to MFT volume, not the stock values shown here. Parts NOT yet patched with MFT need their RESOURCE amounts ×8 directly.

| Part | Current Resources | ×8 Values |
|---|---|---|
| KKAOSS_Automatic_Control_g | EC=250 | EC=2000 |
| KKAOSS_Battery_Tank | EC=3500 (stock), 75000 (ArmorOverhaul) | EC=600000 (from ArmorOverhaul value) |
| KKAOSS_Central_Hub | EC=150 (stock; ArmorOverhaul uses MFT) | MFT volume ×8 |
| KKAOSS_Centrifuge | EnrichedUranium=190, DepletedFuel=190 | EU=1520, DF=1520 |
| KKAOSS_Control_g | EC=150 | EC=1200 |
| KKAOSS_Cupola_g | EC=150 | EC=1200 |
| KKAOSS_Fuel_Tank | LiquidFuel=720, Oxidizer=880 | LF=5760, Ox=7040 |
| KKAOSS_Fuel_Tank_small | LiquidFuel=360, Oxidizer=440 | LF=2880, Ox=3520 |
| KKAOSS_Fuelcell_Tank | EC=250 | EC=2000 |
| KKAOSS_Habitat_MK1_g | EC=150 (stock; ArmorOverhaul uses MFT) | MFT volume ×8 |
| KKAOSS_Habitat_MK2_g | EC=200 (stock; ArmorOverhaul uses MFT) | MFT volume ×8 |
| KKAOSS_LS_container_USILS_big | Supplies=1600, Mulch=200, Fertilizer=200 | S=12800, M=1600, F=1600 |
| KKAOSS_LS_container_USILS_noms_big | Supplies=2000 | S=16000 |
| KKAOSS_LS_container_USILS_noms_small | Supplies=1000 | S=8000 |
| KKAOSS_LS_container_USILS_small | Supplies=800, Mulch=100, Fertilizer=100 | S=6400, M=800, F=800 |
| KKAOSS_LS_container_airfilter | O2=200, CO2=200, H2=200, IntakeAir=0.8 | O2=1600, CO2=1600, H2=1600, IA=6.4 |
| KKAOSS_LS_container_algae | Fertilizer=300, Mulch=100 | F=2400, M=800 |
| KKAOSS_LS_container_carbon_extractor | CO2=500 | CO2=4000 |
| KKAOSS_LS_container_co2_big | CO2=127883 | CO2=1,023,065 |
| KKAOSS_LS_container_co2_small | CO2=63942 | CO2=511,533 |
| KKAOSS_LS_container_eclss_big | O2=1300, CO2=1300 | O2=10400, CO2=10400 |
| KKAOSS_LS_container_eclss_small | O2=650, CO2=650 | O2=5200, CO2=5200 |
| KKAOSS_LS_container_elektron | H2=200 | H2=1600 |
| KKAOSS_LS_container_fertilizer_big | Fertilizer=600 | F=4800 |
| KKAOSS_LS_container_fertilizer_small | Fertilizer=300 | F=2400 |
| KKAOSS_LS_container_food_big | Food=1462.5 | Food=11700 |
| KKAOSS_LS_container_food_small | Food=731.25 | Food=5850 |
| KKAOSS_LS_container_hydrogen | H2=6000 | H2=48000 |
| KKAOSS_LS_container_ifils_big | LifeSupport=150 | LS=1200 |
| KKAOSS_LS_container_ifils_small | LifeSupport=75 | LS=600 |
| KKAOSS_LS_container_ioncross_big | O2=60000, CO2=4000 | O2=480000, CO2=32000 |
| KKAOSS_LS_container_ioncross_small | O2=30000, CO2=2000 | O2=240000, CO2=16000 |
| KKAOSS_LS_container_mulch_big | Mulch=2000 | M=16000 |
| KKAOSS_LS_container_mulch_small | Mulch=1000 | M=8000 |
| KKAOSS_LS_container_nitrogen | Nitrogen=701298 | N2=5,610,390 |
| KKAOSS_LS_container_oxygen_big | O2=148050 | O2=1,184,397 |
| KKAOSS_LS_container_oxygen_small | O2=74025 | O2=592,199 |
| KKAOSS_LS_container_snacks_big | Snacks=1000, Soil=1000 | Sn=8000, So=8000 |
| KKAOSS_LS_container_snacks_small | Snacks=500, Soil=500 | Sn=4000, So=4000 |
| KKAOSS_LS_container_tacls_big | Food=487.5, Water=322.2, O2=49350 | F=3900, W=2578, O2=394799 |
| KKAOSS_LS_container_tacls_small | Food=243.75, Water=161.1, O2=24675 | F=1950, W=1289, O2=197399 |
| KKAOSS_LS_container_tacls_waste_big | Waste=44.3, WasteWater=410.3, CO2=42628 | W=355, WW=3283, CO2=341022 |
| KKAOSS_LS_container_tacls_waste_small | Waste=22.2, WasteWater=205.2, CO2=21314 | W=177, WW=1641, CO2=170511 |
| KKAOSS_LS_container_waste_big | Waste=133 | W=1064 |
| KKAOSS_LS_container_waste_small | Waste=66.5 | W=532 |
| KKAOSS_LS_container_wastewater_big | WasteWater=1231 | WW=9848 |
| KKAOSS_LS_container_wastewater_small | WasteWater=615.5 | WW=4924 |
| KKAOSS_LS_container_water_big | Water=966.65 | Water=7733 |
| KKAOSS_LS_container_water_small | Water=483.3 | Water=3867 |
| KKAOSS_LS_drill_water | Water=50 | Water=400 |
| KKAOSS_Landing_Control_g | EC=35 | EC=280 |
| KKAOSS_Liquid_Fuel_Tank | LiquidFuel=200 | LF=1600 |
| KKAOSS_MKS_WOLFTerminal | EC=1000 | EC=8000 |
| KKAOSS_MKS_Workshop | EC=1000, Machinery=2000, MaterialKits=1000, SpecializedParts=200, Recyclables=2500 | EC=8000, Ma=16000, MK=8000, SP=1600, Re=20000 |
| KKAOSS_MaterialKits | MaterialKits=6000 | MK=48000 |
| KKAOSS_Nuclear_Fuel | DepletedFuel=1000, EnrichedUranium=1000 | DF=8000, EU=8000 |
| KKAOSS_Nuclear_Reactor | EC=800, EnrichedUranium=250, DepletedFuel=250 | EC=6400, EU=2000, DF=2000 |
| KKAOSS_OSEConverter | Ore=40, Dirt=40 | Ore=320, Dirt=320 |
| KKAOSS_OSEworkshop | EC=1500, MaterialKits=100 | EC=12000, MK=800 |
| KKAOSS_Ore_Tank | Ore=400 | Ore=3200 |
| KKAOSS_RCS_Tank | MonoPropellant=350 | MP=2800 |
| KKAOSS_Rocket_Fuel_Tank | LiquidFuel=180, Oxidizer=220 | LF=1440, Ox=1760 |
| KKAOSS_ScrapMetal | ScrapMetal=600 | SM=4800 |
| KKAOSS_Small_Ore_Tank | Ore=200 | Ore=1600 |
| KKAOSS_Smelter | Metal=25, MetalOre=300, ScrapMetal=200 | Me=200, MO=2400, SM=1600 |
| KKAOSS_Storage_Metal | Metal=1000 | Me=8000 |
| KKAOSS_Storage_MetalOre | MetalOre=1000 | MO=8000 |
| KKAOSS_Storage_RocketParts | RocketParts=1000 | RP=8000 |
| KKAOSS_Workshop | RocketParts=400 | RP=3200 |
| KKAOSS_Xenon_Tank | XenonGas=5250 | Xe=42000 |
| KKAOSS_adapter_g | LiquidFuel=45, Oxidizer=55 | LF=360, Ox=440 |
| KKAOSS_base_heatshield | Ablator=600 | Ablator=4800 |
| KKAOSS_container_SEP | EC=600 | EC=4800 |
| KKAOSS_engine_g | LiquidFuel=45, Oxidizer=55 | LF=360, Ox=440 |

## Conversion Rate ×4 Tables

### Mining (ModuleResourceHarvester) — ×4

| Part | Converter | Resource | Dir | Current Rate | ×4 Rate |
|---|---|---|---|---|---|
| KKAOSS_drill | Ore Harvester | ElectricCharge | IN | 7.5 | 30 |
| KKAOSS_LS_drill_water | Water Drill | ElectricCharge | IN | 6 | 24 |
| KKAOSS_DirtDrill | Dirt Harvester | ElectricCharge | IN | 7.5 | 30 |
| KKAOSS_MetalOreDrill | MetalOre Harvester | ElectricCharge | IN | 7.5 | 30 |
| KKAOSS_drill (+Workshop) | Dirt Harvester | ElectricCharge | IN | 1 | 4 |
| KKAOSS_drill (+Workshop) | ExoticMinerals | ElectricCharge | IN | 1 | 4 |
| KKAOSS_drill (+Workshop) | RareMetals | ElectricCharge | IN | 1 | 4 |

Note: Harvester output has no Ratio — determined by planetary abundance and thermal efficiency curves.

### Greenhouses — **UNCHANGED**

| Part | Converter | Resource | Dir | Rate |
|---|---|---|---|---|
| KKAOSS_Greenhouse_g (ArmorOverhaul) | GreenhouseAR | EC=5, Fert=5.79e-06, WasteWater=0.00057, CO2=0.05919, Waste=0.0000615 → O2=0.0686, Food=0.000678, Water=0.000449, H2=0.0178 | — | **UNCHANGED** |
| KKAOSS_LS_container_Armor_greenhouse (ArmorOverhaul) | Greenhouse | EC=5, Fert=1e-06, Water=9.65e-07, Waste=5.36e-06, CO2=0.000127 → O2=8.56e-05, Food=0.000203 | — | **UNCHANGED** |
| KKAOSS_LS_container_greenhouse (TAC LS) | Greenhouse | EC=0.5, Fert=2.77e-06, Water=1.86e-06, CO2=0.000247 → O2=0.000286, Food=1.69e-05 | — | **UNCHANGED** |
| KKAOSS_LS_container_greenhouse (USI LS) | Recycler | Mulch=0.00045, Fert=4.5e-05, EC=0.99 → Supplies=0.000495 | — | **UNCHANGED** |
| KKAOSS_LS_container_greenhouse (Snacks) | Converter | Ore=0.125, EC=1 → Snacks=0.25 | — | **UNCHANGED** |

### ISRU — ×4

| Part | Converter | Resource | Dir | Current | ×4 |
|---|---|---|---|---|---|
| KKAOSS_ISRU_g | LFO | Ore | IN | 0.4 | 1.6 |
| | | ElectricCharge | IN | 24 | 96 |
| | | LiquidFuel | OUT | 0.36 | 1.44 |
| | | Oxidizer | OUT | 0.44 | 1.76 |
| | MonoProp | Ore | IN | 0.4 | 1.6 |
| | | ElectricCharge | IN | 24 | 96 |
| | | MonoPropellant | OUT | 0.8 | 3.2 |
| | LF-only | Ore | IN | 0.36 | 1.44 |
| | | ElectricCharge | IN | 24 | 96 |
| | | LiquidFuel | OUT | 0.72 | 2.88 |
| | Ox-only | Ore | IN | 0.44 | 1.76 |
| | | ElectricCharge | IN | 24 | 96 |
| | | Oxidizer | OUT | 0.88 | 3.52 |

### Reactor & Centrifuge — ×4

| Part | Converter | Resource | Dir | Current | ×4 |
|---|---|---|---|---|---|
| KKAOSS_Nuclear_Reactor | Nuclear Reactor | EnrichedUranium | IN | 2.4e-06 | 9.6e-06 |
| | | DepletedFuel | OUT | 2.4e-06 | 9.6e-06 |
| | | ElectricCharge | OUT | 1350 | 5400 |
| KKAOSS_Centrifuge | Re-Processor | DepletedFuel | IN | 0.06 | 0.24 |
| | | ElectricCharge | IN | 120 | 480 |
| | | EnrichedUranium | OUT | 0.03 | 0.12 |
| | Xenon Extractor | DepletedFuel | IN | 0.006 | 0.024 |
| | | ElectricCharge | IN | 60 | 240 |
| | | XenonGas | OUT | 0.15 | 0.6 |
| | Uranium Extractor | Ore | IN | 0.6 | 2.4 |
| | | ElectricCharge | IN | 120 | 480 |
| | | EnrichedUranium | OUT | 0.0006 | 0.0024 |

### Fuel Cell — ×4

| Part | Converter | Resource | Dir | Current | ×4 |
|---|---|---|---|---|---|
| KKAOSS_Fuelcell_Tank | Fuel Cell | LiquidFuel | IN | 0.02025 | 0.081 |
| | | Oxidizer | IN | 0.02475 | 0.099 |
| | | ElectricCharge | OUT | 18 | 72 |

### LS Containers (ModuleKPBSConverter) — ×4

| Part | Converter | Resource | Dir | Current | ×4 |
|---|---|---|---|---|---|
| KKAOSS_LS_container_algae | Algae Farm | Waste | IN | 4.5e-06 | 1.8e-05 |
| | | Ore | IN | 0.0001 | 0.0004 |
| | | ElectricCharge | IN | 0.5 | 2 |
| | | Fertilizer | OUT | 8.3e-06 | 3.32e-05 |
| KKAOSS_LS_container_carbon_extractor | Carbon Extractor | ElectricCharge | IN | 0.3246 | 1.2985 |
| | | CarbonDioxide | IN | 0.01022 | 0.04088 |
| | | Oxygen | OUT | 0.01028 | 0.04112 |
| | | Waste | OUT | 5.44e-06 | 2.18e-05 |
| KKAOSS_LS_container_elektron | Elektron | ElectricCharge | IN | 2.3 | 9.2 |
| | | Water | IN | 1.63e-05 | 6.53e-05 |
| | | Oxygen | OUT | 0.01028 | 0.04112 |
| | | Hydrogen | OUT | 0.02034 | 0.08135 |
| KKAOSS_LS_container_sabatier | Sabatier | ElectricCharge | IN | 0.09996 | 0.3998 |
| | | CarbonDioxide | IN | 0.01028 | 0.04112 |
| | | Hydrogen | IN | 0.04068 | 0.1627 |
| | | Water | OUT | 1.63e-05 | 6.53e-05 |
| | | Waste | OUT | 9.66e-06 | 3.87e-05 |
| KKAOSS_LS_container_waterpurifier | Water Filter | ElectricCharge | IN | 0.05813 | 0.2325 |
| | | WasteWater | IN | 8.55e-05 | 3.42e-04 |
| | | Water | OUT | 7.69e-05 | 3.08e-04 |
| | | Waste | OUT | 1.20e-05 | 4.79e-05 |
| KKAOSS_LS_container_airfilter | O2/H2/CO2 Filters | ElectricCharge | IN | 1/1/1 | 4/4/4 |

### Custom Armor Parts — ×4

| Part | Converter | Resource | Dir | Current | ×4 |
|---|---|---|---|---|---|
| KKAOSS_LS_container_Armor_convertor | Hydra-Producter | EC | IN | 10 | 40 |
| | | Ore | IN | 0.0005 | 0.002 |
| | | Oxygen | OUT | 0.0685 | 0.274 |
| | | Water | OUT | 0.000448 | 0.001792 |
| | | Fertilizer | OUT | 0.00001 | 0.00004 |
| | FuelGenerator | EC | IN | 10 | 40 |
| | | Ore | IN | 0.036 | 0.144 |
| | | LiquidFuel | OUT | 0.072 | 0.288 |
| | RCSGenerator | EC | IN | 10 | 40 |
| | | Ore | IN | 0.036 | 0.144 |
| | | MMH | OUT | 0.0499 | 0.1996 |
| | | NTO | OUT | 0.0501 | 0.2004 |

### OSE Converter — ×4

| Part | Converter | Resource | Dir | Current | ×4 |
|---|---|---|---|---|---|
| KKAOSS_OSEConverter | MaterialKits | Ore | IN | 0.7 | 2.8 |
| | | ElectricCharge | IN | 3.5 | 14 |
| | | MaterialKits | OUT | 0.2625 | 1.05 |
| | Prospector | Dirt | IN | 0.7 | 2.8 |
| | | ElectricCharge | IN | 3.5 | 14 |
| | | ExoticMinerals | OUT | 0.014 | 0.056 |
| | | RareMetals | OUT | 0.014 | 0.056 |

### MKS Workshop — ×4

| Part | Converter | Resource | Dir | Current | ×4 |
|---|---|---|---|---|---|
| KKAOSS_MKS_Workshop | MaterialKits | Metals | IN | 0.0112 | 0.0448 |
| | | Chemicals | IN | 0.0056 | 0.0224 |
| | | Polymers | IN | 0.0112 | 0.0448 |
| | | ElectricCharge | IN | 56 | 224 |
| | | Machinery | IN | 0.00005 | 0.0002 |
| | | Recyclables | OUT | 0.00005 | 0.0002 |
| | | MaterialKits | OUT | 0.028 | 0.112 |
| | Machinery | MaterialKits | IN | 0.0224 | 0.0896 |
| | | SpecializedParts | IN | 0.0056 | 0.0224 |
| | | ElectricCharge | IN | 56 | 224 |
| | | Machinery | IN | 0.00005 | 0.0002 |
| | | Machinery | OUT | 0.028 | 0.112 |
| | | Recyclables | OUT | 0.00005 | 0.0002 |
| | Workshop Boost | ElectricCharge | IN | 75 | 300 |
| | | Machinery | IN | 0.00005 | 0.0002 |
| | | Recyclables | OUT | 0.00005 | 0.0002 |

## Non-Standard Parts — ×4 Considerations

These parts use custom modules (EL, MKS, USI) with non-Ratio parameters:

| Part | Module | Key Param | Current | ×4 | Issue |
|---|---|---|---|---|---|
| KKAOSS_Workshop | ELWorkshop | ProductivityFactor | 3 | 12 | |
| KKAOSS_Smelter | ELConverter | Rate | 1 kg/s | 4 kg/s | |
| KKAOSS_PartRecycler | ELRecycler | (no rate param) | N/A | N/A | |
| KKAOSS_Launchpad | ELLaunchpad | (no rate param) | N/A | N/A | Not a converter |
| KKAOSS_MKS_Workshop | EfficiencyBooster | EfficiencyMultiplier | 20 | 80 | Not a Rate — leave? |
| KKAOSS_MKS_WOLFTerminal | WOLF_TerminalModule | (abstracted) | N/A | N/A | WOLF logistics — no rates |
| KKAOSS_MKS_WOLFTerminal | MKSModule | EfficiencyMultiplier | 6 | 24 | Not a Rate — leave? |
| KKAOSS_USI_Recicler_g | USILS_Recycler | RecyclePercent | 70% | 280% | **CAP at 100%** |
| KKAOSS_LS_container_USILS_recycler | USILS_Recycler | RecyclePercent | 60% | 240% | **CAP at 100%** |
| KKAOSS_LS_container_air_scrubber | USILS_Recycler | RecyclePercent | 40% | 160% | **CAP at 100%** |
| KKAOSS_LS_container_waterpurifier (USI) | USILS_Recycler | RecyclePercent | 82% | 328% | **CAP at 100%** |

**Key notes:**
- `RecyclePercent` values overflow 100% if ×4 — must be capped at 1.0 (100%)
- `EfficiencyMultiplier` and `ProductivityFactor` are not conversion Rates — need separate design decision
- `REQUIRED_RESOURCE[Machinery].Ratio = 2500` on MKSWorkshop is a minimum stored amount, NOT a consumption rate — do NOT multiply
- WOLF uses abstracted logistics — no per-second rates to adjust
- EL `Rate` uses kg/s rather than KSP Ratio — multiply the Rate parameter directly

## Implementation Checklist

- [x] Scale strategy confirmed (×2 rescaleFactor)
- [x] Parameter multipliers confirmed (mass ×4, cost ×4, MFT volume ×8, resource stock ×8)
- [x] MM arithmetic → direct values calculated
- [x] NEEDS guards to be added to all existing patches
- [x] KPBS_Scale.cfg scope: rescaleFactor ONLY
- [x] Existing patches scope: everything else (mass, cost, volume, resources, converters)
- [x] Conversion rate tables compiled (×4 except greenhouses UNCHANGED)
- [x] Resource storage ×8 table compiled
- [x] TweakScale: ALL PlanetaryBase parts, `type = stack`, `defaultScale = 2.5`
- [x] Deprecated legacy parts: SKIP
- [x] Docking ports: size2 + description append
- [x] Landing engine thrust: ×4
- [x] Heat shield: ablator ×4, other params unchanged
- [x] Garage parts: confirmed ×2
- [x] Custom Armor parts: confirmed ×2
- [x] Corridor clones: preserved as-is
- [ ] Non-standard parts (EL/MKS/USI): **CONFIRMED — all unchanged** (RecyclePercent, EfficiencyMultiplier, ProductivityFactor, WOLF all skipped; user does not have these mods installed)
- [x] 28 production parts converter rate CFG changes: **CONFIRMED** — Mining/ISRU/Reactor/FuelCell/LS/OES/MKS ×4, Greenhouses UNCHANGED, Non-standard skipped
