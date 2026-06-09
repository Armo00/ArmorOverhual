---
name: mk2-scale-rules
description: MK2 family rescaleFactor ×1.5 adjustment rules — all parameter scaling factors, confirmed 2026-06-08
metadata:
  type: project
---

# MK2 Family ×1.5 Scale Rules

MK2 截面从 1.25m×2.5m → 1.875m×3.75m（rescaleFactor ×1.5）。以下为各参数的调整因子。

## 零、关键原则

**所有调整基于当前已修改的值，而非 stock 原始值。**
- 例如：stock mass=1，我们已改为 0.5，mass×2 指 0.5×2=1
- 引擎参数另外讨论，不在此规则范围内

## 一、几何/结构

| 参数 | 因子 | 备注 |
|------|------|------|
| rescaleFactor | ×1.5 | |
| node_stack_* / node_attach | 不调整 | **KSP 会自动根据 rescaleFactor 缩放节点坐标**（见 [[ksp-node-autoscale]]） |
| bulkheadProfiles | 保持 mk2 | 不改变 |

## 二、质量与结构

| 参数 | 因子 |
|------|------|
| mass（干重） | ×2 |
| breakingForce | 不变 |
| breakingTorque | 不变 |
| crashTolerance | 不变 |

## 三、容积类

| 参数 | 因子 |
|------|------|
| CrewCapacity | 不变 |
| ModuleFuelTanks volume | ×3.375 |
| B9PartSwitch baseVolume | ×3.375 |
| RESOURCE amount/maxAmount（油箱类） | ×3.375 |
| InventorySlots / packedVolumeLimit | ×3.375 |
| KIS maxVolume | ×3.375 |
| CargoBay lookupRadius | ×1.5 |

## 四、推进相关（引擎引擎不谈）

| 参数 | 因子 |
|------|------|
| RCS thrusterPower | 不变 |
| ModuleResourceIntake area | ×2.25 |
| IntakeAir amount/maxAmount | ×3.375 |

## 五、能源与热控

| 参数 | 因子 |
|------|------|
| ElectricCharge 储量（电池） | ×3.375 |
| Solar Panel chargeRate | ×2.25 |
| Reactor EC 输出 | ×3.375 |
| Reactor 核燃料量 | ×3.375 |
| Radiator maxEnergyTransfer | ×2.25 |
| thermalMassModifier | ×2（与质量同步） |

## 六、力与力矩

| 参数 | 因子 |
|------|------|
| ReactionWheel torque (Pitch/Yaw/Roll) | ×3.375 |
| Decoupler ejectionForce | 不变 |

## 七、尺寸耦合

| 参数 | 因子 |
|------|------|
| TweakScale defaultScale | 1.25→1.875（×1.5） |
| DockingPort nodeType | size1→Armor1875 |
| DRAG_CUBE | 不调整 | FAR 忽略 drag cube，从 mesh 体素化计算；手动定义的 cube 是冗余数据 |
| IVA 模型 | ×1.5 |

## 八、经济

| 参数 | 因子 |
|------|------|
| cost | ×2（引擎单独调整） |
| entryCost | ×2（引擎单独调整） |

## 九、受影响零件清单

全部通过 `bulkheadProfiles = mk2` 识别。引擎 23 个另议，不在此清单中。

### Squad Stock（17 个）

| name | Title |
|------|-------|
| `mk2Cockpit_Standard` | Mk2 Cockpit |
| `mk2Cockpit_Inline` | Mk2 Inline Cockpit |
| `mk2DroneCore` | MK2 Drone Core |
| `mk2CrewCabin` | MK2 Crew Cabin |
| `mk2CargoBayL` | Mk2 Cargo Bay CRG-08 |
| `mk2CargoBayS` | Mk2 Cargo Bay CRG-04 |
| `mk2DockingPort` | Mk2 Clamp-O-Tron |
| `mk2Fuselage` | Mk2 Liquid Fuel Fuselage |
| `mk2FuselageLongLFO` | Mk2 Rocket Fuel Fuselage |
| `mk2FuselageShortLiquid` | Mk2 Liquid Fuel Fuselage Short |
| `mk2FuselageShortLFO` | Mk2 Rocket Fuel Fuselage Short |
| `mk2FuselageShortMono` | Mk2 Monopropellant Tank |
| `mk2SpacePlaneAdapter` | Mk2 to 1.25m Adapter |
| `mk2_1m_AdapterLong` | Mk2 to 1.25m Adapter Long |
| `mk2_1m_Bicoupler` | Mk2 Bicoupler |
| `adapterSize2-Mk2` | 2.5m to Mk2 Adapter |
| `adapterMk3-Mk2` | Mk3 to Mk2 Adapter |

### Mk2 Expansion — Command（5 个）

| name | Title |
|------|-------|
| `M2X_AnglerCockpit` | Angler Cockpit |
| `M2X_BladeCockpit` | Blade Cockpit |
| `M2X_TunaCockpit` | Tuna Cockpit (Fishhead) |
| `M2X_RavenCockpit` | Raven Cockpit |
| `M2X_ViperCockpit` | Viper Cockpit |

