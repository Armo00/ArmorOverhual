# 嫦娥-吴刚 月面任务技术报告

> **生成日期**: 2026-06-23
> **存档文件**: `NASA/Ships/VAB/嫦娥-吴刚.craft`
> **KSP 版本**: 1.12.3
> **发射载具**: CZ-16A（长征十六号甲）
> **任务目标**: 载人月球轨道 + 月面着陆
> **全箭高度**: ~125.6 m | **总零件数**: 113 | **级数**: 8

---

## 一、任务概述

嫦娥-吴刚是一次载人月球探测任务，使用 CZ-16A 重型运载火箭将嫦娥载人飞船和吴刚月面着陆器送入月球轨道。CZ-16A 是 CZ-16 的改进型号，主要改动为增大二级燃料箱尺寸并将二级 Raptor Vacuum 发动机从 2 台增至 3 台。全箭采用 Methalox（甲烷+液氧）主推进 + Hydrolox（液氢+液氧）上面级/飞船的统一低温推进剂体系。

### 飞行剖面（发射段）

| 时序 | 事件 | 触发条件 |
|------|------|---------|
| **T-3 s** | 一级 15× Raptor 2 SL 点火（Stage 7），起飞推力 43,809 kN | 手动点火 |
| **T+0 s** | 6 组发射卡钳释放（Stage 6），火箭离开发射台 | Stage 指令 |
| **T+0 s** | **Smart Timer 激活**：发射卡钳释放同步启动 2 分 45 秒倒计时 | Stage 6 事件触发（`allowStage = True`） |
| T+55 s | 一级发动机节流，渡过最大动压区（Max-Q） | 飞行程序 |
| T+75 s | 一级发动机恢复最大推力 | 飞行程序 |
| **T+2 min 35 s** | **MECO**：一级主发动机关机 | Stage 指令 |
| **T+2 min 37 s** | **一二级分离**（Stage 4，sepIB 分离固体火箭点火） | Stage 指令 |
| **T+2 min 42 s** | **二级 3× Raptor Vac 点火**（Stage 3），1.10 ML Methalox | Stage 指令 |
| **T+2 min 45 s** | **发射逃逸塔分离**：Smart Timer 倒计时归零 → Action Group Custom07 → 逃逸塔上下段分离抛弃 | Timer 到期 |
| T+2:45+ | 二级继续推进，进入 LEO → TLI 轨道 | — |

### 飞行剖面（在轨段）

| 时序 | 事件 |
|------|------|
| TLI 完成后 | **展开式整流罩解锁**：类似土星五号 SLA 面板，整流罩花瓣展开（非抛离），暴露嫦娥飞船和吴刚着陆器 |
| — | 嫦娥飞船服务舱 Hecate 发动机点火，从整流罩中分离并调头 180° |
| — | 嫦娥飞船与吴刚着陆器顶部对接，将着陆器从整流罩中拖出 |
| — | 组合体继续飞往月球 |
| — | 月球轨道插入（嫦娥服务舱 Hecate 工作，Stage 1） |
| — | 吴刚着陆器分离，动力下降至月面 |
| — | 月面驻留 20 天（6 人） |
| — | 吴刚上升级点火，返回月球轨道，与嫦娥飞船交会对接 |
| — | 乘员转移至嫦娥返回舱，地月返回 |
| — | 梦舟返回舱再入：热盾 + 双降落伞（Stage 0），溅落回收 |

### 一级回收段

| 时序 | 事件 |
|------|------|
| 一二级分离后 | 一级惯性滑行至远地点 |
| 再入前 | 底部储箱（发动机舱上方）提供反推推进剂，栅格翼展开 |
| 再入段 | 4× Grid Fin L Titanium 气动减速 + 热气 RCS 姿控 |
| 着陆前 | 发动机反推点火（使用底部储箱 Methalox），4× FalconLeg Mk2 着陆腿展开 |
| 着陆 | 垂直软着陆，回收准备复用 |

