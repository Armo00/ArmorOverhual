---
name: craft-report
description: |
  Generate a detailed technical report from a KSP .craft file. Use this skill whenever the user asks to analyze a spacecraft, generate a craft report, document a vessel's technical parameters, or understand what parts/fuel/engines are in a saved vessel. Triggers on mentions of .craft files, vessel analysis, spacecraft reports, "analyze this craft", or similar requests involving KSP save files. Even if the user doesn't explicitly say "report", if they're asking for technical details about a specific .craft file, use this skill.
---

# KSP Craft File Technical Report Generator

Generate a detailed, human-readable technical report from a Kerbal Space Program `.craft` file. The report documents all parts, resources, staging, engines, and modifications applied by the ArmorOverhual project.

## Core Process

### Phase 1: Read the Craft File

1. **Read the craft header**: extract ship name, version, type (VAB/SPH), vessel type, size, and mod list (`_modVersions`)
2. **Extract all parts**: `grep "part = "` to get every part with instance IDs, then strip IDs to get unique part basenames with counts
3. **Extract staging**: for each part, capture `istg` (stage it belongs to) and `dstg` (stage it decouples in)
4. **Extract connections**: `attN` lines show parent-child relationships — use these to trace the vessel topology
5. **Extract all RESOURCE blocks**: resource name, amount, maxAmount, linked to their parent part
6. **Extract all ModuleFuelTanks blocks**: volume, type, utilization, tank contents with amounts
7. **Extract all ModuleEngineConfigs**: configuration name, techLevel, any CONFIG sub-nodes
8. **Extract TweakScale modules**: currentScale and defaultScale for every part that has them
9. **Extract action groups**: grep for `actionGroup = ` excluding `None` to find Custom01-CustomXX, Light, Gear, Brakes, Abort assignments
10. **Extract Smart Parts**: look for `km.smart.time` or other SmartParts modules — capture timer delays (`triggerDelaySeconds`, `triggerDelayMinutes`), trigger conditions (`allowStage`), and target action groups (`group`)

### Phase 2: Cross-Reference with GameData

For each unique part basename, find the original configuration in `GameData/`:

1. Search for `name = <partName>` in `.cfg` files under the relevant mod directory
2. Record: mass, crewCapacity, default RESOURCE blocks, default MODULE blocks, bulkheadProfiles, TechRequired
3. For engines: record maxThrust, atmosphereCurve keys (ISP at vacuum and sea level), PROPELLANT ratios, gimbal range
4. Use background agents to fan out searches across multiple mod directories simultaneously

**Key mod directories to search** (adapt based on part name prefixes):
- `KKAOSS_*` → `GameData/PlanetaryBaseInc/`
- `sspx-*` → `GameData/StationPartsExpansionRedux/`
- `nfex-*` → `GameData/NearFutureExploration/`
- `cryoengine-*` → `GameData/CryoEngines/`
- `SEP_*` → `GameData/StarshipExpansionProject/`
- `KK_SpaceX*` → `GameData/Launchers Pack/`
- `KCHS*` → `GameData/KIU/`
- `KCLV*` → `GameData/KIU/`
- `nflv-*` → `GameData/NearFutureLaunchVehicles/`
- `Squad parts` → `GameData/Squad/`
- `proceduralTank*` → `GameData/ProceduralParts/`

### Phase 3: Cross-Reference with ArmorOverhual

Check `GameData/zzzArmorOverhual/` for any patches affecting the parts:

1. Search for `@PART[<partName>]` or `+PART[<partName>]` across all AO `.cfg` files
2. For each match, read the full patch to understand what changed:
   - Mass adjustments (`@mass`)
   - RealFuels conversion (`ModuleEnginesRF`, `ModuleEngineConfigs`)
   - ModuleFuelTanks configuration
   - TACLS life support integration
   - SystemHeat conversion
   - RemoteTech integration
   - TweakScale additions
   - Cost/entryCost changes
3. Note the AO modification date from the file header comment

### Phase 4: Understand the Vessel Architecture

Before writing, build a mental model:

