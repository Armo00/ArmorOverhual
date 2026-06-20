---
name: battery-rules
description: Battery part balancing rules — energy density, price, and annotation conventions (2026-06-19)
metadata:
  type: project
---

# Battery Rules

**Ground rules for all battery part adjustments.**

## Unit Conversions

| Game Unit | Real Equivalent |
|-----------|----------------|
| 1 EC | 1 Wh (watt-hour) |
| 1 cost (funds) | $1,000 USD |

## Energy Density

- Range: **50–150 Wh/kg**
- Larger capacity → higher energy density (better packaging efficiency at scale)
- Smallest battery (~1 kWh): 50 Wh/kg
- Largest battery (~150 kWh): 150 Wh/kg
- Formula: `mass (tons) = EC / (energy_density × 1000)`

## Price

- Range: **$500–2,000 /kWh**
- Larger capacity → lower price per kWh (economy of scale)
- Smallest battery (~1 kWh): $2,000/kWh
- Largest battery (~150 kWh): $500/kWh
- Formula: `cost = EC × price_per_kWh / 1,000,000`

## Annotation Convention

Every battery patch MUST annotate in comments:
- Energy density: `// XXX Wh/kg`
- Price: `// $XXX/kWh`

Example:
```
// Z-1U Rechargeable Battery Pack 0.010t
@PART[batteryPack]:Final
{
    @cost = 2   // 100 Wh/kg, $2000/kWh
    @mass = 0.010
}
```

## Reference Table

| Battery | EC (Wh) | Wh/kg | $/kWh | Mass (t) | Cost |
|---------|--------:|------:|------:|---------:|-----:|
| Z-1U | 1,000 | 50 | 2,000 | 0.020 | 2 |
| Z-2U | 2,100 | 65 | 1,700 | 0.032 | 3.6 |
| Z-4U | 4,400 | 85 | 1,400 | 0.052 | 6.2 |
| Z-12U | 12,000 | 110 | 900 | 0.109 | 10.8 |
| Z-50U | 50,000 | 150 | 500 | 0.333 | 25 |
| Z-150U (KPBS) | 150,000 | 150 | 500 | 1.000 | 75 |
