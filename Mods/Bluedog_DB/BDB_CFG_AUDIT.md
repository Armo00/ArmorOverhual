# BDB CFG Audit — 2026-09-01

## This pass

- Every AO `bluedog_*` wildcard in RealScale, RF tanks/engines/RCS, TweakScale, VABOrganizer, and RemoteTech excludes Gemini, Gemini Ferry, Big G, Shuguang, Mercury, Atlas/Atlas V, Redstone, Sergeant, and X-15.
- Removed the Atlas II term from the shared Atlas/Centaur Waterfall RCS patch; Centaur-only entries remain.
- Directly removed the Atlas V 500 fairing PART cfg. Its `.mu` is retained because `Parts/SAF_Fairings/bluedog_LDC_3p5mFairingBase.cfg` references the same `bluedog_AtlasV_500Fairing_SAF` model. The obsolete AO PART-hide patch was deleted.

## Exact removals in this pass

- `Spaces/MOL/bdb_mol_iva_mercury*` (3 files), `FX_Gemini_Service_A.mu`, `Parts/Apollo/bluedog_BigG_LES_Tower.mu`, `Sounds/GeminiWhoop.ogg`, and `RSMP/.../Launch Escape System/bigG LES.cfg`.
- 20 confirmed Gemini/Big G/Atlas thumbnail PNGs under `Parts/@thumbs`.
- 50 Gemini/Gemini Ferry MOL assets/configs, 11 Gemini/Big G MOL IVA assets, Titan II Gemini decoupler files, Atlas launch clamp assets, BDB Gemini contract, 34 series-specific compatibility cfgs, and 6 RSMP cfgs removed during the preceding cleanup pass.
- FilterExtensions UI icons remain in its directory. They are not part assets: `BDBFEredstone` is still used by the mixed Vanguard/Redstone/Jupiter category, and `BDBFEgemini` by the Mercury/Gemini/MOL category, whose MOL folder still contains retained hardware. They must not be deleted without first splitting or removing those categories. The Atlas V 500 filter-list token was removed; generic Vanguard/Jupiter/Centaur category content was retained.

## Engine review

| Selector / part | Result | Reference status |
|---|---|---|
| Able | Moved out of kerolox. `AJ10-42`: UDMH/IWFNA, pressure-fed, 33.0 kN, 267/122 s, 1 ignition. | `AJ10_Early_Config.cfg` |
| M-1 | Moved to hydrolox. `M-1-Spec`: LH2/LOX 0.7631/0.2369, 5337.866 kN, 428/306 s, 2 ignitions. | `M1_Config.cfg` |
| Vanguard GE-405 | Moved to kerolox. `X-405`: RP-1/LOX 0.4003/0.5997, 135.28 kN, 278/254 s, 1 ignition. | `X405_Config.cfg` |
| Vega GE-405H | Moved to kerolox. `X-405H`: RP-1/LOX/HTP, 150.5 kN, 318.68/213.03 s, 1 ignition. | `X405_Config.cfg` |
| Kerolox | E-1, F-1, H-1, Thor LR79/LR101, Jupiter vernier, GE-405, and GE-405H use individual MECs; the generic Kerolox selector no longer covers them. | E1_Config.cfg, F1_Config.cfg, H1_Config.cfg, LR79_Config.cfg, LR101_Config.cfg, X405_Config.cfg |
| Cryogenic / G-1 / Timberwind batch | RL10, RS-68, G-1, LR87-LH2, MB-60, RL-20, RS-30, J-2/J-2T, STNP-75, and XLR-129 now have per-part MECs. G-1 is hydrazine/fluorine and STNP-75 is LH2 NTR, not hydrolox. | `RL10_Config.cfg`, `RS68_Config.cfg`, `G1_Config.cfg`, `LR87LH2_Config.cfg`, `MB60_Config.cfg`, `RL20_Config.cfg`, `RS30_Config.cfg`, `J2_Config.cfg`, `J2T_Config.cfg`, `LR129_Config.cfg`, `RO_Timberwind.cfg` |
| Agena XLR81 | Independent XLR81-BA-5, -BA-7, -BA-11, and Model8096-39 MECs; UDMH/IRFNA and historical thrust/Isp/restarts. | Agena_XLR81_Config.cfg |
| AJ10, Apollo, LM, Titan storable batch | Independent historical MECs now replace the former shared Aerozine50/NTO block. | See current implementation status below. |
| Solid and NTR groups | HTPB and LH2 family assignments respectively. | Retain source thrust/Isp; **not historical-performance complete** |