### Mk2 Expansion — FuelTank（11 个）

| name | Title |
|------|-------|
| `M2X_Shortbicoupler` | Mk2 Bicoupler |
| `M2X_HypersonicNose` | Hypersonic Nose |
| `M2X_InverterFuselage` | Inverter Fuselage |
| `M2X_linearTricoupler` | Linear Tricoupler |
| `M2X_Mk2bicoupler` | Mk2 Bicoupler (dual) |
| `M2X_SupersonicNose` | Supersonic Nose |
| `M2X_UST` | Service Tank |
| `M2X_Short25adapter` | Mk2 to 2.5m Adapter |
| `M2X_SlantAdapterS` | Slant Adapter S |
| `M2X_SpadeTail` | Spade Tail |
| `M2X_625tricoupler` | Mk2 Tricoupler |

### Mk2 Expansion — Aero（8 个）

| name | Title |
|------|-------|
| `M2X_InlineIntake` | Inline Intake |
| `M2X_AeroIntake` | Aerodynamic Intake |
| `M2X_circularintake` | Circular Intake |
| `M2X_EngineShroud` | Engine Shroud |
| `M2X_MantaIntake` | Manta Intake |
| `M2X_Precooler` | Precooler |
| `M2X_Shockcone` | Shock Cone |
| `M2X_Tailboom` | Empennage / Tailboom |

### Mk2 Expansion — Structural（10 个）

| name | Title |
|------|-------|
| `M2X_Decoupler` | Mk2 Decoupler |
| `M2X_Endcap` | Endcap |
| `M2X_LHub` | L Hub |
| `M2X_THub` | T Hub |
| `M2X_XHub` | X Hub |
| `M2X_RadialMountA` | Hardpoint A |
| `M2X_RadialMountB` | Hardpoint B |
| `M2X_StructuralTube` | Structural Tube C |
| `M2X_StructuralAdapterS` | Structural Tube B |
| `M2X_StructuralAdapterLong` | Structural Tube A |

### Mk2 Expansion — Utility（10 个）

| name | Title |
|------|-------|
| `M2X_AligningDockingPort` | Aligned Docking Port |
| `M2X_Mk2BatteryBank` | Battery Bank |
| `M2X_cargoContainer` | Cargo Container |
| `M2X_ShieldedDockingPort` | Shielded Docking Port |
| `M2X_SmallLab` | Lab |
| `M2X_LongCabin` | Crew Cabin |
| `M2X_Nosebay` | Nose Bay |
| `M2X_SCRCS` | RCS Stability Control |
| `M2X_Reactor` | Nuclear Reactor |
| `M2X_Servicebay` | Service Bay |

### SpaceTux RecycledParts（9 个）

| name | Title |
|------|-------|
| `mk2Separator` | Mk2 Separator |
| `mk2Decoupler` | Mk2 Decoupler |
| `mk2SAS` | Mk2 Advanced Reaction Wheel |
| `mk2Battery` | Mk2 Battery |
| `mk2BatteryAlt` | Mk2 Battery Alt |
| `mk2SolarBattery2k` | MK2 Solar Fuselage 2000 |
| `mk2SolarBattery1k` | MK2 Solar Fuselage 1000 |
| `MK2Cont8` | Mk2 KIS Container 8000L |
| `MK2Cont4` | Mk2 KIS Container 4000L |

### 汇总

| 来源 | 数量 |
|------|------|
| Squad Stock | 17 |
| Mk2 Expansion (Command) | 5 |
| Mk2 Expansion (FuelTank) | 11 |
| Mk2 Expansion (Aero) | 8 |
| Mk2 Expansion (Structural) | 10 |
| Mk2 Expansion (Utility) | 10 |
| SpaceTux RecycledParts | 9 |
| **非引擎合计** | **70** |
| 引擎（另议） | 23 |
| **总计** | **93** |

排除：`KKAOSS_adapter_base_to_MK2_g`（KPBS 转接器）、`mk2DockingPort` 旧版（zDeprecated，与新版同名）、`M2X_SolarpanelPod`（title: SP-G Shrouded Solar Array，bulkheadProfiles = srf，非 mk2 零件）。

## 十、燃料箱容积详细计算

所有 mk2 燃料箱零件通过 `Fuel_Conversions.cfg` 的 `:NEEDS[RealFuels]:Final` 规则自动转换为 `ModuleFuelTanks`。转换公式为：

- **LiquidFuel + Oxidizer**（无 ModuleCommand）：`volume = (LF_maxAmount + Ox_maxAmount) × 5`，type = Default
- **LiquidFuel only**（无 ModuleCommand）：`volume = LF_maxAmount × 5`，type = Fuselage
- **MonoPropellant only**（无 ModuleCommand）：`volume = MP_maxAmount × 5`，type = ServiceModule

×3.375 补丁应运行在 Fuel_Conversions.cfg 之后（同名 pass `:FINAL`，目录排序 M > G，已确保在之后），直接 assign 最终 volume 值（不使用 `*=`）。

### 10.1 Squad Stock 油箱

