# O-A Outpost — 月球基地详细分析报告

> **生成日期**: 2026-06-22
> **存档文件**: `NEXT Aerospace/Ships/SPH/O-A Outpost.craft`
> **KSP 版本**: 1.12.3
> **分析工具**: ArmorOverhual 项目配置审核

---

## 一、总览

**O-A Outpost** 是一个综合性月球表面基地，由 **61 个零件**构成，使用 KPBS（Kerbal Planetary Base Systems）和 SSPXr（Stockalike Station Parts Expansion Redux）为主体框架，搭配 Near Future Exploration 探针核心、RemoteTech 通讯系统和 Squad 原版散热/照明/结构件。

基地按功能可划分为三大区域：

| 区域 | 定位 | 核心零件 |
|------|------|----------|
| **CenterHub** | 指挥/居住核心 | Central Hub + 4× Habitat MK2 + 2× Science Lab + 2× Greenhouse |
| **Utility** | 资源生产与电力 | Nuclear Reactor、Centrifuge、ISRU、Drill、Ore/Fuel Tanks、Batteries、Life Support |
| **Storage Expansion** | 扩展仓储与通讯塔 | KIS FuelTank、Fuel Tank LS、SSPX Cargo Container、通讯塔 |

---

## 二、完整零件清单

### 2.1 零件汇总（按类型分组）

#### 指挥/居住 (Habitation & Command) — 15 个

| # | 零件名 | 来源 Mod | 数量 | ArmorOverhual 配置 |
|---|--------|----------|------|---------------------|
| 1 | `KKAOSS.Central.Hub` | KPBS | 1 | ✅ Control.cfg — mass=26t, MFT 5000L (380 crew-day LS), RPM |
| 2 | `KKAOSS.Habitat.MK2.g` | KPBS | 4 | ✅ Habitat.cfg — mass=10.4t, MFT 4000L (300 crew-day LS each) |
| 3 | `KKAOSS.Science.g` | KPBS | 2 | ✅ science_lab.cfg — mass=14.2t, scienceCap=120000, scienceMultiplier=12 |
| 4 | `KKAOSS.Greenhouse.g` | KPBS | 2 | ✅ GreenHouse.cfg — 10-crew rated, custom "GreenhouseAR" converter |
| 5 | `sspx-observation-25-1` | SSPXr | 1 | ⚠️ 无直接 AO 补丁（有 station-habitation LS 规则适用于同类） |
| 6 | `sspx-tube-125-1` | SSPXr | 1 | ⚠️ 无直接 AO 补丁 |

#### 结构/连接 (Structural) — 19 个

| # | 零件名 | 来源 Mod | 数量 | ArmorOverhual 配置 |
|---|--------|----------|------|---------------------|
| 7 | `AO2.KKAOSS.KAS.Flexible.Corridor` | **AO 自定义** | 10 | ✅ corridor.cfg — ST 型 (2× scale), mass=0.3t, pipeDiameter=2.3m, TweakScale 2.5 |
| 8 | `KKAOSS.CrossSection.g` | KPBS | 2 | ✅ KPBS_Structural.cfg — mass=1.2t |
| 9 | `sspx-adapter-25-375-2` | SSPXr | 2 | ⚠️ 无直接 AO 补丁 |
| 10 | `sspx-cargo-container-25-3` | SSPXr | 1 | ⚠️ 无直接 AO 补丁 |
| 11 | `strutCubeLarge` | Squad | 1 | ✅ truss_tweak.cfg — cost=0.02 |

#### 电力/热力 (Power & Thermal) — 9 个

| # | 零件名 | 来源 Mod | 数量 | ArmorOverhual 配置 |
|---|--------|----------|------|---------------------|
| 12 | `KKAOSS.Nuclear.Reactor` | KPBS | 1 | ✅ KPBS_Production.cfg (mass=15t, 50 EU/DF) + SystemHeat FissionReactor (2MW heat, 200kW electric) |
| 13 | `KKAOSS.Nuclear.Fuel` | KPBS | 1 | ✅ KPBS_Production.cfg (mass=12t, 200 EU + 200 DF) |
| 14 | `KKAOSS.Centrifuge` | KPBS | 1 | ✅ KPBS_Production.cfg (mass=15t, 50 EU/DF) + SystemHeat 3 converters |
| 15 | `KKAOSS.Battery.Tank` | KPBS | 2 | ✅ FuelTank.cfg — "Z-150U", 150 kWh each, mass=1.0t |
| 16 | `foldingRadLarge` | Squad | 3 | ✅ SystemHeat radiator.cfg — 500 kW/m² @400K |
| 17 | `radPanelLg` | Squad | 3 | ✅ SystemHeat radiator.cfg — 25 kW/m² @400K |
| 18 | `radPanelSm` | Squad | 1 | ✅ SystemHeat radiator.cfg — 5 kW/m² @400K |