The storable-engine batch below is historical-performance calibrated. Other broad
families remain incomplete; this document does not treat source stock values as
real-data validation.

## Kerolox coverage

Eight retained Kerolox-route engine parts are calibrated: bluedog_E1_engine,
bluedog_Saturn_Engine_F1, bluedog_Saturn_Engine_H1, bluedog_Thor_LR79,
bluedog_Thor_LR101, bluedog_Jupiter_Vernier, bluedog_Vanguard_GE405, and
bluedog_Vega_GE405H. The first six are in
22_BDB_RealFuels_KeroloxEngines.cfg; the X-405 pair remain in their existing
individually calibrated blocks in 20_BDB_RealFuels_Engines.cfg.

bluedog_Juno4_Engine_6K and bluedog_Juno4_Engine_45K have stock LFO modules
but are not Kerolox in the local RO reference: their actual MECs are
hydrazine/NTO or hydrazine/ClF3. The Thor LR79 stock B9 RS-27 subtypes are not
separate retained PARTs and the local BDB-RO patch removes that engine switch;
no unsupported RS-27 MEC has been guessed.

## Current cryogenic / fluorine / nuclear implementation

- `bluedog_CentaurD_RL10`: RL10A-1, RL10A-3-3, RL10A-4, and RL10A-4-2N;
  `bluedog_DeltaIV_RS68`: RS-68, -68A, -68B, and -68K — `RL10_Config.cfg`,
  `RS68_Config.cfg`.
- `bluedog_G1`: G-1 and G-1A use Hydrazine/LqdFluorine/Helium, are
  pressure-fed, and intentionally do **not** use Hydrolox — `G1_Config.cfg`.
- `bluedog_LR87_LH2_Single`: Titan C, Vacuum, Sustainer Upgrade, and Vacuum
  Upgrade; `bluedog_MB60`: MB-60; `bluedog_RL20`: RL20P-3; and
  `bluedog_RS30`: RS-30 — their named local Engine_Configs files.
- `bluedog_Saturn_Engine_J2`: J-2-200K, -225K, -230K, J-2S;
  `bluedog_Saturn_Engine_J2T`: J-2T-200K and -250K — `J2_Config.cfg`,
  `J2T_Config.cfg`.
- `bluedog_STNP_75`: Timberwind-75 LH2/EnrichedUranium NTR, 130.92–654.6 kN,
  1000/890 s, 60 ignitions — `RO_Timberwind.cfg`.
- `bluedog_XLR129`: RF bimodal XLR129-P-1, LR129-P-1, and LR129-P-2 with
  retracted/extended nozzle subconfigs — `LR129_Config.cfg`. The obsolete
  stock secondary engine and its Waterfall module are removed together.
- The RO career cost/tech fields are not imported for this batch because they
  require the absent RO/RP-1 progression API. Each MEC carries `origMass` and
  `massMult`; this AO installation retains its existing cost/tech policy.

## Current NTR / Juno IV implementation

- `bluedog_NERVA_FF`: NERVA NRX Hydrogen; `bluedog_NERVA_II`: NERVA-II;
  `bluedog_NERVA_XE`: NERVA XE Hydrogen; `bluedog_smallNuclearEngine`:
  SmallEngine Hydrogen — `NERVANRX_Config.cfg`, `NERVAII_Config.cfg`,
  `NERVAXE_Config.cfg`, and `SmallEngine_Config.cfg`.
- `bluedog_SNTP_45`: SNTP PFE-100 Prototype and Hydrogen configurations —
  `SNTPPFE_Config.cfg`. All five NTR parts use LH2/EnrichedUranium rather than
  the removed stock-derived shared NTR MEC.
- `bluedog_Juno4_Engine_6K`: Juno6k Block I (Hydrazine/NTO) and Block II
  (Hydrazine/ClF3); `bluedog_Juno4_Engine_45K`: equivalent 45K Block I/II
  configurations — `Juno6k_Config.cfg`, `Juno45k_Config.cfg`.