### 自定义动作组

| 动作组 | 功能 |
|--------|------|
| Custom01 | 外围 3 台 Raptor 2 SL（gimbal limiter 70%） |
| Custom02 | 中心 12 台 Raptor 2 SL（gimbal limiter 20%） |
| Custom03 | 与 Custom02 联动 |
| Custom05 | （待确认） |
| Custom07 | 发射逃逸塔分离（Smart Timer 触发目标） |
| Custom08 | 栅格翼制动（与 Brakes 动作组联动） |
| Custom09 | （待确认） |
| Light | 外部照明灯 |
| Gear | 着陆腿收放 |

---

## 二、CZ-16A 第一级（可回收助推器）

CZ-16A 第一级采用 15 台 Raptor 2 SL 发动机，推进剂为 Methalox。一级具备完整的回收能力：栅格翼气动减速 + 发动机反推 + 着陆腿垂直着陆。

| 参数 | 数值 |
|------|------|
| **发动机** | 15× Raptor 2 SL（SEP.23.RAPTOR2.SL.RC，Raptor 3 构型） |
| **安装方式** | 12 台中心簇（stack node）+ 3 台外围（surface attach） |
| **中心簇 gimbal limiter** | 20%（Custom02） |
| **外围 gimbal limiter** | 70%（Custom01） |
| **单台推力** | 2,920.6 kN（海平面），ISP 351 s（真空）/ 330 s（海平面） |
| **总起飞推力** | **43,809 kN** |
| **推进剂** | LqdMethane 41.37% + LqdOxygen 58.63% |
| **发动机单台质量** | 1.525 t（AO 配置） |
| **发动机总干质量** | 22.875 t |
| **TweakScale** | defaultScale=1.25, currentScale=1.25（所有 15 台） |

### 推进剂储箱

| 储箱 | MFT 容积 | 类型 | 内容 |
|------|---------|------|------|
| **主储箱** (P074) | **2,392,274 L** | Cryogenic, util 95% | LqdMethane 989,684 + LqdOxygen 1,402,590 |
| **底部回收储箱** (P079) | **125,909 L** | Default, util 95% | LqdMethane 52,089 + LqdOxygen 73,821 |

> **设计说明**：回收用的辅助储箱位于火箭**底部**（发动机舱上方），而非头部。此布局确保回收阶段的重心尽可能靠下，提高反推着陆时的稳定性。

| 推进剂 | 装载量 (单位) | 备注 |
|--------|-------------|------|
| LqdMethane | 1,041,773 | 主箱 + 底部回收箱 |
| LqdOxygen | 1,476,410 | 主箱 + 底部回收箱 |

### 回收与姿控

| 组件 | 数量 | 关键参数 | TweakScale |
|------|------|---------|-----------|
| KRE-FalconLegMk2-L 着陆腿 | 4 | mass=0.55t/个 | defaultScale=3.75, **currentScale=7.0** |
| Grid Fin L Titanium 栅格翼 | 4 | mass=0.7t/个 | **currentScale=200**（2× 面积） |
| HotGasThruster-U 热气姿控 | 4 | 15 kN/个，Methalox 构型 | currentScale=100 |
| sepIB 分离固体火箭 | 4 | 用于级间分离 | currentScale=100 |
| launchClamp1 发射卡钳 | 6 | 地面固定 | currentScale=100 |

### 级间段与航电

| 组件 | 数量 | 关键参数 |
|------|------|---------|
| nflv-cluster-mount-lower-75-1 | 1 | 7.5m 发动机簇安装板（19 节点），AO mass×0.4 |
| KCLV.CZ7.Interstage1-2 | 1 | 级间段，10 kN 分离力 |
| eisenhower.angara.urm2avionics | 1 | 航电模块，TweakScale=3.6→**currentScale=7.5**，EC=72,338 |

---

## 三、CZ-16A 第二级（Methalox 真空级）