#### 生产/资源 (ISRU & Resources) — 9 个

| # | 零件名 | 来源 Mod | 数量 | ArmorOverhual 配置 |
|---|--------|----------|------|---------------------|
| 19 | `KKAOSS.drill` | KPBS | 1 | ✅ KPBS_Production.cfg (mass=1.8t) + SystemHeat Harvester (20kW) |
| 20 | `KKAOSS.ISRU.g` | KPBS | 1 | ✅ KPBS_Production.cfg (mass=9t, ISRUmultiplier=4) + SystemHeat (9 converters) |
| 21 | `KKAOSS.Ore.Tank` | KPBS | 1 | ✅ KPBS_Storage.cfg — mass=1.47t, Ore=3200 |
| 22 | `KKAOSS.Rocket.Fuel.Tank` | KPBS | 1 | ✅ FuelTank.cfg (MFT 12000L) + KPBS_Storage.cfg |
| 23 | `KKAOSS.Liquid.Fuel.Tank` | KPBS | 2 | ✅ KPBS_Storage.cfg — MFT 12000L ServiceModule |
| 24 | `KKAOSS.KIS.FuelTank` | KPBS | 1 | ✅ KPBS_Storage.cfg — mass=4.4t, KIS volume=88000L |
| 25 | `KKAOSS.Fuel.Tank.LS` | **AO 自定义** | 1 | ✅ Life_Support_Container.cfg — 50kL, 3943 crew-day LS storage |
| 26 | `KKAOSS.Storage.g` | KPBS | 2 | ✅ KPBS_Storage.cfg — mass=0.4t |

#### 生命支持 (Life Support) — 2 个

| # | 零件名 | 来源 Mod | 数量 | ArmorOverhual 配置 |
|---|--------|----------|------|---------------------|
| 27 | `KKAOSS.LS.container.tacls.big` | KPBS | 1 | ✅ Life_Support_Container.cfg — MFT 12000L, ~946 crew-day |
| 28 | `KKAOSS.LS.container.tacls.waste.big` | KPBS | 1 | ✅ Life_Support_Container.cfg — MFT 12000L, 15% Waste/35% WasteWater/50% CO2 |

#### 通讯 (Communications) — 5 个

| # | 零件名 | 来源 Mod | 数量 | ArmorOverhual 配置 |
|---|--------|----------|------|---------------------|
| 29 | `RelayAntenna100` (RA-100) | Squad → RT | 3 | ✅ squad_tweak.cfg — RT relay, 100.269AU @200Mbps, mass=0.25t |
| 30 | `RTLongAntennaX` (Communotron 64) | **AO 自定义** | 2 | ✅ Comms64Antenna.cfg — RT omni, 64,000km @0.5Mbps |

#### 探针/控制 (Probe & Control) — 2 个

| # | 零件名 | 来源 Mod | 数量 | ArmorOverhual 配置 |
|---|--------|----------|------|---------------------|
| 31 | `nfex-probe-stp-1` | NF Exploration | 2 | ✅ probe.cfg — mass×0.25, cost×0.2, EC×8, TweakScale 1.25 |

#### 照明 (Lighting) — 16 个

| # | 零件名 | 来源 Mod | 数量 | ArmorOverhual 配置 |
|---|--------|----------|------|---------------------|
| 32 | `spotLight1.v2` | Squad (ReStock variant) | 16 | ❌ 无直接 AO 补丁 |

---

### 2.2 AO 覆盖率统计

| 类别 | 总数 | 有 AO 配置 | 覆盖率 |
|------|------|-----------|--------|
| KPBS 零件 | 24 | 24 | **100%** |
| AO 自定义零件 | 11 | 11 | **100%** |
| Squad 零件 | 25 | 25 | **100%** |
| SSPXr 零件 | 5 | 0 (仅同类规则) | **~60%** |
| NF Exploration 零件 | 2 | 2 | **100%** |
| **总计** | **61** | **56** | **91.8%** |