- Juno Waterfall targets retain `basicEngine`; NERVA FF/II/XE and SmallEngine
  retain their respective `NERVA_FF`, `NERVA_II`, `NERVA_XE`, and `SNE`
  engineIDs. No engineID is changed by the new MEC file.

## Liquid / nuclear primary-engine inventory

The re-enumerated retained liquid/nuclear primary-engine inventory is 43 PARTs.
Thirty-nine are individually calibrated in the primary-engine batches. The four
small propulsion parts formerly deferred from that inventory are now handled in
the separate RCS/retro pass below, rather than being left as unconfigured stock
engines. The count includes the calibrated Jupiter vernier and the two real Juno
4 main-engine PARTs, while still excluding solid-only motors and RCS/retro/
ullage parts from its calibrated-primary total.

| Status | PARTs |
|---|---|
| Calibrated (39) | Able, M-1, GE-405, Vega GE-405H, Agena XLR81, AJ10-118F, TR-201, Apollo Block2 SPS, LM Ascent, LM Descent, GATV SPS, LR87, LR87 Single, LR91, Titan Transtage, E-1, F-1, H-1, Thor LR79, Thor LR101, Jupiter Vernier, Centaur RL10, RS-68, G-1, LR87 LH2 Single, MB60, RL20, RS30, J-2, J-2T, STNP-75, XLR129, NERVA FF/II/XE, smallNuclearEngine, SNTP-45, Juno4 Engine 6K, Juno4 Engine 45K. |
| Moved to RCS / retro pass (4) | Agena RetroThrustModule, GATV SPS LFO, Hexagon ServiceEngine, LM LunarFlyingVehicle. |

`bluedog_Agena_Subsat_Hitchhiker`, `bluedog_Keyhole_OCV_KH7`,
`bluedog_Corona_Retro`, and the Apollo/Skylab RCS parts are not counted as
primary engines; their current RCS/retro status is listed below.

## Current storable-engine implementation

- `bluedog_Agena_Engine_XLR81`: XLR81-BA-5, XLR81-BA-7,
  XLR81-BA-11, Model8096-39 — `Agena_XLR81_Config.cfg`.
- `bluedog_AJ10_118F`: AJ10-118F; `bluedog_TR_201`: TR-201;
  `bluedog_Apollo_Block2_SPS`: AJ10-137 — `AJ10_Adv_Config.cfg`,
  `LMDE_Config.cfg`, and `AJ10_137_Config.cfg`.
- `bluedog_LM_Ascent_Engine`: LMAE; `bluedog_LM_Descent_Engine`:
  LMDE-H and LMDE-J — `LMAE_Config.cfg`, `LMDE_Config.cfg`.
- `bluedog_GATV_SPS`: Model8250, ISPS, ISPS-HDA —
  `AgenaSPS_Config.cfg`.
- `bluedog_LR87` and `bluedog_LR87_Single`: LR87-AJ-5, -AJ-9,
  -AJ-11; `bluedog_LR91`: LR91-AJ-5, -AJ-9, -AJ-11; and
  `bluedog_Titan_Transtage`: AJ10-138 and -138A —
  `LR87_Config.cfg`, `LR91_Config.cfg`, and `AJ10_Adv_Config.cfg`.
- Each MEC carries historical propellant ratio, min/max thrust, vacuum/sea-level
  Isp curve, ullage, feed system, ignition count, origMass, and massMult. The
  stock B9 engine-switch module is removed only for these MEC-controlled parts;
  engineID and Waterfall modules are not changed.
- Career cost/tech keys are intentionally not copied: their RO source is coupled
  to the unavailable RO/RP-1 progression layer, while this AO installation does
  not provide that career API.

## Liquid RCS / retro / ACS pass

The live BDB source tree has 66 PART cfgs with `ModuleRCS*`: 13 live in
`Parts/Solids` and are deliberately deferred to the solid-motor pass; 53 are
non-solid. `bluedog_MORL_resistojet_RCS` is an electric resistojet and is not a
liquid RF engine; `bluedog_Corona_Retro` has its fictional RCS removed because
the local RO source identifies it as a solid-only retro. The remaining 51
liquid-RCS PARTs have a defined propellant path: 12 have per-part
historical/source-derived RCS calibration and 39 use the explicit
single-Hydrazine fallback in `30_BDB_RealFuels_RCS.cfg`. The fallback selector
is an exact part list, not a `bluedog_*` wildcard, and never deletes
`ModuleEngineConfigs`.