| Part Name | Title | 原始 RESOURCE | FC 转换值 | ×3.375 目标 |
|-----------|-------|--------------|-----------|-------------|
| `mk2Fuselage` | Mk2 Liquid Fuel Fuselage | LF 800 | 800×5 = 4000 (Fuselage) | **13500** |
| `mk2FuselageLongLFO` | Mk2 Rocket Fuel Fuselage | LF 360 + Ox 440 | (360+440)×5 = 4000 (Default) | **13500** |
| `mk2FuselageShortLiquid` | Mk2 Liquid Fuel Fuselage Short | LF 400 | 400×5 = 2000 (Fuselage) | **6750** |
| `mk2FuselageShortLFO` | Mk2 Rocket Fuel Fuselage Short | LF 180 + Ox 220 | (180+220)×5 = 2000 (Default) | **6750** |
| `mk2FuselageShortMono` | Mk2 Monopropellant Tank | MP 400 | 400×5 = 2000 (ServiceModule) | **6750** |
| `mk2SpacePlaneAdapter` | Mk2 to 1.25m Adapter | LF 180 + Ox 220 | (180+220)×5 = 2000 (Default) | **6750** |
| `mk2_1m_AdapterLong` | Mk2 to 1.25m Adapter Long | LF 360 + Ox 440 | (360+440)×5 = 4000 (Default) | **13500** |
| `mk2_1m_Bicoupler` | Mk2 Bicoupler | LF 180 + Ox 220 | (180+220)×5 = 2000 (Default) | **6750** |
| `adapterSize2-Mk2` | 2.5m to Mk2 Adapter | LF 360 + Ox 440 | (360+440)×5 = 4000 (Default) | **13500** |
| `adapterMk3-Mk2` | Mk3 to Mk2 Adapter | LF 900 + Ox 1100 | (900+1100)×5 = 10000 (Default) | **33750** |

### 10.2 MK2 Expansion 油箱

| Part Name | Title | 原始 RESOURCE | FC 转换值 | ×3.375 目标 |
|-----------|-------|--------------|-----------|-------------|
| `M2X_HypersonicNose` | Hypersonic Nose | LF 180 + Ox 220 | (180+220)×5 = 2000 | **6750** |
| `M2X_InverterFuselage` | Inverter Fuselage | LF 360 + Ox 440 | (360+440)×5 = 4000 | **13500** |
| `M2X_SpadeTail` | Spade Tail | LF 90 + Ox 110 | (90+110)×5 = 1000 | **3375** |
| `M2X_SlantAdapterS` | Slant Adapter S | LF 180 + Ox 220 | (180+220)×5 = 2000 | **6750** |
| `M2X_Short25adapter` | Mk2 to 2.5m Adapter | LF 180 + Ox 220 | (180+220)×5 = 2000 | **6750** |
| `M2X_Mk2bicoupler` | Mk2 Bicoupler (dual) | LF 360 + Ox 440 | (360+440)×5 = 4000 | **13500** |
| `M2X_linearTricoupler` | Linear Tricoupler | LF 360 + Ox 440 | (360+440)×5 = 4000 | **13500** |
| `M2X_625tricoupler` | Mk2 Tricoupler | LF 90 + Ox 110 | (90+110)×5 = 1000 | **3375** |

### 10.3 B9PartSwitch 油箱（非 Fuel_Conversions.cfg 转换）

| Part Name | Title | 原始 baseVolume | 当前补丁值 | ×3.375 目标 |
|-----------|-------|----------------|-----------|-------------|
| `M2X_UST` | Service Tank | 200 | @baseVolume = 1800 (fueltank.cfg) | **6075** |

### 10.4 座舱 ServiceModule MFT（手动添加，非 Fuel_Conversions.cfg 转换）

| Part Names | 当前 volume | ×3.375 目标 |
|------------|-----------|-------------|
| `M2X_DropshipCockpit`, `M2X_BladeCockpit`, `M2X_TunaCockpit`, `M2X_RavenCockpit`, `M2X_ViperCockpit` | 200 (Command.cfg) | **675** |

### 10.5 无燃料零件确认

以下 mk2 零件经检查**无 RESOURCE 块**（或仅有 ElectricCharge），不受 Fuel_Conversions.cfg 影响：

- Squad：`mk2Cockpit_Standard`（仅 EC+MP，有 ModuleCommand 排除）, `mk2Cockpit_Inline`（同）, `mk2DroneCore`（仅 EC，有 ModuleCommand）, `mk2CrewCabin`（无 RESOURCE）, `mk2CargoBayL/S`（无）, `mk2DockingPort`（无）
- MK2E：`M2X_Shortbicoupler`（无 RESOURCE）, `M2X_SupersonicNose`（无 RESOURCE）
- SpaceTux：全部无 LF/Ox/MP RESOURCE

## Why

Before implementing MK2 Expansion rescaleFactor changes, we needed a complete, agreed-upon set of rules for which parameters scale by what factor, and a comprehensive list of all affected parts. These were discussed and confirmed on 2026-06-08.

**How to apply:** When writing patches that change rescaleFactor to 1.5, apply these multipliers to the corresponding parameters. Always multiply against the CURRENT value in our patches (not stock), since stock values have already been modified.