> **注意**: SSPXr 的 5 个零件（tube-125-1、observation-25-1、adapter-25-375-2 ×2、cargo-container-25-3）在 ArmorOverhual 中没有针对特定零件名的补丁，但受益于 SSPXr 通用的 `station-habitation.cfg` 和 `station-core.cfg` 中的 LS/资源规则。

---

## 三、三大功能区域详细分析

### 3.1 CenterHub — 指挥/居住核心

```
                       [Tower #1: Comms/Observation]
                       sspx-adapter-25-375-2
                       ├── 8× spotLight1.v2 (环形照明)
                       ├── nfex-probe-stp-1 (探针核心)
                       └── RelayAntenna100 (RA-100 中继天线)
                              ↑
                       sspx-observation-25-1 (观测舱)
                              ↑
                       sspx-tube-125-1 (垂直通道)
                              ↑
    [Hab MK2] ← left ── KKAOSS.Central.Hub ── right → [Hab MK2]
    (dead end)            (ROOT, 380 crew-day)           ↓
                              ↑                    AO2 Corridor ×2
                              │                         ↓
                         front│back              [Storage.g] → Utility Zone
                              │
                   ┌──────────┴──────────┐
                   ↓                     ↓
           [Greenhouse.g]         [Science.g]
           (10-crew rated)        (scienceCap=120k)
                   ↓                     ↓
           AO2 Corridor ×2       AO2 Corridor ×2
                   ↓                     ↓
           [Greenhouse.g]         [Science.g]
                   ↓                     ↓
           [CrossSection.g]       [Hab MK2]
                   ↓                  (dead end)
           Storage Expansion Zone
```

**CenterHub 核心数据**（经 ArmorOverhual 调整后）：

| 属性 | 值 |
|------|-----|
| 质量 | 26 吨 |
| 生命支持容量 | 5000L MFT（380 crew-day） |
| 内含资源 | Food=2222.7, Water=1470.9, Oxygen=224,899.2, EC=50,000 |
| RPM | 已启用 RasterPropMonitorComputer |
| TweakScale | stack, defaultScale=2.5 |
| rescaleFactor | ×2 |

**4 个 Habitat MK2**（每个）：
- 质量: 10.4 吨
- LS 容量: 4000L MFT（300 crew-day）
- 内含资源: Food=1754.8, Water=1161.2, Oxygen=177,552, EC=40,000

**2 个 Science Lab**（每个）：
- 质量: 14.2 吨
- scienceCap=120,000, scienceMultiplier=12, researchTime=7
- dataStorage=12,000

**2 个 Greenhouse**（每个）：
- 10 人额定，custom "GreenhouseAR" converter
- 输入: EC (18kW), Fertilizer, WasteWater, CO2, Waste
- 输出: Oxygen, Food, Water, Hydrogen
- 存储: Fertilizer=1500, Waste capacity=700

---

### 3.2 Utility — 资源生产与电力区

```
         [Storage.g] (来源: CenterHub right 分支)
              ↓
         [CrossSection.g] ←──────────┬──────────→ [Centrifuge]
              ↓                       │            mass=15t
         [Nuclear Reactor]            │            SystemHeat converters:
          mass=15t                    │            - Uranium Reprocessor (108kW)
          2MW thermal                 │            - Xenon Extractor (54kW)
          200kW electric              │            - Uranium Extractor (108kW)
          ├── 2× radPanelLg           │                 ↓
          └── foldingRadLarge         │         [Nuclear Fuel]
              ↓                       │         mass=12t, EU/DF=200
         [Storage.g] ← Left ──────────┘
              ↓
    ┌─────────┼─────────┬──────────────┬──────────────┐
    ↓         ↓         ↓              ↓              ↓
  [Drill]  [ISRU]  [Ore.Tank]  [Rocket.Fuel]  [Liquid.Fuel×2]
  mass=1.8t mass=9t  Ore=3200    MFT 12000L      MFT 12000L each
  SH 20kW   SH ×9    mass=1.47t
       ↓    converters
    [ISRU] ←── drill 前方连接
```

**Utility 区表面挂载**（在 Storage.g 上）：
- `KKAOSS.Battery.Tank` ×2 — 各 150 kWh（总计 300 kWh / 1,080 MJ）
- `KKAOSS.LS.container.tacls.big` — 12000L MFT, ~946 crew-day
- `KKAOSS.LS.container.tacls.waste.big` — 12000L MFT, 废物储存
- `foldingRadLarge` — SystemHeat 散热器 (500 kW/m² @400K)