| Status | PARTs / implementation |
|---|---|
| Historical RCS (12) | Apollo R-4D 1X/2X/3X/4X, Apollo RCS Engine, Apollo RCS EngineQuad, Titan Transtage RCS, Titan 23G ACS, KH-7 OCV, LM Lunar Flying Vehicle, Hexagon Mk8 nitrogen spin jets, and P-11 HTP RCS. Apollo values come from `Apollo/RO_Apollo.cfg` / `Apollo/RO_LEM.cfg`; Titan from `Titan/RO_Titan.cfg`; KH-7/P-11 from `Agena` RO cfgs; Mk8 from `Hexagon/RO_Hexagon.cfg`. |
| Explicit fallback (39) | The remaining named non-solid liquid RCS parts in `30_BDB_RealFuels_RCS.cfg`; all retain their source transforms/thruster power and use Hydrazine at 230/95 s until a vehicle-specific source is found. |
| Non-liquid / deferred (15) | `bluedog_MORL_resistojet_RCS` remains electric; `bluedog_Corona_Retro` is solid-only after its source-backed RCS removal; the 13 `Parts/Solids` RCS/ACS cfgs are deferred with solid retro/separation work. |

The four former primary-inventory deferrals are no longer stock-propellant
engines: `bluedog_Agena_RetroThrustModule` is the local RO nitrogen cold-gas
retro (0.075 kN, 58/27 s); `bluedog_GATV_SPS_LFO` is the local BDB-RO draft's
two-chamber Agena SPS interpretation (UDMH/MON3/He, 0.1424–1.9216 kN,
255.8/95 s); `bluedog_LM_LunarFlyingVehicle` uses its RO LFV MEC and RCS
mixture; and `bluedog_Hexagon_ServiceEngine` uses a clearly labelled
RCSBlock4x-derived Hydrazine fallback (1.112 kN, 230/95 s) because its source
does not give a separate Isp or engine configuration.

`bluedog_Skylab_TRS_propulsionKit` is also in this small-propulsion scope even
though it has no `ModuleRCS*`: its RO patch supplies Hydrazine/Helium storage.
It now has an explicit `TRS-Hydrazine-Fallback` MEC, with its retained source
stock 4 kN and 240/100 s performance clearly recorded as a fallback until an
independent historical thrust/Isp source is found.

The historical RCS MECs use pressure-fed, `ignitions = 0` semantics for
effectively unlimited RCS restarts. The Agena nitrogen retro retains its source
single ignition and non-pressure-fed designation. No tank type is invented in
this pass: parts with built-in stock MonoPropellant are renamed only where a
single Hydrazine fallback is used; pressurant-bearing historical configurations
require a connected RF tank containing the listed propellants.

## Solid / retro / separation pass

The source-tree enumeration finds 75 retained PART cfgs with a
`ModuleEngines*` module that consumes stock `SolidFuel`, including 45 outside
`Parts/Solids`. All 75 are explicitly targeted by
`32_BDB_RealFuels_Solids.cfg`; the former unbounded solid HTPB MEC block was
removed from `20_BDB_RealFuels_Engines.cfg`.

| Status | Count | Notes |
|---|---:|---|
| Historical/source-calibrated solid MEC | 33 | P-11, Corona and Hexagon Mk8 plus 30 file-32 targets are superseded by files 33-38. AO file 01 supplies RO-compatible CTPB, PBAA and PUPE resource/tank definitions when RealismOverhaul is absent. |
| Exact-name HTPB source fallback | 42 effective | File 32 still carries a literal 72-PART safety selector, but files 33-38 supersede 30 of those PARTs with historical MEC/tank data. The remaining 42 are functional, explicitly provisional fallbacks. |

### Solid main-booster / kick-stage follow-up (in progress)

