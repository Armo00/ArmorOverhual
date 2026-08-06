---
name: nuclear-reactor-rules
description: Nuclear reactor balancing rules — efficiency range, 19% HALEU fuel consumption formula, and annotation conventions (updated 2026-08-06)
metadata:
  type: project
---

# Nuclear Reactor Rules

## Efficiency

- Range: **0.216 – 0.432** (electrical power / thermal power)
- Larger reactor → higher efficiency (better thermodynamic cycle at scale)
- Efficiency = `ElectricalGeneration(kW) / HeatGeneration(kW)`
- EC/s → kW conversion: 1 EC/s = 3.6 kW (see [[resource-energy-rules]])

### Reference (full fleet, 2026-08-06)

| Reactor | Thermal kW | Electrical | Efficiency | Density kW/t |
|---------|-----------:|-----------:|:----------:|-------------:|
| nfe-reactor-tiny-1 | 120 | 8 EC/s (28.8 kW) | 0.240 | 2,143 |
| nfe-reactor-tiny-2 | 260 | 18 EC/s (64.8 kW) | 0.249 | 1,548 |
| nfe-reactor-0625-1 | 840 | 65 EC/s (234 kW) | 0.279 | 1,265 |
| nfe-reactor-125-1 | 2,800 | 241 EC/s (867.6 kW) | 0.310 | 1,275 |
| nfe-reactor-1875-1 | 7,500 | 708 EC/s (2,548.8 kW) | 0.340 | 1,117 |
| nfe-reactor-25-1 | 24,000 | 2,467 EC/s (8,881.2 kW) | 0.370 | 1,200 |
| nfe-reactor-25-2 | 32,000 | 3,556 EC/s (12,801.6 kW) | 0.400 | 1,120 |
| nfe-reactor-375-1 | 52,000 | 6,240 EC/s (22,464 kW) | 0.432 | 1,196 |
| KKAOSS_Nuclear_Reactor (KPBS) | 2,000 | 200 EC/s (720 kW) | 0.360 | 133 |
| M2X_Reactor (MK2E) | 800 | 80 EC/s (288 kW) | 0.360 | 200 |

## Power Density (kW/t)

- **NFE 航天堆（Stacked）: 1,100 – 1,300 kW/t**（8 堆定档 1,117–2,143，主体收敛 1,100–1,300；tiny 例外偏轻）
- **KPBS 基地级: ~133 kW/t**（地面/基地堆不需要高密度，15t/2MW）
- **MK2E 航天紧凑堆: ~200 kW/t**（KPBS 的 1.5 倍，"略高一点点"——集成散热 mk2 机身）
- 原则：航天堆 > 基地堆；小堆允许偏高密度（tiny），大堆收敛 1,100–1,300

## Cost Baseline (kW/cost)

- **NFE: 0.009 – 0.072 kW/cost**，随尺寸递增（规模化效应：大堆单位成本更低）
- **KPBS: 0.01 kW/cost**（cost = 热kW / 0.01）
- **MK2E: 0.005 kW/cost**（集成散热 + mk2 机身的溢价，为 KPBS 的一半）
- 公式：`cost = Thermal_kW / target_kW_per_cost`

## Core Load & Lifetime

- **tiny（≤260 kW）: ≥30 年**（1.5–3 u）
- **常规堆（840 kW+）: 10–15 年**（4–240 u）
- 装载单位换算：1 u = 10.97 kg

## Radiator Integration

- 集成散热器散热能力 = 反应堆热功率（@ 800K）
- temperatureCurve 按辐射 T⁴ 定档：**400K = 1/16、600K = 5/16、800K = 1（满功率）**（即 400:600:800 = 1:5:16；600K 取 ×5 近似 5.06）
- 例（MK2E 800 kW 堆）：`key = 0 0 / key = 400 50 / key = 600 253 / key = 800 800`

## Fuel

All reactors use **19% enriched uranium (HALEU) fuel rods** (changed from 3% on 2026-08-06 — space reactors have no 3% precedent; historical space reactors used HEU 90%+, modern NASA/DOE lunar designs use 19.75% HALEU).

### Physical Basis

- 1 g U-235 complete fission ≈ 81,640 MJ = **0.945 MWd** thermal (198.9 MeV usable per fission)
- ~50% of U-235 in rod actually fissions (rest lost to neutron capture / transmutation)
- 1 MWd thermal needs 1 / 0.945 × 2 = **2.12 g U-235**
- Usable U-235 per gram of 19% fuel rod = 0.19 × 0.5 = 0.095 g
- Daily rod consumption per MW thermal = 2.12 / 0.19 = **11.14 g/day per MW** (exact; simplified value using "1 g ≈ 1 MWd" would be 10.5, do not use)

### Formula

```
daily_grams = Thermal_MW × 11.14
daily_units = Thermal_MW × 11.14 / 10970 = Thermal_MW × 1.015×10⁻³
Ratio(unit/s) = Thermal_MW × 1.015×10⁻³ / 86400 = Thermal_MW × 1.175×10⁻⁸
```

### Resource

| Property | Value | Source |
|----------|-------|--------|
| Resource | EnrichedUranium | CRP |
| Density | 0.01097 t/L = 10.97 kg/unit | `CommonResources.cfg:604` |
| Waste product | DepletedFuel | same density |
| Ratio IN = Ratio OUT | mass conserved | — |

### Annotation Convention

Every reactor patch MUST annotate:
- Thermal power, electrical power, efficiency
- Fuel consumption (g/day) at 100% throttle
- Core load and runtime

Example (KPBS baseline):
```
// 2 MW thermal, 720 kW electrical, 36% efficiency
// Fuel: 19% HALEU rods, ~22 g/day at 2 MW
// Core: 12 unit = 131.6 kg, ~16.2 years runtime
```

## Patch Convention (SystemHeat 模块，2026-08-06 批次教训)

- **模块整体重写**：`!MODULE[ModuleSystemHeatFissionReactor] {}` + 全新 `MODULE`（完整字段，key 直接写入）——**不要用 `@key[N, ]` 选择器**（实测匹配不可靠，会污染 key 行）
- **INPUT/OUTPUT_RESOURCE 选择器**：NFE 原版用 `ResourceName =`（非 `name =`），修改须 `@INPUT_RESOURCE:HAS[#ResourceName[EnrichedUranium]]` 或整体重写
- **赋值一行一个**：绝不在同一行写多个 `@field = value`（会被 MM 拼进同一值导致 FormatException，part 编译失败）
- 每个 patch 块注释：热功率 / 电功率 / 效率 / 日耗 / 装载 / 寿命（见 Annotation Convention）
- 散热器单独成块（ModuleSystemHeatRadiator），curve 按 Radiator Integration 规则

### Related

- [[resource-energy-rules]] — resource density and EC power conversion