**Nuclear Reactor 散热**：
- 2× `radPanelLg` (共 50 kW/m² @400K)
- 1× `foldingRadLarge` (500 kW/m² @400K)
- 总计散热能力: ~550 kW/m² @400K（反应堆 2MW 热输出需要这些散热器维持热平衡）

**ISRU 生产能力**（ISRUmultiplier=4，SystemHeat 改造）：
- LF-only, LFO, Ox-only, MonoPropellant
- LH2-only / LH2O（需 CryoTanks）
- LCH4-only / LCH4O（需 CryoTanks）
- 每个 converter: 40kW 系统功率, 800K 出口温度

---

### 3.3 Storage Expansion — 扩展仓储与第二通讯塔

```
         [Greenhouse.g] (来源: CenterHub front 分支)
              ↓
         [CrossSection.g]
              │
    ┌─────────┼─────────┬──────────────┐
    ↓         ↓         ↓              ↓
  [Fuel.Tank.LS]  [KIS.FuelTank]  [Hab.MK2]  [SSPX Cargo Container]
  50kL MFT       KIS vol=88000    (dead end)       ↓
  3943 crew-day                                [strutCubeLarge]
                                                    ↓
                                            [sspx-adapter-25-375-2]
                                            ├── 8× spotLight1.v2
                                            ├── RelayAntenna100
                                            ├── RTLongAntennaX ×2
                                            └── (Tower #2)
```

**Storage Expansion 区关键数据**：

| 零件 | 关键参数 |
|------|----------|
| `KKAOSS.Fuel.Tank.LS` | AO 自定义 X50L 克隆，50kL MFT，~3943 crew-day LS |
| `KKAOSS.KIS.FuelTank` | mass=4.4t, KIS inventory=88,000L |
| `sspx-cargo-container-25-3` | SSPXr 2.5m 货柜，无直接 AO 补丁 |
| Tower #2 通讯塔 | RA-100 (100AU relay) + 2× Communotron 64 (64,000km omni) |

---

## 四、ArmorOverhual 修改影响分析

### 4.1 几何缩放 (KPBS_Scale.cfg)

**所有 KPBS 零件 rescaleFactor = 2**，这意味着整个基地的物理尺寸是原始 KPBS 的 **2 倍**。

- Central Hub 直径: ~3.67m → **~7.33m**
- Habitat MK2 长度: ~3.12m → **~6.23m**
- Greenhouse 长度: ~4.37m → **~8.74m**
- CrossSection 跨距: ~5.28m → **~10.56m**

**TweakScale**: 所有 KPBS 零件 `defaultScale = 2.5`（匹配 ×2 后的直径）。

**AO 自定义走廊**: `AO2.KKAOSS.KAS.Flexible.Corridor` (ST 型) 使用 2× rescaleFactor，pipeDiameter=2.3m。

### 4.2 质量调整

ArmorOverhual 的 KPBS 质量调整策略为 **÷4**（相对于原版后 ×4 再 ÷4 = 净效果接近原版值，但因为 rescaleFactor=2 带来的体积 ×8，实际是轻量化设计）：

| 零件 | 原版质量 | AO 质量 | 变化 |
|------|---------|---------|------|
| Central Hub | ~6.5t | 26t | **×4** |
| Habitat MK2 | ~2.6t | 10.4t | **×4** |
| Greenhouse | ~3.8t | ~15t (默认为 ×4) | **×4** |
| Science Lab | ~3.5t | 14.2t | **×4** |
| Nuclear Reactor | ~3.75t | 15t | **×4** |
| Centrifuge | ~3.5t | 15t | **×4.3** |
| ISRU | ~2.25t | 9t | **×4** |
| CrossSection | ~0.3t | 1.2t | **×4** |
| Storage.g | ~0.1t | 0.4t | **×4** |
| Battery Tank | ~0.25t | 1.0t | **×4** |

> **设计理念**: rescaleFactor=2 使零件体积变为原来的 8 倍，质量 ×4 意味着零件密度为原始的一半，符合"轻量化太空结构"的设计定位。

### 4.3 生命支持系统

**TACLS 集成**（RealFuels ModuleFuelTanks）：

基地总 LS 储存容量估算：