| 参数 | 数值 |
|------|------|
| **发动机** | 3× KK SpaceX Raptor Vacuum |
| **单台推力** | 2,800 kN（真空），ISP 380 s（真空）/ 280 s（海平面）|
| **总推力** | **8,400 kN** |
| **推进剂** | LqdMethane 41.37% + LqdOxygen 58.63% |
| **单台质量** | 2.38 t（AO 配置） |
| **总干质量** | 7.14 t |
| **TweakScale** | defaultScale=2.5, currentScale=2.5 |

### 推进剂储箱

| 储箱 | MFT 容积 | 类型 | 内容 |
|------|---------|------|------|
| 主储箱 (P027) | **1,104,466 L** | BalloonCryo, util 100% | LqdMethane 456,918 + LqdOxygen 647,549 |
| RCS 储箱 ×2 | 各 153 L | ServiceModule | Hydrazine 各 152.7 |

### 姿控与航电

| 组件 | 数量 | 关键参数 |
|------|------|---------|
| nflv-rcs-integrated-4x-2 | 8 组（32 喷口） | 1 kN/喷口，Hydrolox 构型 |
| RCSBlock.v2 | 2 | Hydrazine 构型 |
| eisenhower.angara.urm2avionics | 2 | currentScale=2.25（EC=1,953）和 currentScale=3.75（EC=15,000） |
| radPanelLg 散热器 | 2 | 热控 |

### 展开式整流罩（非分离式）

| 组件 | 数量 | 关键参数 |
|------|------|---------|
| KzProcFairingSide1 | 4 片 | Procedural Fairings，**花瓣展开模式** |
| SSTUBase.Interstage | 1 | 级间桁架适配器 |

> **设计说明**：整流罩采用**展开式设计**（类似土星五号 SLA 面板），而非传统抛离式。TLI burn 完成后，整流罩花瓣展开并锁定，嫦娥飞船服务舱发动机点火，从展开的整流罩中分离、调头 180°，对接吴刚着陆器顶部，将着陆器从整流罩中拖出。此方案避免了传统抛离式整流罩可能产生的碰撞风险。

---

## 四、吴刚着陆器（下降/月面驻留级）

吴刚着陆器携带独立的 Hydrolox 推进系统、生命支持系统和月面着陆系统，支持 6 名乘员在月面活动 20 天。电力来源于液氢/液氧自然蒸发气体驱动的燃料电池（boil-off fuel cell）。

### 推进系统

| 参数 | 数值 |
|------|------|
| 主推进剂 | LqdHydrogen + LqdOxygen（Hydrolox） |
| 储箱 (P048) | **46,940 L**，BalloonCryo 型，util 100%，MLI 100 层 |
| LqdHydrogen 装载量 | 34,153 单位 |
| LqdOxygen 装载量 | 12,786 单位 |
| 姿控 | 8× nflv-rcs-integrated-4x-2（Hydrolox 构型，利用主储箱蒸发气体）+ 8× HotGasThruster-U（Methalox 构型） |

### 居住与指令

| 组件 | 参数 |
|------|------|
| sspx-habitation-375-2 | PXL-2 'Shelter' 深空居住模块，6 人，TweakScale=3.75 |
| command-ppd-1 | NF-Spacecraft 6 人指令舱，RCS 构型 MMH/NTO |
| 内置 LS | Oxygen 31,959, Food 316, Water 209, EC 18,360（MFT 1,000L） |

### 生命支持

| 储罐 | 内容 |
|------|------|
| proceduralTankTAC (P063) | Food 478, Oxygen 48,586, Water 316, EC 72,338 |
| HexCanLifeSupportLarge ×2 | TACLS 综合 LS 容器，各 currentScale=**200**（2× 缩放，4× 容积） |

### 着陆与对接

| 组件 | 数量 | 关键参数 |
|------|------|---------|
| KRE-FalconLegMk2-L | 4 | defaultScale=3.75, currentScale=3.75 |
| dockingPort2 | 1 | 标准对接端口 |
| telescopicLadderBay | 2 | 登月梯 |
| spotLight1.v2 | 2 | 月面照明 |
| radPanelLg | 2 | 热控散热 |