`33_BDB_RealFuels_SolidMainStages.cfg` now supersedes the provisional engine
MEC on six retained main-booster PARTs: SRMU (`SRMU_Config.cfg`, 8233.777 kN,
281/251 s, HTPB); GEM-40 and its inline variant (`GEM40_Config.cfg`, 644 kN,
274/245 s, HTPB); GEM-46 (`GEM46_Config.cfg`, 875 kN, 279.8/251 s, HTPB);
GEM-60 (`GEM60_Config.cfg`, 1235.947 kN, 274/246 s, HTPB); and UA120
(`RO_Titan_SRBs.cfg` UA120X B9 experiment and the matching UA120 engine
configs, eight PBAN configurations). Their final MEC replaces the file-32
engine MEC while retaining the source thrust curve/prefab, transform,
engineID, abort mode, and effect modules (including Waterfall where present).
Files 34 and 35 add thirteen more retained PARTs: seven Star/Altair upper-stage
motors and six Scout-family motors. These use the RO HTPB, PBAN, PSPC, NGNC,
CTPB, PBAA and PUPE assignments, source dry-mass baselines, thrust/Isp and
ModuleFuelTanks volumes. AO file 01 supplies the CTPB, PBAA and PUPE clones that
RO would normally add. File 36 adds the M55, SR19, SR73, Orbus 6E and Orbus 21
families, including SR19's separate Hydrazine/Helium RCS supply. Files 37 and
38 add Castor 120, Castor 30B/30XL, Castor II, Castor IV and Castor IVA-XL;
their B9 stock-fuel engine switches are removed or reduced to a single
geometry-only switch. In total, files 33-38 supersede 30 entries from file 32;
the other 42 entries remain explicitly **provisional**.

`39_BDB_RealFuels_B9SolidSafety.cfg` closes the remaining editor-time stock
fuel reintroduction paths. It removes RF-incompatible volume switches from the
Scout Castor/Algol parts, fixes GEM-60 to its non-XL body, fixes Little Joe II
Recruit to the single-motor body, fixes AJ260 to the full-length body, and fixes
SRMU to its full 2.5-segment body. Geometry-only replacement switches contain
no `RESOURCE[SolidFuel]` blocks. UA120 remains intentionally switchable because
file 33 replaces every subtype resource/tank payload with PBAN-backed
`ModuleFuelTanks` data linked to its MEC.

## RealScale selector correction

- The baseline remains 1.6, with the retained RO part-specific exceptions
  applied afterwards. `bluedog_Pegasus_*` is no longer used as a scale group:
  that prefix covers both the Pegasus launcher and Saturn's unrelated Pegasus
  scientific satellites.
- Pegasus launcher parts are now listed explicitly. Pegasus avionics and the
  Orion 38 remain at 1.6; the Saturn Pegasus experiment hardware uses 1.5646.
  Minotaur M55/nose hardware uses 1.336, and Scout Algol Short now shares the
  1.311 Scout motor scale.
- Added retained exceptions for M-1, MB-60, XLR-129, BE-3, Star 48BV,
  Castor 30B, AJ260, Skylab/MORL, MOL docking hardware and both spin tables.

## TweakScale correction

BDB supplies TweakScale through compatibility patches using `%MODULE`, so a
literal `name = TweakScale` scan was insufficient. File 40 now first normalizes
every retained existing BDB TweakScale module to AO's percentage profile
(`type = free`, `defaultScale = 100`), then adds that profile to uncovered RF
engines, RF tanks, RCS parts, structural parts and coupling parts. Repeated add
selectors are guarded by `!MODULE[TweakScale]`. Pods, science payloads, crewed
modules and arbitrary utility parts are intentionally not made rescalable by a
blanket wildcard.

#### UA120 common-part integration

The installed BDB `bluedog_UA120` is one common PART with eight existing
`engineSwitch` subtypes, rather than eight separate PARTs. AO keeps that model
switch and uses the installed MEC/B9 linkage (`LinkB9PSModule[engineSwitch]`):
choosing an MEC configuration selects the matching B9 body. Each subtype's B9
data also updates the common PBAN `ModuleFuelTanks` volume and both preserved
`ModuleEnginesRF` modes (`S1SRB` and `S1SRB_Abort`). No RO duplicated PART,
transform-fix model, `engineType`, or other RO helper is required. The stock
B9 `addedMass` values are removed so MEC `massMult` is the sole segment-mass
scaler instead of being applied a second time. The MEC uses the RO UA1205
reference `origMass = 33.79`; using BDB's stock `8.4094` tonne part mass here
would make every subtype roughly four times too light.