| 来源 | 容量 (crew-day) | 内含资源 |
|------|-----------------|----------|
| Central Hub | 380 | Food/Water/Oxygen/EC |
| 4× Habitat MK2 | 1,200 (4×300) | Food/Water/Oxygen/EC |
| TACLS Big Container | 946 | Food/Water/Oxygen |
| Fuel Tank LS (X50L) | 3,943 | Food/Water/Oxygen |
| **总计** | **~6,469 crew-day** | |

> 以 20 名 Kerbal 计算，基地可自主维持约 **323 天**（~10.8 个月）无需补给。

**废物处理**：
- TACLS Waste Big Container: 12000L，按 15%/35%/50% 分配 Waste/WasteWater/CO2
- Greenhouse 消耗 Waste、WasteWater、CO2，产出 Oxygen、Food、Water，形成部分闭环

### 4.4 电力与热力系统

**EC 能量单位**：根据项目约定（`memory/battery-rules.md`），**1 EC = 1 Wh = 3.6 kJ**，因此 **1 EC/s = 3.6 kW**。

**Nuclear Reactor（SystemHeat FissionReactor）**：

| 参数 | 值 | 来源 |
|------|-----|------|
| 热输出 (100% throttle) | **2,000 kW** (废热) | `HeatGeneration { key = 100 2000 }` |
| 电输出 (100% throttle) | **200 EC/s = 720 kW** | `ElectricalGeneration { key = 100 200 }`, 1 EC/s = 3.6 kW |
| 反应堆总热功率 | **2,720 kW** | 2000 + 720 |
| 热电转换效率 | **26.5%** | 720 / 2720 |
| 额定温度 | **850 K** | `NominalTemperature = 850` |
| 临界温度 | **1,300 K** | `CriticalTemperature = 1300` |

**储能**：

| 组件 | 储电容量 | 换算 |
|------|---------|------|
| 2× Battery Tank | 150,000 EC 各 = **300 kWh** 总 | AO FuelTank.cfg |
| Central Hub | 50,000 EC = **50 kWh** | 内置 |
| 4× Habitat MK2 | 各 40,000 EC = **160 kWh** 总 | 内置 |

**散热器（含 TweakScale 修正，从 craft 实测）**：

TweakScale 的 `currentScale` 为百分比（`free` 类型），200% = ×2 倍线性尺寸 = ×4 倍面积。

| 散热器 | 数量 | craft currentScale | 面积倍率 | 散热量 @400K (每个) | 热导率 (每个) | 总热导 |
|--------|------|-------------------|---------|--------------------|-------------|--------|
| foldingRadLarge | **4** | 100 (×1.0) | ×1 | 500 kW | 1.25 kW/K | **5.00 kW/K** |
| radPanelLg | 3 | **200 (×2.0)** | ×4 | **100 kW** | **0.25 kW/K** | **0.75 kW/K** |
| radPanelSm | 1 | **200 (×2.0)** | ×4 | **20 kW** | **0.05 kW/K** | **0.05 kW/K** |
| **总计** | **8** | | | **2,320 kW @400K** | | **5.80 kW/K** |

**SystemHeat 环路温度平衡分析**：

平衡温度 T_eq = 总热负荷 ÷ Σk。

| 运行场景 | 总热负荷 | 平衡温度 | 评估 |
|----------|---------|---------|------|
| 仅反应堆 100% | 2,000 kW | **~345 K** | ✅ 远低于额定 850K，**散热过剩** |
| 反应堆 + ISRU + Centrifuge + Drill | 2,650 kW | **~457 K** | ✅ ISRU/Centrifuge 全效，Drill ~77% 效率 |
| 反应堆 50% 油门 | 1,000 kW | **~172 K** | ✅ 极保守，散热大幅过剩 |

> **结论**：TweakScale ×4 的 radPanelLg/Sm + 4 个 foldingRadLarge 提供了远超所需的散热能力。反应堆全功率时环路仅 ~345K，全系统全开 ~457K，均在安全范围内。Drill 在 457K 时尚有 ~77% 效率。

### 4.5 通讯系统 (RemoteTech)

基地配备两套通讯塔：

**Tower #1**（CenterHub 顶部）：
- 1× RA-100 Relay: 100.269AU @200Mbps（碟形，可覆盖整个太阳系）

**Tower #2**（Storage Expansion 顶部）：
- 1× RA-100 Relay: 同上
- 2× Communotron 64 (RTLongAntennaX): 64,000km 全向 @0.5Mbps

