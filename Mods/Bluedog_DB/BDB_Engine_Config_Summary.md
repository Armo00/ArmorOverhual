# BDB RF engine configuration status

Date: 2026-09-01

This inventory describes the current cfg, not a planned configuration.

| Family | Current selector coverage | RF propellant | Historical-data status |
| --- | --- | --- | --- |
| Kerolox | E-1, Saturn F-1/H-1, Thor LR79/LR101, Jupiter vernier, GE-405, GE-405H | RP-1/LqdOxygen; GE-405H also HTP | All eight actual Kerolox-route parts use individual historic MECs. Juno 6K/45K are not Kerolox in the RO reference. |
| Hydrolox | RL10, RS-68, XLR-129, RS-30, RL-20, MB-60, LR87-LH2, J-2/J-2T, M-1 | LqdHydrogen/LqdOxygen | Each uses individual historical MECs; XLR-129 retains retracted/extended bimodal subconfigs. |
| G-1 fluorine | G-1 / G-1A | Hydrazine/LqdFluorine/Helium | Individually calibrated, pressure-fed; deliberately not classified as Hydrolox. |
| Timberwind NTR | STNP-75 | LqdHydrogen/EnrichedUranium | Individually calibrated 75 MW particle-bed NTR; deliberately not classified as Hydrolox. |
| Able storable | Able AJ10 | UDMH/IWFNA | AJ10-42 calibrated from AJ10 Early config. |
| Solid | Retained engines with stock SolidFuel | HTPB | Source thrust/Isp retained; pending. |
| Agena storable | XLR81 | UDMH/IRFNA-III or IRFNA-IV | Four calibrated XLR81 historical variants. |
| Storable main engines | AJ10-118F, TR-201, Apollo SPS, LM ascent/descent, GATV SPS, LR91/LR87, Transtage | Individual historical mixtures and MECs | Calibrated in 21_BDB_RealFuels_StorableEngines.cfg. |
| Nuclear thermal | SNTP-45, STNP-75, small NTR, NERVA XE/II/FF | LqdHydrogen/EnrichedUranium | Individual historical MECs; NTR variants use their own thrust/Isp/restart data. |
| Juno IV special | Juno 6K / 45K | Hydrazine/NTO or Hydrazine/ClF3, with Helium pressurant | Individual Block I/II MECs; deliberately not RP-1/LOX. |

Able, M-1, the eight Kerolox-route parts, GE-405/GE-405H, the eleven storable
main-engine parts in 21_BDB_RealFuels_StorableEngines.cfg, and the eleven
parts in 23_BDB_RealFuels_HydroloxEngines.cfg, and the five NTR plus two Juno
IV engines in 24_BDB_RealFuels_NTR_Juno4.cfg use imported per-model thrust/Isp
data. The remaining four listed engine-like parts are deferred RCS/retro work,
not uncalibrated main-engine MECs.
Agena RetroThrustModule, GATV SPS LFO, Hexagon ServiceEngine, and LM
LunarFlyingVehicle remain outside the main-engine MEC scope because they are
reserved for the later RCS/retro conversion pass.