| B9 subtype | MEC CONFIG | Thrust (kN) | Isp vac/SL (s) | `massMult` | PBAN volume |
|---|---|---:|---:|---:|---:|
| UA1202 | UA1202 | 2075.65 | 266/238 | 0.649127 | 42231.225 |
| UA1203 | UA1203 | 3113.5 | 265/240 | 0.766085 | 63346.8375 |
| UA1204 | UA1204 | 4151.3 | 266/238 | 0.82388 | 84462.45 |
| UA1205 | UA1205 | 5338 | 266/238 | 1.0 | 105578.0625 |
| UA1206 | UA1206 | 6227 | 265/240 | 1.17659 | 118865.69 |
| UA1206F | UA1206F | 6385.714 | 269.5/245 | 1.03451 | 130078.3999 |
| UA1207 | UA1207 | 7450 | 269.5/245 | 1.206925 | 151758.1332 |
| UA1208 | UA1208 | 7450 | 269.5/245 | 1.3238828 | 170458.08 |

All eight MEC configurations use PBAN, `curveResource = PBAN`, one ignition,
no ullage requirement, and the source UA120 curve retained by the original
engine's `ThrustCurvePrefab`. UA1205 is the default MEC/body/tank combination.
The local RealFuels `PBAN` tank definition and propellant resource both exist.

P-11 retains its separate HTP/Helium RCS MEC while its main `ModuleEnginesRF`
uses PSPC. Corona's earlier source-backed RCS removal remains in place and its
solid retro is restored. Mk8 retains both its Nitrogen spin RCS and its solid
retro; the two MECs are separated by `type`.

All named fallback motors rename their built-in `SolidFuel` resource to HTPB,
so the final `ModuleEnginesRF` propellant is available locally. P-11 and Corona
instead carry PSPC; Hexagon Mk8 carries CTPB. HTPB, PSPC and AO's RO-compatible
CTPB definitions are present in GameData. No
ModuleFuelTanks type is introduced by this pass; source tank/transform and
thrust-curve modules are preserved.

## Waterfall static binding samples

RSMP and BDB both target 18 retained solid/retro parts. RSMP's legacy-pass
patches run before BDB's `AFTER[Bluedog_DB]` Waterfall patches, so both modules
would otherwise survive. `41_BDB_Waterfall_RSMP_Compat.cfg` removes only the
later BDB main-engine module IDs when RSMP is installed. Separate BDB RCS plume
modules are retained for P-11, Mk8, Scout Castor and the VFB entry probe;
Corona's orphan RCS plume is removed because its fictional RCS module was
already removed by the RF pass.

