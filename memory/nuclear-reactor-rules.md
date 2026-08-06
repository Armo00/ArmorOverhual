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

### Reference

| Reactor | Thermal | Electrical | Efficiency |
|---------|--------:|-----------:|:----------:|
| KKAOSS_Nuclear_Reactor | 2,000 kW | 720 kW | 0.36 |

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

### Related

- [[resource-energy-rules]] — resource density and EC power conversion
