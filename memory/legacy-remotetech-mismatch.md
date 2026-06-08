---
name: legacy-remotetech-mismatch
description: RemoteTech patches were reviewed and aligned with official RT conventions on 2026-06-07
metadata:
  type: project
---

The RemoteTech patches in this project were thoroughly audited and aligned with the official RemoteTech conventions (from `GameData/RemoteTech/RemoteTech_Squad_*.cfg`) on 2026-06-07.

Changes applied:
- **Command pods/probes**: Replaced `ModuleRTAntenna` (active) with `ModuleRTAntennaPassive` (passive, OmniRange) — 11 files, 30 parts
- **Standalone antennas**: Added `!MODULE[ModuleDataTransmitter]{}` deletion — 7 files, 39 parts
- **Bugs fixed**: Duplicate ModuleSPU in NF-SpacecaftRT.cfg, trailing pipe syntax in NovaPunchRT.cfg
- **Missing parts**: Added mk1pod, mk1pod_v2, cupola, mk1Cockpit, mk1InlineCockpit, landerCabinSmall, mk2LanderCabin, mk2LanderCabin_v2ts to Squad commandPod.cfg

**How to apply:** New RemoteTech patches should follow the conventions documented in [[CLAUDE.md]].