---

## 五、嫦娥飞船

嫦娥飞船由服务舱和返回舱两部分组成，服务舱使用 Hydrolox 主推进，RCS 采用 Hydrolox Hot Gas 构型（利用主储箱蒸发气体）。前向支持 6 人乘组飞行 15 天，或关闭大多数子系统后在轨无人驻留 210 天。电力同样来源于 boil-off fuel cell。

### 服务舱

| 参数 | 数值 |
|------|------|
| **发动机** | 3× CE-10 'Hecate' 液氢发动机 |
| **单台推力** | 70 kN（真空），ISP 480 s（真空）/ 100 s（海平面） |
| **单台质量** | 0.22 t（AO 配置） |
| **推进剂** | LqdHydrogen 72.76% + LqdOxygen 27.24% |
| **储箱 (P007)** | **42,452 L**，ServiceModule 型，util 93%，MLI 100 层 |
| LqdHydrogen | 30,888 单位 |
| LqdOxygen | 11,564 单位 |
| **姿控** | 4× nflv-rcs-integrated-4x-2（Hydrolox Hot Gas RCS） |
| **热控** | 2× radPanelLg |
| **航电** | eisenhower.angara.urm2avionics，currentScale=1.25，EC=335 |

### 返回舱

| 参数 | 数值 |
|------|------|
| 返回舱 | KCHS.Mengzhou.ReEntry.Module（梦舟），乘员 7 人，mass=4 t（原始） |
| 热盾 | KCHS.Mengzhou.Heat.Shield，Ablator=400 单位 |
| 引导伞 | KCHS.Mengzhou.Drogue.Chute |
| 主伞 | KCHS.Mengzhou.Main.Chute（70m 展开直径，RealChute） |
| 对接端口 | KCHS.TG.DockingPort（侧向，与着陆器对接） |
| RCS | Hydrazine 构型，380 单位 |
| 内置 LS | Oxygen 17,755, Food 175.5, Water 116.1, EC 25,000（MFT 800L） |
| TweakScale | free type, currentScale=**75**（缩小至 75%） |

### 发射逃逸系统

| 组件 | 关键参数 |
|------|---------|
| CZ-10 Escape Tower Lower | 固体逃逸塔下段，推力 500 kN，PBAN 2,500 单位 |
| CZ-10 Escape Tower Upper | 固体逃逸塔上段，推力 80 kN，PBAN 135 单位 |
| km.smart.time | 智能计时器：**发射后 2 分 45 秒自动触发逃逸塔分离**（Action Group Custom07） |

### 分离机构

| 组件 | 关键参数 |
|------|---------|
| Decoupler.3 | 分离服务舱与返回舱，currentScale=3.75 |
| nflv-decoupler-5-1 | 5m 分离器，currentScale=5.0 |

---

## 六、公共系统

### 电力架构

嫦娥飞船和吴刚着陆器均采用 **boil-off fuel cell** 供电方案：液氢和液氧储箱的自然蒸发气体被导入燃料电池反应生成电力，副产品为水（补充生命支持）。此方案无需专门的太阳能电池板，减少了展开机构的复杂性和月面尘埃污染风险。

各级航电模块内置分布式蓄电池作为缓冲：一级 72 kWh、二级 17 kWh、着陆器 72 kWh。

### 热控

各级均采用被动辐射散热（radPanelLg 面板），无主动冷却需求。第一级再入阶段依赖栅格翼气动散热和发动机喷管辐射。

---

## 七、动力系统汇总

