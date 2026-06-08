---
name: vabo-engine-categories
description: VABO/VABOrganizer engine subcategory assignments — which multi-fuel engines were moved to MultipropellantEngines and where MK2 Expansion engines were classified (2026-06-07)
metadata:
  type: project
---

# VABO Engine Subcategory Classification

## Category system

VABO defines 6 custom subcategories (in `a_general_settings.cfg`): KeroloxEngines, MethaloxEngines, MultipropellantEngines, HydroloxEngines, HypergolicEngines, TripropellantEngines.

VABOrganizer defines 8 base subcategories (in `VABOrganizer/Subcategories/engines.cfg`): lfoEngines, cryogenicEngines, solidEngines, nuclearEngines, jetEngines, propEngines, monoEngines, ionEngines.

## Classification rule

Engines that support switching between fundamentally different fuel types (e.g. Kerolox ↔ Hypergolic, Hydrolox ↔ Methalox) belong in MultipropellantEngines. Engines switching only within the same fuel family (e.g. MMH/NTO ↔ UDMH/NTO) stay in their original category.

## What was moved to MultipropellantEngines (2026-06-07)

### stock.cfg — SquadPartsOverhaul (5 engines)
liquidEngine2_v2 (Swivel), liquidEngine3_v2 (Terrier), liquidEngineMainsail_v2 (Mainsail), liquidEngine_v2 (Reliant), radialLiquidEngine1-2 (Thud)
- Swivel/Terrier/Mainsail were KeroloxEngines → moved to MultipropellantEngines
- Reliant/Thud were HypergolicEngines → moved to MultipropellantEngines

### NF-LaunchVehicle.cfg (6 engines)
nflv-engine-ar1-1 (Walrus), nflv-engine-ar1c-1 (Manatee), nflv-engine-m1d-1 (Otter), nflv-engine-m1d-vac-1 (Sphinx), nflv-engine-rd701-1 (Cougar), nflv-engine-rs84-1 (Ocelot)
- All 6 were KeroloxEngines → moved to MultipropellantEngines

### NovaPunch.cfg (6 engines)
NP_lfe_25m_BroncoSingle (Bearcat KD-270), NP_lfe_375m_Bearcat3x, NP_lfe_5m_Bearcat5x, NP_lfe_25m_4X800Engine, NP_lfe_25m_Orbitalbertha, NP_lfe_125m_AerospikeEngine (LF-A30)
- Bearcat×3 and 4X800 were HypergolicEngines → moved to MultipropellantEngines
- OrbitalBertha was KeroloxEngines → moved to MultipropellantEngines
- Aerospike was HydroloxEngines → moved to MultipropellantEngines

### VanStockRevamp.cfg (1 engine)
LVT15 (Dachshund)
- Was KeroloxEngines → moved to MultipropellantEngines

### MK2Expansion.cfg (19 engines — newly classified)
Previously had NO engine classifications. Added:
- jetEngines: TurbofanMk2, Ramjet, HeavyVTOL, Pegasus, Siddeley, Jumpjet (6)
- solidEngines: RadialAASRB, RATO (2)
- nuclearEngines: AtomicJet, Pluto (2)
- ionEngines: IonEngine (1)
- KeroloxEngines: AugmentedRocket (1)
- MethaloxEngines: ESTOCv2, MATTOCKv2, RocketEngine (3)
- MultipropellantEngines: LinearAerospike, FuselageRVTOLE, RocketEngineAtmo (3)
- HypergolicEngines: OMSpod (1)

## Engines deliberately NOT changed

- Nuclear engines with propellant variants (LV-N, NERVA, Shiba, Nova, Pluto): stay in nuclearEngines
- Hypergolic variant switchers (Puff, Ant, Fire Ant, RMA-3, Stationkeeper, Twitch, Spark, Bertha Mini Quad, AJ10, K2-X/U series): stay in HypergolicEngines/monoEngines
- LE-25 "Corgi" and RE-M7 "Pila": user has separate plans, excluded from MultipropellantEngines
- VTOL engines (HeavyVTOL, Pegasus, Siddeley, Jumpjet): use LiquidFuel+IntakeAir in base Mk2Expansion, classified as jetEngines

## VABOrganizer patching pattern

VABO uses per-mod config files in `Mods/VABO/`. Each file groups parts by subcategory using the pattern:
```
@PART[part1|part2|...]:NEEDS[VABOrganizer]:Final
{
    %VABORGANIZER
    {
        %organizerSubcategory = SubcategoryName
    }
}
```
This is separate from KSP's `%category` field — VABOrganizer reads `organizerSubcategory` to sort within the editor.

## Related files

- [[vabo-category-definitions]] — subcategory definitions in a_general_settings.cfg
