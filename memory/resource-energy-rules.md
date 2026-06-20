---
name: resource-energy-rules
description: Resource definition reference, density units, and EC power conversion rules (2026-06-19)
metadata:
  type: reference
---

# Resource & Energy Rules

## Resource Definitions

All CRP (Community Resource Pack) resource definitions are at:

`D:\KSP\KSP_1.12.3\TestRun\GameData\CommunityResourcePack\CommonResources.cfg`

### Density

- `density` field in `RESOURCE_DEFINITION` is in **tons per liter (t/L)**
- KSP internally uses metric tons for mass and liters for volume
- Examples:
  - Water: 0.001 t/L (1 kg/L)
  - LqdHydrogen: 0.00007085 t/L (0.07085 kg/L)
  - LqdOxygen: 0.001141 t/L (1.141 kg/L)
  - EnrichedUranium: 0.01097 t/L
  - Plutonium-238: 0.019816 t/L

### MFT Volume Calculation

When a resource is stored via `ModuleFuelTanks` with `utilization` from `TANK_DEFINITION`:

`volume_used (L) = maxAmount / utilization`

For resources without explicit TANK entries, utilization defaults to 1.

## Electric Charge (EC)

| Concept | Value |
|---------|-------|
| 1 EC | 1 Wh (watt-hour) |
| 1 EC/s | 3,600 W (3600 watt) |

### Derived Conversions

- 1 kW = 1/3.6 EC/s ≈ 0.278 EC/s
- 1 MW = 1,000/3.6 EC/s ≈ 277.8 EC/s
- Converter `Ratio` in KSP is per-second: a Ratio of N consumes/produces N units per second
  - EC Ratio = 5 means 5 EC/s = 18,000 W = 18 kW

### Related

- [[battery-rules]] — battery energy density and pricing