| 发动机 | 数量 | 推进剂 | 推力/台 | ISP Vac | 质量/台 | 所属级 |
|--------|------|--------|---------|---------|---------|--------|
| Raptor 2 SL (R3 cfg) | 15 | Methalox | 2,920.6 kN | 351 s | 1.525 t | 一级 |
| KK Raptor Vacuum | 3 | Methalox | 2,800 kN | 380 s | 2.38 t | 二级 |
| CE-10 Hecate | 3 | Hydrolox | 70 kN | 480 s | 0.22 t | 飞船 SM |
| CZ-10 Escape Lower | 1 | PBAN 固体 | 500 kN | 300 s | 0.3 t | 逃逸塔 |
| CZ-10 Escape Upper | 1 | PBAN 固体 | 80 kN | 300 s | 0.3 t | 逃逸塔 |
| HotGasThruster-U | 16 | Methalox | 15 kN | 320 s | 0.225 t | 姿控 |
| sepIB | 6 | 固体 | — | — | — | 分离 |

---

## 八、推进剂总计

| 推进剂 | 装载总量 (单位) | 用途 |
|--------|----------------|------|
| **LqdMethane** | **~1,498,692** | 一级 + 二级 + 底部回收箱 |
| **LqdOxygen** | **~2,123,959** | 一级 + 二级 + 底部回收箱 |
| **LqdHydrogen** | **~65,041** | 嫦娥 SM + 吴刚着陆器 |
| **Hydrazine** | **~685** | RCS（返回舱 + 二级 Hydrazine RCS 储箱） |
| **PBAN** | **2,635** | 逃逸塔 |
| **Ablator** | **450** | 热盾 |

---

## 九、嫦娥 vs 吴刚 对比

| 参数 | 嫦娥飞船 | 吴刚着陆器 |
|------|---------|-----------|
| 主推进剂 | Hydrolox | Hydrolox |
| 主发动机 | 3× Hecate (210 kN) | （转移级使用 Hecate，下降使用 Raptor Vac 或其他） |
| RCS | Hydrolox Hot Gas | Hydrolox Hot Gas + Methalox Hot Gas |
| 电力 | Boil-off fuel cell | Boil-off fuel cell |
| 乘员 | 6 人 | 6 人 |
| 自持力（载人） | 15 天 | 20 天（月面） |
| 自持力（无人） | 210 天（低功耗驻留） | — |
| 特殊能力 | 高速再入返回 | 动力下降 + 月面着陆 |

---

## 十、ArmorOverhual 配置覆盖

| 零件 | AO 文件 | 主要修改 |
|------|---------|---------|
| SEP.23.RAPTOR2.SL.RC | SEP_Engines.cfg | RF 转换，Raptor 2/3 CONFIG，mass=1.525t，thrust=2,393/2,921 kN |
| KK.SpaceX.Raptor.Vac | Raptor.cfg | Methalox，mass=2.38t，thrust=2,800 kN，ISP 380/280 |
| cryoengine-hecate-1 | CryoEngines.cfg | Hydrolox，mass=0.22t，thrust=70 kN，ISP 480/100 |
| HotGasThruster-U | ColdGasThruster.cfg | Multi-CONFIG，TweakScale free type，mass×1.5 |
| nflv-rcs-integrated-4x-2 | integrated_rcs.cfg | Hydrolox/Methalox B9 切换，fuel cell 集成 |
| KRE-FalconLegMk2-L | SpaceXLeg.cfg | mass=0.55t，TweakScale defaultScale=3.75 |
| eisenhower_angara_urm2avionics | Avionics.cfg | rescaleFactor=1.44，MFT 内置 EC |
| proceduralTankLiquid | TankPrice.cfg | costPerkL=14.375 |
| nflv-cluster-mount-* | Structural.cfg | cost×0.4, mass×0.4 |
| sspx-habitation-375-2 | station-habitation.cfg | LS 60 crew-day rating，MFT 集成 |

---

> **文档说明**：本报告基于 craft 文件、ArmorOverhual 配置文件及 GameData 原始零件配置生成，并经过人工审核修正。推进剂单位值直接从 craft RESOURCE/MFT 块提取。EC 能量单位：1 EC = 1 Wh（项目约定）。
