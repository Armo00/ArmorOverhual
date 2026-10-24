# BDB fixed tank volumes

2026-09-06. AO BDB capacity values must be literal numbers in a separate exact-PART patch for each part. Calculations below were performed offline, not by ModuleManager.

Removed 60 repeated BDB runtime conversion blocks. All pre-existing calibrated numeric capacity values remain unchanged. The 37 parts below previously depended on generic conversion and now have explicit numeric modules in their host-type files. Their modules (including any ElectricCharge/Hydrazine TANK capacities) match the prior evaluated cache; all 37 totals were independently recalculated from their source RESOURCE maxAmount values.

The six AO global dynamic volume rules now exclude parts marked `aoBDBExplicitTank = true`. The marker is set in `:FIRST` for the retained BDB catalogue only; unrelated mods retain their prior global behavior. Ninety-two parts have explicit cleanup of exactly the legacy resources removed by their former global/local conversion. Pre-existing calibrated definitions then supply their established volumes.

This preserves the former conversion results, including questionable pre-existing capacities. It is not a new capacity calibration or a claim that every existing value is physically correct. Life-support modules supplied by other mods, existing solid motor fuel variations, and engine performance are outside this change.

| Part | File | Offline calculation (L) | Fixed volume (L) |
| --- | --- | --- | --- |
| `bluedog_Juno4_FuelTank_1` | FuelTank.cfg | (198 + 242) × 5 | 2200 |
| `bluedog_CELV_SustainerTank` | FuelTank.cfg | (585 + 715) × 5 | 6500 |
| `bluedog_Minuteman_PSRE` | UpperStage.cfg | (4.5 + 5.5) × 5 | 50 |
| `bluedog_Vega_EngineMount` | Structural.cfg | (45 + 55) × 5 | 500 |
| `bluedog_LittleJoe2_Body` | Structural.cfg | 20 × 5 | 100 |
| `bluedog_LM_SheLab` | CommandPod.cfg | 30 × 5 | 150 |
| `bluedog_Hexagon_Mk8_Retro` | SolidRocketMotor.cfg | 3.6 × 5 | 18 |
| `bluedog_Hexagon_ServiceModule` | ServiceModule.cfg | 300 × 5 | 1500 |
| `bluedog_LDC_S2_EngineMount` | Structural.cfg | 30 × 5 | 150 |
| `bluedog_MiniLab_Adapter` | Structural.cfg | 30 × 5 | 150 |
| `bluedog_MOL_EquipmentSection` | ServiceModule.cfg | 300 × 5 | 1500 |
| `bluedog_Titan3_CommercialPLF_SAF` | Payload.cfg | 40 × 5 | 200 |
| `bluedog_Saturn_S4B_EngineMount` | Structural.cfg | 45 × 5 | 225 |
| `bluedog_GooLab_Module` | CommandPod.cfg | 5 × 5 | 25 |
| `bluedog_Skylab_EOSS_aftMDA` | CommandPod.cfg | 500 × 5 | 2500 |
| `bluedog_Skylab_EOSS_interstageModule` | CommandPod.cfg | 150 × 5 | 750 |
| `bluedog_Skylab_RadiatorMount` | Thermal.cfg | 270 × 5 | 1350 |
| `bluedog_Burner2` | RCS.cfg | 10 × 5 | 50 |
| `bluedog_HAPS_HAPS` | Engines.cfg | 3.2 × 5 | 16 |
| `bluedog_HAPS_SuperHAPS` | Engines.cfg | 12.8 × 5 | 64 |
| `bluedog_HOSS_EngineMount` | Structural.cfg | 15 × 5 | 75 |
| `bluedog_Titan3_CommercialPLF` | Payload.cfg | 40 × 5 | 200 |
| `bluedog_Titan3_CommercialPLF_PF` | Payload.cfg | 40 × 5 | 200 |
| `bluedog_DCSS_Tank` | FuelTank.cfg | 40 × 5 + 200 / 1000 | 200.2 |
| `bluedog_Juno4_FuelTank_2` | FuelTank.cfg | 10 × 5 + 50 / 1000 | 50.05 |
| `bluedog_Jupiter_Guidance` | UncrewedCommandPod.cfg | 6 × 5 + 60 / 1000 | 30.06 |
| `bluedog_Skylab_ATM_core` | UncrewedCommandPod.cfg | 20 × 5 + 100 / 1000 | 100.1 |
| `bluedog_Skylab_powerModule_core` | UncrewedCommandPod.cfg | 20 × 5 + 2000 / 1000 | 102 |
| `bluedog_Skylab_VFB_entryProbe` | UncrewedCommandPod.cfg | 8 × 5 + 500 / 1000 | 40.5 |
| `bluedog_Skylab_TRS_probeCore` | UncrewedCommandPod.cfg | 65 × 5 + 500 / 1000 | 325.5 |
| `bluedog_IUS_Avionics` | UncrewedCommandPod.cfg | 50 × 5 + 200 / 1000 | 250.2 |
| `bluedog_TOS_Avionics` | UncrewedCommandPod.cfg | 10 × 5 + 60 / 1000 | 50.06 |
| `bluedog_Athena_OAM` | UpperStage.cfg | 25 × 5 + 60 / 1000 | 125.06 |
| `bluedog_Minotaur_GCA` | UncrewedCommandPod.cfg | 10 × 5 + 60 / 1000 | 50.06 |
| `bluedog_Peacekeeper_PostBoostVehicle` | UpperStage.cfg | 6.8 × 5 + 60 / 1000 | 34.06 |
| `bluedog_Pegasus_Avionics` | UncrewedCommandPod.cfg | 5 × 5 + 30 / 1000 | 25.03 |
| `bluedog_Taurus_Avionics` | UncrewedCommandPod.cfg | 5 × 5 + 30 / 1000 | 25.03 |

Validation: all 37 offline calculations equal the prior cache, all former 92 conversion targets are accounted for, no BDB patch retains runtime volume arithmetic, and numeric volume patches use one exact PART name each. KSP has not been launched for this change.