| Engine | Waterfall file | engineID | transform | Static conclusion |
|---|---|---|---|---|
| F-1 | `Compatibility/WaterfallFX/Saturn.cfg` | `basicEngine` | `thrustTransform` | RF patch preserves engineID and transform. |
| J-2 | `Compatibility/WaterfallFX/Saturn.cfg` | `BDBJ2` | `thrustTransform` | Hydrolox; not grouped with F-1. |
| RL10 | `Compatibility/WaterfallFX/Centaur.cfg` | `basicEngine` | `thrustTransform` | RF patch preserves binding. |
| Agena XLR81 | `Compatibility/WaterfallFX/Agena.cfg` | `basicEngine` | `thrustTransform` | RF patch preserves binding. |
| Peacekeeper SR119 | `Compatibility/WaterfallFX/Solids_Peacekeeper.cfg` | default solid module (no explicit engineID) | `thrustTransform` | No RF engineID rewrite occurs. |
| UA120 common part | No dedicated BDB Waterfall target; optional `Compatibility/RealPlumes/BDB_Titan_Solids.cfg` | `S1SRB` / `S1SRB_Abort` | `thrustTransform2` / `abortTransform2` | AO preserves both modes and identifiers; B9/MEC only changes performance, body and PBAN volume. |
| SNTP-45 | `Compatibility/WaterfallFX/Timberwind.cfg` | `timberwind45` | `thrustTransform` | RF patch preserves binding. |
| RS-68 | `Compatibility/WaterfallFX/Delta.cfg` | `basicEngine` | `thrustTransform` | MEC changes performance only; engineID/transform remain. |
| J-2/J-2T | `Compatibility/WaterfallFX/Saturn.cfg` | `BDBJ2` / `BDBJ2T` | `thrustTransform` | Separate hydrolox bindings remain intact. |
| STNP-75 | `Compatibility/WaterfallFX/Timberwind.cfg` | `timberwind75` | `thrustTransform` | AO corrects the copied `timberwind45` binding to 75, then removes the LOX-augmentation engine and matching aux module. |
| XLR-129 | `Compatibility/WaterfallFX/Engines.cfg` | `BDB_XLR129_SL` | `fxTransformBooster` | RF bimodal MEC retains the SL binding; orphan vacuum engine and `XLR129Vac` Waterfall module are removed together. |
| NERVA FF/II/XE, SmallEngine | `Compatibility/WaterfallFX/Engines.cfg` | `NERVA_FF` / `NERVA_II` / `NERVA_XE` / `SNE` | `thrustTransform` | New MECs retain the source engineIDs and transforms. |
| Juno IV 6K / 45K | `Compatibility/WaterfallFX/Jupiter.cfg` | `basicEngine` | `thrustTransform` / `thrustTransform2` | 6K is explicitly assigned `basicEngine`; 45K already uses it. |
| Apollo R-4D | `Compatibility/WaterfallFX/Apollo.cfg`, `ApolloRCS.cfg` | `basicEngine`; quad `foreEngine` / `aftEngine` | `thrustTransform`, `thrustTX`, `thrustTX1`, RCS transforms | The new MECs preserve those IDs; RCS effect modules remain transform-bound. |
| Titan Transtage / 23G ACS | `Compatibility/WaterfallFX/TitanRCS.cfg` | RCS module (no engineID) | `rcsTransform` | Propellant-only RCS calibration preserves the source effect transform. |
| KH-7 / LFV | `Compatibility/WaterfallFX/Agena.cfg`, `Apollo.cfg` | `basicEngine` | `thrustTransform` / `rcsTransform` | Main-engine MEC and RCS propellant patches leave Waterfall bindings intact. |
| Agena Retro / GATV SPS LFO / Hexagon Service | No dedicated BDB Waterfall target for retro/LFO; `HexagonRCS.cfg` for Hexagon | default engine / `basicEngine` | source `thrustTransform` | No effect binding was renamed; in-game ignition remains required. |

Part rescaling propagates to the part hierarchy, but that is not proof of visual plume placement. Editor and ignition testing is still required.

The current 11 MEC-controlled storable parts each have one matching BDB
Waterfall target patch: Agena.cfg (XLR81, GATV SPS), Delta.cfg (AJ10-118F,
TR-201), Apollo.cfg (SPS, LMAE, LMDE), and Titan.cfg (LR87 pair/single, LR91,
Transtage). The AO patch only removes B9 engine-switch modules and replaces
MECs; it does not change ModuleEnginesRF engineID or any ModuleWaterfallFX
module. This is a static binding check only; visual plume position/scale still
requires in-game ignition.

For Kerolox, explicit Waterfall targets exist for E-1 (Engines.cfg), F-1/H-1
(Saturn.cfg), Thor LR79/LR101 (Thor.cfg), and GE-405/GE-405H
(Vanguard_Vega.cfg). Their engineID fields are not edited by the AO MEC patch.
Jupiter Vernier has no BDB Waterfall target; its stock FX is preserved and must
be checked in-game if a Waterfall conversion is later added.

## Static checks re-run

- AO BDB, BDB RemoteTech and BDB VABO cfg scan for literal backtick newline
  escapes, one-line nodes, empty assignments, and `NaN`: zero matches. The
  previously retained single-line deletion nodes in file 10 and RemoteTech
  were expanded to KSP-safe multiline nodes.
- Brace delta of each AO BDB cfg: zero.
- RCS-pass target lookup: 56 explicit target names across
  `30_BDB_RealFuels_RCS.cfg` and `31_BDB_RealFuels_HistoricalRCS.cfg`; all 56
  exist in the current BDB `Parts` tree.
