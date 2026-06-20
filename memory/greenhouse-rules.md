---
name: greenhouse-rules
description: Greenhouse power consumption rules — per-person power range, scale economy, and annotation conventions (2026-06-20)
metadata:
  type: project
---

# Greenhouse Rules

## Power Consumption

- Range: **12–20 kW per person** (electrical power for lighting, climate control, nutrient circulation)
- Larger greenhouse (more crew supported) → lower per-person power (economy of scale)
- Power in KSP: `EC_Ratio × 3.6 = kW` (see [[resource-energy-rules]])

### Reference

| Greenhouse | Crew | Total kW | kW/person |
|-----------|:----:|--------:|:---------:|
| KKAOSS_LS_container_Armor_greenhouse | 3 | 151.2 | 50.4 |

*Note: 151.2 kW was set as a baseline for this specific part; the 12–20 kW/person range will be applied in batch adjustments.*

## Converter EC Ratio

The `INPUT_RESOURCE[ElectricCharge].Ratio` in greenhouse converters is in EC/s:

```
Ratio = kW / 3.6
```

Example: 151.2 kW → Ratio = 151.2 / 3.6 = 42 EC/s

## Batch Adjustment Priority

- [ ] Audit all greenhouse parts across mods for current EC Ratio
- [ ] Set per-person power within 12–20 kW range (larger → lower end)
- [ ] Update descriptions with kW and kW/person ratings

### Related

- [[resource-energy-rules]] — 1 EC/s = 3.6 kW
