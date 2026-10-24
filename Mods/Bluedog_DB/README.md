# BDB patches by part type

Current propulsion policy (Helium removal, fixed mixtures, Apollo/LM RCS choices): see `PropulsionPolicy.md`.

Reorganized 2026-09-06 following `Mods/NF-Spacecraft`. Locate the host part's type first: its size, tanks, internal RCS, engine modules, B9 compatibility, TweakScale and local Waterfall corrections are together in that file. An internal RCS system belongs with its host pod, tank or stage, not in the standalone RCS file.

| File | Parts |
| --- | --- |
| CommandPod.cfg | Crewed command/landing pods, laboratories, habitation and crewed station modules |
| UncrewedCommandPod.cfg | Probe cores, avionics and uncrewed return capsules |
| ServiceModule.cfg | Spacecraft service/equipment modules |
| UpperStage.cfg | Integrated propulsion stages: Athena OAM, Peacekeeper PBV, Minuteman PSRE, Titan Transtage |
| FuelTank.cfg | Propellant tanks, including their built-in systems |
| Engines.cfg | Liquid and nuclear engines |
| SolidRocketMotor.cfg | Solid motors, separation motors and launch escape systems |
| RCS.cfg | Independent attitude-control parts |
| Structural.cfg | Adapters, engine mounts, trusses and structural nodes |
| Coupling.cfg | Decouplers, docking ports and interstages |
| Payload.cfg | Fairings, cargo carriers and payload bays |
| Thermal.cfg | Heatshields and radiators |
| Electrical.cfg | Batteries, solar panels and generators |
| Antenna.cfg | Communication antennas |
| Science.cfg | Instruments and deployed experiments |
| Aerodynamics.cfg | Fins, noses and aerodynamic surfaces |
| Ground.cfg | Rovers, wheels and landing legs |
| Robotics.cfg | Hinges and sliders |
| Utility.cfg | Lights, ladders, seats, parachutes and decals |
| Resources.cfg | Shared RF resource/tank definitions; not associated with a single PART |
| UpgradeIcons.cfg | Hidden upgrade icon PARTs retained for compatibility |

VABOrganizer remains centrally maintained in `Mods/VABO/Bluedog_DB.cfg`; RemoteTech remains in `Mods/RemoteTech/Bluedog_DBRT.cfg`, following the AO project-wide convention.

## Refactor validation

The notes below describe the initial layout-only refactor. The subsequent fixed-capacity correction is documented in `FixedTankVolumes.md`: runtime tank calculations were removed and replaced by explicit per-part numeric values, with BDB-only exclusions from AO global conversion.

- No numerical values, patch bodies, HAS guards, NEEDS dependencies or passes were changed.
- Mixed-type selectors were split into explicit names by the host part's role. The ordered patch-body/guard sequence was compared for all 665 retained affected PART names (including hidden icons and four names from compatibility/other-mod files previously matched by BDB-wide patches).
- Existing parameters have not been approved or recalibrated by this refactor.
- New BDB parts require explicit inclusion in the appropriate type file; there is no automatic wildcard inclusion.
- 24 original blocks have no currently installed target. They remain recoverable in the archived source files rather than active configurations.
- The 21 original numbered files were moved, with SHA256 verification, to `Bluedog_DB/delete/RetiredAOPatches/BeforePartTypeLayout`, suffixed `.disabled`. The user may manually delete that archive. Do not reactivate old and new layouts together.
- Existing audit reports describe the pre-refactor layout and retain historical filenames. Spreadsheet artifacts were not changed.
- Static equivalence validation does not replace a future KSP load; KSP was not launched for this refactor.
