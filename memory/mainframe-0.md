---
name: mainframe-0
description: Mainframe-0 machine identity — the only machine that knows the exact GameData path
metadata:
  type: reference
---

# Mainframe-0

This machine is designated **Mainframe-0**. It is the primary workstation for the ArmorOverhual project.

## Paths (only valid on Mainframe-0)

| Path | Description |
|------|-------------|
| `d:\KSP\KSP_1.12.3\TestRun\GameData` | KSP GameData root |
| `d:\KSP\KSP_1.12.3\TestRun\GameData\zzzArmorOverhual` | Project working directory |
| `d:\KSP\KSP_1.12.3\TestRun\GameData\Squad` | Stock Squad parts |
| `d:\KSP\KSP_1.12.3\TestRun\GameData\VABOrganizer` | VABOrganizer base configs |

## Why

Only Mainframe-0 has the full KSP 1.12.3 TestRun installation with all 50+ mods. Paths like the GameData location are specific to this machine and should not be hardcoded in scripts or patches that might run elsewhere.