1. **Trace the root part** (first PART in the file) and follow attN connections to build a tree
2. **Group parts by stage** (istg) to understand the flight sequence
3. **Identify functional zones** for surface bases, or **stage separation points** for rockets
4. **Calculate totals**: sum all propellant quantities by type, sum engine thrust by stage
5. **Cross-check staging with Smart Parts and action groups** — the KSP stage numbers alone don't tell the full story. Smart timers may trigger events mid-stage, and action groups may control subsystems independent of staging

### Phase 5: Report Conventions

**EC energy units**: Per ArmorOverhual project convention (`memory/battery-rules.md`), **1 EC = 1 Wh**, so **1 EC/s = 3.6 kW**.

**Propellant naming**: Use the RealFuels resource names as they appear in the craft file (LqdMethane, LqdOxygen, LqdHydrogen, Hydrazine, etc.).

**TweakScale**: The `currentScale` value interpretation depends on `type`:
- `free` type: value is a percentage (100 = 1×, 200 = 2×). Area scales as scale².
- `stack` type: value is the diameter in meters (1.25, 2.5, 3.75, 5.0, 7.5, etc.)

**SystemHeat radiators**: If radiators have TweakScale, their heat rejection capacity scales with area (scale²).

**Mass values**: Use ArmorOverhual-modified mass where available; fall back to original cfg mass.

### Phase 6: Write the Report

Structure the report based on vessel type:

**For Rockets/Launch Vehicles:**
```
# [Mission Name] Technical Report
## 一、Mission Overview (architecture, flight profile, stage sequence)
## 二-N、Stage-by-Stage Breakdown (engines, tanks, propellant, dry mass, recovery, avionics)
## N+1、Propulsion Summary (all engines in a comparison table)
## N+2、Propellant Totals
## N+3、ArmorOverhual Coverage (which parts have AO configs)
```

**For Surface Bases/Stations:**
```
# [Base Name] Detailed Analysis Report
## 一、Overview (total parts, functional zones)
## 二、Complete Parts List (grouped by type with AO coverage status)
## 三、Functional Zone Breakdown (topology, each zone's modules)
## 四、System Analysis (power, thermal, life support, communications, ISRU)
## 五、Design Assessment (strengths, issues, recommendations)
## Appendix: AO Configuration File Index
```

**For each stage/zone**, include:
- Engine table: quantity, type, thrust per engine, total thrust, ISP (vac/SL), mass per engine, propellant
- Tank table: MFT volume, type, propellant quantities
- Ancillary: RCS, avionics, thermal, recovery, separation
- Key TweakScale values that differ from default

### Phase 7: Human Review Mandate

**THIS IS A HARD REQUIREMENT.** At the end of every report, append this exact block (translated to match the report language):

```
---

> ⚠️ **人工审核要求 / Human Review Required**
>
> 本报告由自动化工具基于 craft 文件和配置文件生成。以下内容**必须**经过人工核实：
> - 飞行剖面与时序（Smart Parts 和动作组事件可能未被完全解析）
> - 零件归属和级数分配（复杂挂载关系可能被误判）
> - 设计背景和任务架构（无法从文件自动推断）
> - 推进剂质量估算（依赖密度假设）
>
> **请在确认所有数据准确后，删除本提示块。**
```

## Important Reminders

- **Part names in craft files use dots** (e.g., `KKAOSS.Central.Hub`) while **cfg files use underscores** (`KKAOSS_Central_Hub`). Both refer to the same part — the dot form is KSP's internal save format.
- **TweakScale affects radiator performance**: `currentScale=200` on a `free` type radiator means 2× linear dimension → 4× area → 4× heat rejection.
- **Smart Parts timers** override pure stage-based sequencing — always check for `km.smart.time` or similar modules.
- **Always read the full craft file** — don't assume architecture from part names alone. Use `attN` connections to verify.
- **Use background agents** for the cross-referencing phases (GameData search and AO search) to speed up the process.
- **Propellant quantities** are in RealFuels units (liters for MFT volume, dimensionless units for RESOURCE amounts). Provide both the raw unit values and estimated mass where helpful.
