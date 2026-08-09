---
name: greenhouse-rules
description: Greenhouse power consumption rules — 12-20 kW/person with linear scale economy, EC Ratio convention, and part registry (2026-08-09)
metadata:
  type: project
---

# Greenhouse Rules

## Power Consumption

- Range: **12–20 kW per person** (electrical power for lighting, climate control, nutrient circulation)
- Linear interpolation by crew count: **kW/person = 20 − (8/14) × (n−1)** (1 crew → 20, 15 crew → 12)
- Power in KSP: `EC_Ratio × 3.6 = kW` (converter `INPUT_RESOURCE[ElectricCharge].Ratio` is EC/s)
- Example: 147.6 kW → Ratio = 147.6 / 3.6 = 41 EC/s

## Part Registry (2026-08-09, all adjusted)

| Part | Crew | Total kW | EC/s (base) | kW/person |
|------|:----:|--------:|:-----------:|:---------:|
| sspx-greenhouse-5-1 | 15 | 180 | 3.33 | 12.0 |
| KKAOSS_Greenhouse_g | 10 | 147.6 | 41 | 14.8 |
| sspx-greenhouse-375-1 | 8 | 128 | 4.44 | 16.0 |
| sspx-dome-greenhouse-5-1 | 8 | 128 | 4.44 | 16.0 |
| sspx-greenhouse-25-1 | 4 | 73.2 | 5.08 | 18.3 |
| KKAOSS_LS_container_Armor_greenhouse | 3 | 56.6 | 15.7 | 18.9 |
| sspx-cupola-greenhouse-125-1 | 1 | 20 | 5.56 | 20.0 |
| phoenixgreenhouse (Snacks) | 1 | 20 | 5.56 | 20.0 |
| sspx-aquaculture-375-1 (辅助) | 3.2 | 0.96 | 0.267 | — (不动) |

## SSPXR Specifics

- `greenhouse-1.cfg` 中每个零件用 `@MODULE[TacGenericConverter],0`（index 0 = 温室本体，无KPBS时为 SETI 温室、有KPBS时为 K&K 温室）覆盖 EC base；index 1 是 Mineral Siphon（EC 2.0，不动）
- 无 KPBS / 有 KPBS 两套 base Ratio **统一为同一数值**（不再有 6.0/1.5 差异）
- 实际 EC = base × conversionRate（conversionRate = 产能人数）
- 矿藏虹吸 (Mineral Siphon, +2.0 EC/s) 与 aquaculture 不属温室本体，不按此规则

## Related

- [[resource-energy-rules]] — 1 EC/s = 3.6 kW
- VABO: 所有温室归 `greenhouses` 子分类（KPBS 覆盖原 base_component）
