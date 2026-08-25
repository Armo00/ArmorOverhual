# Long-Term TODO List

> **记录规则：** 此文件仅记录 Dr Armor 明确要求纳入长期规划的优化项目。日常小修小补、单零件参数调整不在其中。每当用户说"把这个记到长期列表里"或类似表述时，才追加条目。
>
> **本文件是唯一的 to-do 清单**（2026-08-05 起，替代 memory/todo-list.md）。
>
> **优先级体系（P0–P4，另有 P5/P6）：**
> - P0：当前主线，方案已就绪，立即执行
> - P1：高优先，规则已定或可顺带执行
> - P2：审计/审查类，先出报告再定方案
> - P3：大工程/规则设计，需先讨论方案
> - P4：低优先级，长期规划/遗留项
> - P5：更低优先级，规则已定但延后执行（2026-08-09 新增）
> - P6：仅记录待规划（2026-08-06 新增）

---

## S2Pila 重构（三推进剂 → 氢氧单平台多版本）

**优先级：P0** ｜ **状态：**[x] 已完成（2026-08-05）

按 `copilot report/03_S2Pila专项重构建议.md` 方案执行：
- 移除 Methalox / Kerolox 两个 CONFIG，仅保留 Hydrolox
- 拆为 Block1（通用）/ Vac（真空优化）/ Boost（推力增强）三版本
- 清理 PLUME 特效命名（当前 Hydrolox 档位误用 Hypergolic 特效）
- 消除"更高推力 + 更轻质量"的跨推进剂矛盾

**实际落地：** GG 开式循环 46:1（1.7m/0.25m），Standard 14.3MPa 900–1400kN / Boost 15.7MPa 1400–1550kN / Reusable 12.9MPa 600–1250kN，TWR 57.1/60.8/53.1，价格 11000/11500/10500，heatProduction 15。

**记录日期：** 2026-08-05

---

## 雷霆-RS（LT-RS）燃料修正

**优先级：P0** ｜ **状态：**[x] 已完成（2026-08-05）

`Mods/KIU/ChineseCommercialRocketEngines/methalox_engines.cfg` 中 LT-RS 100%/110% 档位：
- 描述为"液氧煤油"（现实中雷霆-RS 确为煤油机），但 PROPELLANT 是 LqdMethane + LqdOxygen
- 方案待定：按现实改为 Kerosene + LqdOxygen（需同步调整推力/Isp/热参数），或改文案为液氧甲烷

**实际落地：** 改回 Kerosene 37.7/LOX 62.3（O/F 1.65），ISP 337/282（官方地面 282s + 真空 1480kN），独立为 KCLV_CCRE_1300K，TEA-TEB 点火限 10 次/飞行，Waterfall kerolox-RD170。

**记录日期：** 2026-08-05

---

## 新增 YF-219 引擎

**优先级：P1** ｜ **状态：**[x] 已完成（2026-08-05）

在 `Mods/KIU/ChineseCommercialRocketEngines/methalox_engines.cfg` 中新增 YF-219（参考现有 YF-209 的录入方式）：
- 140 吨级可复用液氧甲烷发动机，燃气发生器循环（开式循环）
- 真空推力约 140t；真空版 YF-219E 约 160t；海平面型约 140t
- 装机对象：长征十号乙（二级 1 台）、长征十号丙（一级 9 台 + 二级 1 台 YF-219E）

**实际落地：** KCLV_CCRE_1500M 改造为 YF-219（Dev 318s/152t + E 329s/160t），YF-219EV（355s/162t）追加至 TQ-15A，节流统一 27%（402.6kN），价格 2350/2500/2600。

**记录日期：** 2026-08-05

---

## 核反应堆批量调整

**优先级：P1** ｜ **状态：**[x] 已完成（2026-08-06；NTR 拆出为 P5 单独条目）

**规则：** [memory/nuclear-reactor-rules.md](../memory/nuclear-reactor-rules.md)（2026-08-06 固化：19% HALEU、密度 1100-1300 kW/t、kW/cost 基准、散热 T⁴ 曲线、Patch 惯例）

**已完成（2026-08-06）：**
- KPBS 反应堆：Ratio 2.35e-8、装载 12u（16.2 年）、cost 200000（0.01 kW/cost）
- NFE 8 个反应堆：质量 ×4 / rescale ×2 / 热功率 ×4，效率 0.24–0.432 定档，电功率取整 EC/s，19% HALEU Ratio + 按寿命装载（`Mods/NF-Electrical/Reactors.cfg`）
- NFE 修复：模块整体重写（@key 选择器不可靠）、ResourceName 选择器、TweakScale defaultScale 显式覆盖、tiny B9PartSwitch 节点 ×2
- M2X_Reactor 重设计：4t / 800kW 热 / 80 EC/s（36%）/ 200 kW/t / 集成散热 800kW@800K / cost 160000（0.005 kW/cost）（`Mods/SystemHeat/MK2Expansion/M2X_Reactor.cfg`）
- Squad 散热器：600K 曲线（×5）—— radPanelEdge 65 / Sm 25 / Lg 125 / foldingRadSmall 125 / Med 500 / Large 2500；900K 高温点（×25.6，T⁴）

**记录日期：** 2026-06-20（2026-08-06 更新，NTR 已拆出单独条目）