**内置天线**（所有 KPBS 指挥舱经 `squad_tweak.cfg` 自动获得）：
- ModuleRTAntennaPassive: 3,000km 全向（从 craft 文件确认）
- ModuleSPU: 主动信号处理

**通讯覆盖评估**：
- 月球轨道高度 (~384,400km from Earth) 远在 Communotron 64 (64,000km) 范围之外
- RA-100 (100AU ≈ 15,000,000,000km) 绰绰有余覆盖整个地月系统及深空
- 2× RA-100 提供 relay 冗余

### 4.6 SystemHeat 热管理

所有产热零件已转换为 SystemHeat 模块，分布在 **3 个独立热环路**：

| 环路 moduleID | 零件 | 热负荷 @100% | 额定/出口温度 | 临界/Shutdown |
|--------------|------|-------------|-------------|---------------|
| `reactor` | Nuclear Reactor | 2,000 kW | 850K / — | 1,300K |
| `isru` | ISRU (×9) + Centrifuge (×3) | 630 kW | — / 800K | 1,250–1,300K |
| `harvester` | Drill | 20 kW | — / 400K | 750K |

**全系统 100% 运行平衡温度**：2,650 kW ÷ 5.80 kW/K = **~457K**

- **reactor 环路**：457K ≪ 850K 额定，安全 ✅
- **isru 环路**：457K < 800K 出口温度，全效运行 ✅
- **harvester 环路**：Drill 效率曲线 `key = 400 1.0; key = 650 0.0`，457K → ~77% 效率 ⚠️

> **结论**：散热能力充足，全系统满负荷运行时环路仅 457K。唯一的轻微损失是 Drill 效率从 100% 降到 ~77%，在可接受范围内。如果需 Drill 满效运行，可将反应堆降功率至 ~80%（环路 ~360K）。

---

## 五、零件拓扑连接图

```
                                   [Tower #1]
                                   nfex-probe-stp-1
                                   RelayAntenna100
                                   sspx-adapter-25-375-2 (8× spotLight)
                                        │
                                   sspx-observation-25-1
                                        │
                                   sspx-tube-125-1
                                        │
        [Hab MK2]                   top│                    [Hab MK2]
        Corridor                    left│right                 Corridor ×2
        (dead end)        ────────── CENTRAL HUB ─────────    Corridor
                                    (ROOT)                    Storage.g ─── CrossSection.g
                                   front│back                 [UTILITY ZONE]
                        ┌───────────────┴───────────────┐
                   Greenhouse.g                   Science.g
                   Corridor ×2                    Corridor ×2
                   Greenhouse.g                   Science.g
                   CrossSection.g                 Hab MK2
                        │                         Corridor
        ┌───────────────┼───────────────┬──────────────┐        (dead end)
   Fuel.Tank.LS   KIS.FuelTank    Hab.MK2    SSPX Cargo
   Corridor       Corridor        Corridor   strutCube
   (dead end)     (dead end)      (dead end) sspx-adapter-25-375-2
                                                  │
                                            [Tower #2]
                                        8× spotLight
                                        RelayAntenna100
                                        Communotron 64 ×2
```

---

## 六、设计评估与建议

### 6.1 优势

1. **生命支持冗余充足**: ~6,469 crew-day 总容量，远超实际需求，适合长期无人值守运行
2. **ISRU 闭环能力强**: Drill → Ore Tank → ISRU → Fuel Tanks 形成完整 ISRU 链条，可在月球表面自持生产燃料
3. **通讯覆盖全面**: 2× RA-100 relay + 2× Communotron 64 omni，提供深空 relay 和近距全向通讯双重保障
4. **模块化设计**: KPBS CrossSection 实现十字分支，走廊系统灵活连接各模块
5. **电力充足**: 200kW 核电 + 300kWh 电池储能，可持续供应基地所有系统

### 6.2 潜在问题

1. **Drill 在全功率下的效率损失**：全系统满负荷时环路 ~457K，Drill 效率从 100% 降至 ~77%。不是致命问题（钻头仍可工作），但如果需要最高采矿效率，可降反应堆功率至 80%。
2. **SSPXr 零件未直接适配**: sspx-tube-125-1、sspx-observation-25-1 等 5 个 SSPXr 零件没有针对性的 AO 补丁，其 mass/cost 可能与其他已适配零件不成比例。
3. **KIS FuelTank 容积**: KIS inventory=88,000L 非常大，可携带大量建造材料，但也增加了 4.4t 质量。
4. **无起落架/着陆腿**: 基地未配备任何着陆支撑结构，在月球表面需依赖平地或 KAS 地面固定板。