- RCS fallback safety: zero four-choice generic-menu markers and zero
  `!MODULE[ModuleEngineConfigs]` operations in file 30. File 31 contains 13
  dedicated historical/source-derived MEC modules; it does not alter an
  unrelated primary-engine MEC.
- RCS MEC thrust-field check: AO's existing RCS convention uses
  `type = ModuleRCS,*`; the four BDB RCS MEC groups use the same type. All four
  RCS `CONFIG`s now contain `thrusterPower`: Mk8-N2-Spin `0.005`, P11-HTP-RCS
  `0.05`, Apollo R-4D `0.445`, and Transtage-ACS `0.2`. Groups: 4; CONFIGs: 4;
  missing `thrusterPower`: 0.
- The RCS files have zero positive matches for Gemini, GeminiFerry, BigG,
  Shuguang, Mercury, Atlas, Redstone, Sergeant, or X-15 selectors.
- Hydrazine, MMH, MON1, Aerozine50, NTO, UDMH, MON3, Helium, Nitrogen, and HTP
  resource definitions are present in the installed GameData tree.
- Static Waterfall selector lookup finds one BDB target cfg each for Apollo RCS
  Engine, Apollo RCS EngineQuad, LFV, Transtage RCS, Titan 23G ACS, KH-7 OCV,
  and Hexagon ServiceEngine. No engineID or transform name is changed by this
  RCS pass.
- Solid-pass target lookup: 75 discovered stock-SolidFuel engine PARTs and 75
  explicit file-32 targets; uncovered: 0; extra targets: 0. File 32 has zero
  brace imbalance, literal newline escapes, one-line nodes, empty assignments,
  or `NaN` values. The removed file-20 global solid MEC has zero remaining
  markers.
- Waterfall static selector lookup finds a BDB target cfg each for P-11,
  Hexagon Mk8 Retro, and Corona Retro. File 32 does not rename engineIDs or
  thrust transforms.
- File-33 follow-up check: 6 explicit targets exist. UA120 has 8 installed B9
  subtype patches, 8 MEC CONFIGs, 8 MEC-to-B9 links, and 8 distinct PBAN tank
  volume mappings; HTPB/PBAN resources and the PBAN RF tank definition exist.
  Brace imbalance, literal escapes, one-line nodes, empty assignments, and
  `NaN` are all zero.
- AO cfg headers: all begin `// Modified 2026-09-01`.
- Every explicit AO BDB PART target resolves in the currently retained BDB
  source tree. Removed ProbeExpansion and unwanted programme targets are not
  carried as inert selector entries.
- RF tank types used by the BDB patches exist locally: Default, ServiceModule,
  Balloon, BalloonCryo, Cryogenic, HTPB, PBAN, PSPC and NGNC; AO file 01
  supplies CTPB, PBAA and PUPE when RO itself is absent.
- VABO solid classification now recognizes HTPB, PBAN, PSPC, CTPB, NGNC,
  PBAA and PUPE. Its RCS fallback excludes parts that also contain a primary
  `ModuleEnginesRF`, preventing combined engine/RCS stages from being moved out
  of their engine category. Generic tank classification also excludes engine,
  RCS and command parts, so it cannot overwrite those earlier classes;
  `bluedog_G1` is included in the hydrolox override.
- No Gemini/Big G/Shuguang asset remains under `Parts/MOL` or `Spaces/MOL`.
- The only compatibility filename still mentioning Atlas is the shared `Atlas_AtlasV_Centaur_RCS.cfg`; its remaining part selectors are Centaur.

## Required runtime follow-up

- After a user-initiated KSP start, inspect ModuleManager cache for actual selector matches, MEC coverage, and duplicate modules.
- Ignite the six Waterfall samples above; verify no double plume and correct origin/scale.
- The 13 solid-directory RCS/ACS cfgs and the MORL electric resistojet are outside this liquid RCS pass. XLR-129 bimodal nozzle switching, NTR plume origins, and all altered Waterfall visual scale/position still require in-game verification.
- The 42 remaining named HTPB fallbacks require per-engine historical
  engineType/config transcription in a follow-up pass. CTPB, PBAA and PUPE are
  already supplied locally by AO using RO's RealFuels clone policy.