---

## 核热引擎 NTR 适配

**优先级：P5** ｜ **状态：**[ ] 待办

按 19% HALEU 规则适配核热引擎（自核反应堆批次拆出，2026-08-09 降为 P5）：
- M2X_AtomicJet / M2X_Pluto（MK2E）：NFE 模式 HeatGeneration 55/65 MW 异常，燃料装载未定义（原版 EU maxAmount = 0）
- KerbalAtomics 核热引擎：按 19% HALEU 燃料规则适配

**记录日期：** 2026-08-09

---

## 温室批量调整

**优先级：P1** ｜ **状态：** [x] 已完成（2026-08-09）

**规则：** [memory/greenhouse-rules.md](../memory/greenhouse-rules.md)

按规则批量审计并调整所有 mod 中的温室：
- 审计所有温室当前 EC Ratio、支持人数、单人均耗
- 按 12–20 kW/person 范围调整 EC Ratio（线性插值 1人→20 / 15人→12）
- 更新 descriptions 标注 kW 和 kW/person
- 9 个零件（KPBS ×2 + SSPXR ×6 + Phoenix ×1）全部调整并分配 VABO greenhouses 分类

**记录日期：** 2026-06-20

---

## RTG 全局扫描与调整

**优先级：P1** ｜ **状态：**[x] 已完成（2026-08-09，commit 7222245）

RTG（放射性同位素热电发电机）全局扫描：
- 扫描所有 mod 中的 RTG 零件
- 参数调整（发热/发电功率、效率、燃料）
- VABO 新增分类

**记录日期：** 2026-08-09

---

## 方舟反应堆调整

**优先级：P1** ｜ **状态：**[ ] 待办

（2026-08-09 自 P6 升级）方舟反应堆：
- 参数调整
- 新增 part
- 描述调整
- SystemHeat 适配
- VABO 新增分类

**记录日期：** 2026-08-09

---

## Part 类 Mod 适配完整性审计

**优先级：P2** ｜ **状态：**[ ] 待办

扫描 GameData 所有 part 类 mod，标记每个 mod 的适配状态：
- 完全未适配
- 部分适配（列出缺哪些类型）
- 已完成

**记录日期：** 2026-06-08

---

## SEP (Starship Expansion Project) 适配审查

**优先级：P2** ｜ **状态：**[ ] 待办

检查 SEP 是否存在错误适配，整理报告并讨论修改方案。

**记录日期：** 2026-06-09

---

## KPBS 生产零件 Converter Rate 审核

**优先级：P2** ｜ **状态：**[ ] 待办

28 个生产零件的 converter rate 逐一确认 ×4 倍率是否合理。（原 KPBS 整体尺寸调整的遗留子项）

**记录日期：** 2026-06-19

---

## Jet Engine 体系 Overhaul

**优先级：P3** ｜ **状态：**[ ] 待办

参考真实 TSFC → ISP 数据，按发动机类型系统性翻修所有空气喷气发动机参数：
- 大涵道涡扇 / 小涵道涡扇 / 涡喷 / 冲压 / 超燃冲压
- 涡桨 / 组合循环 / 核热喷气 / VTOL 涡扇

**记录日期：** 2026-06-08

---

## 燃料箱分类规则重新制定

**优先级：P3** ｜ **状态：**[ ] 待办

属于长期规划性质。

**记录日期：** 2026-06-08

---

## 电推批量修正

**优先级：P6** ｜ **状态：**[ ] 待办

电推（离子推进器等）参数批量修正。

**记录日期：** 2026-08-06

---

## Plasma 推进器批量修正

**优先级：P6** ｜ **状态：**[ ] 待办

Plasma 推进器参数批量修正。

**记录日期：** 2026-08-06

---

## Crew Selection“上一次任务”排序

**优先级：P6** ｜ **状态：**[ ] 待规划

为 VAB/SPH Crew Selection 的 Available Crew 列表增加“按上一次任务排序”。第一版人员排序插件不实现此项，只实现原始顺序和姓名顺序。

已确认原版 `careerLog` / `flightLog` 没有可供不同宇航员比较的任务时间；将来需要通过存档级历史模块自行记录任务回收或发射时间。实施前还需确定：
- “上一次任务”按发射时间还是回收时间排序
- 从未执行任务、以及插件安装前没有历史记录的宇航员如何排列
- 是否允许使用近似数据回填旧存档，或只从插件安装后开始精确记录

**记录日期：** 2026-08-15

---

## RoboticsKJRCompat：Stock PAW Servo 锁定同步

**优先级：P4** ｜ **状态：**[ ] 已遗留（当前有可用绕行方案）

Stock 零件右键菜单中的 `Locked` 开关会改变 Servo 字段显示，但在当前
mod 环境中不会同步触发载具级 Auto Strut ACTIVE/SUSPENDED 状态。管理窗口
和动作组的锁定/解锁均能正常同步，可继续使用。

已尝试 BaseField、UIPartActionFieldItem 和 UI_Control 三条 PAW 回调路径，
均未解决。详细诊断与后续恢复调查前应增加的观测字段见
[`memory/robotics-kjr-compat.md`](../memory/robotics-kjr-compat.md)。除非 Dr Armor
明确重新启动调查，否则不再继续修改。

**记录日期：** 2026-08-23
