# Long-Term TODO List

> **记录规则：** 此文件仅记录 Dr Armor 明确要求纳入长期规划的优化项目。日常小修小补、单零件参数调整不在其中。每当用户说"把这个记到长期列表里"或类似表述时，才追加条目。
>
> **本文件是唯一的 to-do 清单**（2026-08-05 起，替代 memory/todo-list.md）。
>
> **优先级体系（P0–P4）：**
> - P0：当前主线，方案已就绪，立即执行
> - P1：高优先，规则已定或可顺带执行
> - P2：审计/审查类，先出报告再定方案
> - P3：大工程/规则设计，需先讨论方案
> - P4：低优先级，长期规划/遗留项

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

**优先级：P1** ｜ **状态：**[ ] 待办（主体已完成，剩余收尾）

**规则：** [memory/nuclear-reactor-rules.md](../memory/nuclear-reactor-rules.md)（2026-08-06 更新：丰度统一 **19% HALEU**）

**已完成（2026-08-06）：**
- KPBS 反应堆：Ratio 2.35e-8、装载 12u（16.2 年）、注释规范
- NFE 8 个反应堆：质量 ×4 / rescale ×2 / 热功率 ×4，效率 0.24–0.432 定档，电功率取整 EC/s，19% HALEU Ratio + 按寿命装载（`Mods/NF-Electrical/Reactors.cfg`）

**剩余（下一步立刻调整）：**
- M2X_Reactor：45 MW 热 / 4% 效率异常（密度 12,857 kW/t），NFE 补丁与项目补丁冲突（NFE 删 ModuleResourceConverter 导致项目 FINAL 修改失效），需重定热功率/效率/燃料
- 核热引擎 NTR（M2X_AtomicJet / M2X_Pluto / KerbalAtomics）：按 19% HALEU 规则适配

**记录日期：** 2026-06-20（2026-08-06 更新）

---

## 温室批量调整

**优先级：P1** ｜ **状态：**[ ] 待办

**规则：** [memory/greenhouse-rules.md](../memory/greenhouse-rules.md)

按规则批量审计并调整所有 mod 中的温室：
- 审计所有温室当前 EC Ratio、支持人数、单人均耗
- 按 12–20 kW/person 范围调整 EC Ratio
- 更新 descriptions 标注 kW 和 kW/person

**记录日期：** 2026-06-20

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
