---
name: nuclear-reactor-rules
description: Nuclear reactor balancing rules — efficiency range, 3% enriched fuel consumption formula, and annotation conventions (2026-06-20)
metadata:
  type: project
---

# Nuclear Reactor Rules

## Efficiency

- Range: **0.216 – 0.432** (electrical power / thermal power)
- Larger reactor → higher efficiency (better thermodynamic cycle at scale)
- Efficiency = `ElectricalGeneration(kW) / HeatGeneration(kW)`

### Reference

| Reactor | Thermal | Electrical | Efficiency |
|---------|--------:|-----------:|:----------:|
| KKAOSS_Nuclear_Reactor | 2,000 kW | 720 kW | 0.36 |

## Fuel

All reactors use **3% enriched uranium fuel rods**.

### Physical Basis

- 1 g U-235 complete fission ≈ 1 MWd thermal (82,100 MJ)
- ~50% of U-235 in rod actually fissions (rest lost to neutron capture / transmutation)
- Usable U-235 per gram of 3% fuel rod = 0.03 × 0.5 = 0.015 g
- Daily rod consumption per MW thermal = 1 / 0.015 = **66.7 g/day per MW**

### Formula

```
daily_grams = Thermal_MW × 66.7
daily_units = Thermal_MW × 66.7 / 10970 = Thermal_MW × 0.00608
Ratio(unit/s) = Thermal_MW × 0.00608 / 86400 = Thermal_MW × 7.04×10⁻⁸
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

Example:
```
// 2 MW thermal, 720 kW electrical, 36% efficiency
// Fuel: 3% enriched rods, ~133 g/day at 2 MW
// Core: 50 unit = 5,485 kg, ~10.7 years runtime
```

### Related

- [[resource-energy-rules]] — resource density and EC power conversion