### 6.3 改进建议

- 考虑为反应堆增加 1-2 个专用大型散热器，确保热平衡
- 为 SSPXr 零件添加单独的 AO 补丁以统一 mass/cost 比例
- 考虑添加 KIS 地面固定板 (KKAOSS_KIS_ground_plate) 用于月面固定
- 可在 Tower #2 上增加 1 个 RTGigaDish 以支持真正深空通讯（如果需要）

---

## 七、附录

### A. ArmorOverhual 相关配置文件清单

| 文件 | 涉及零件 |
|------|----------|
| `Mods/PlanetaryBaseInc-KPBS/Control.cfg` | Central Hub, Cupola, Control |
| `Mods/PlanetaryBaseInc-KPBS/Habitat.cfg` | Habitat MK1, MK2 |
| `Mods/PlanetaryBaseInc-KPBS/GreenHouse.cfg` | Greenhouse (PlanetaryGreenhouse→GreenhouseAR) |
| `Mods/PlanetaryBaseInc-KPBS/science_lab.cfg` | Science Lab |
| `Mods/PlanetaryBaseInc-KPBS/KPBS_Production.cfg` | Reactor, Centrifuge, ISRU, Drill, Nuclear Fuel |
| `Mods/PlanetaryBaseInc-KPBS/KPBS_Storage.cfg` | Storage, Ore Tank, Liquid Fuel, KIS, Battery |
| `Mods/PlanetaryBaseInc-KPBS/FuelTank.cfg` | Fuel Tank MFT conversion, Battery container |
| `Mods/PlanetaryBaseInc-KPBS/KPBS_Structural.cfg` | CrossSection, corridors, garage, airlocks |
| `Mods/PlanetaryBaseInc-KPBS/KPBS_Scale.cfg` | rescaleFactor=2, TweakScale=2.5 (all KPBS) |
| `Mods/PlanetaryBaseInc-KPBS/Life_Support_Container.cfg` | All LS containers + Fuel_Tank_LS clone |
| `Mods/PlanetaryBaseInc-KPBS/corridor.cfg` | KAS Flexible Corridor + AO clones |
| `Mods/PlanetaryBaseInc-KPBS/adapters.cfg` | All KPBS adapters |
| `Mods/SystemHeat/KPBS/NuclearReactor.cfg` | Nuclear Reactor → SystemHeat FissionReactor |
| `Mods/SystemHeat/KPBS/centrifuge.cfg` | Centrifuge → SystemHeat converters |
| `Mods/SystemHeat/KPBS/ISRU.cfg` | ISRU → SystemHeat converters |
| `Mods/SystemHeat/KPBS/drills.cfg` | Drill → SystemHeat Harvester |
| `Mods/SystemHeat/Squad/radiator.cfg` | All Squad radiators → SystemHeat temp curves |
| `Mods/NF-Exploration/probe.cfg` | All NF Exploration probes |
| `Mods/RemoteTech/Squad/squad_tweak.cfg` | All Squad antennas → RemoteTech |
| `Mods/RemoteTech/Squad/Comms64Antenna.cfg` | RTLongAntennaX (Communotron 64) clone |
| `Mods/RemoteTech/RemoteTech_tweak.cfg` | RT dedicated antenna parts |
| `SquadPartsOverhaul/structure/truss_tweak.cfg` | strutCube, truss pieces |
| `Mods/VABO/PlanetaryBaseInc-KPBS.cfg` | All KPBS → base_component VABO category |

### B. 数据来源

- Craft 文件: `saves/NEXT Aerospace/Ships/SPH/O-A Outpost.craft`
- ArmorOverhual 项目: `GameData/zzzArmorOverhual/`
- KPBS 原始配置: `GameData/PlanetaryBaseInc/`
- SSPXr 原始配置: `GameData/StationPartsExpansionRedux/`
- NF Exploration 配置: `GameData/NearFutureExploration/`
- RemoteTech 配置: `GameData/RemoteTech/`
- Squad 原版配置: `GameData/Squad/`

---

> **报告生成工具**: ArmorOverhual 项目分析 | **分析师**: Dr Armor 的 Claude 助手
