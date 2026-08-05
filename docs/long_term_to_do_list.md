# Long-Term TODO List

> **记录规则：** 此文件仅记录 Dr Armor 明确要求纳入长期规划的优化项目。日常小修小补、单零件参数调整不在其中。每当用户说"把这个记到长期列表里"或类似表述时，才追加条目。

---

## 核反应堆批量调整

按 `memory/nuclear-reactor-rules.md` 规则，批量审计并调整所有 mod 中的核反应堆：
- 审计所有反应堆当前热功率/电功率/效率
- 按 0.216–0.432 效率范围调整 ElectricalGeneration
- 按 3% 丰度公式重新计算 EnrichedUranium Ratio
- 标注堆芯装载量和运行时间

**记录日期：** 2026-06-20

---

## 温室批量调整

按 `memory/greenhouse-rules.md` 规则，批量审计并调整所有 mod 中的温室：
- 审计所有温室当前 EC Ratio、支持人数、单人均耗
- 按 12–20 kW/person 范围调整 EC Ratio
- 更新 descriptions 标注 kW 和 kW/person

**记录日期：** 2026-06-20

---

## Part 类 Mod 适配完整性审计

扫描 GameData 所有 part 类 mod，标记每个 mod 的适配状态：
- 完全未适配
- 部分适配（列出缺哪些类型）
- 已完成

**记录日期：** 2026-06-08

---

## Jet Engine 体系 Overhaul

参考真实 TSFC → ISP 数据，按发动机类型系统性翻修所有空气喷气发动机参数：
- 大涵道涡扇 / 小涵道涡扇 / 涡喷 / 冲压 / 超燃冲压
- 涡桨 / 组合循环 / 核热喷气 / VTOL 涡扇

**记录日期：** 2026-06-08

---

## KPBS 生产零件 Converter Rate 审核

28 个生产零件的 converter rate 逐一确认 ×4 倍率是否合理。

**记录日期：** 2026-06-19

---

## SEP (Starship Expansion Project) 适配审查

检查 SEP 是否存在错误适配，整理报告并讨论修改方案。

**记录日期：** 2026-06-09

---

## 燃料箱分类规则重新制定

属于长期规划性质。

**记录日期：** 2026-06-08
