---
name: todo-list
description: Active TODO list for the ArmorOverhual project — prioritized tasks awaiting execution (2026-06-08)
metadata:
  type: project
---

# TODO List (2026-06-08)

## P0 — 当前

### 8. MK2 家族尺寸调整
MK2 Expansion 整个零件族的尺寸需要调整。
- [x] 方案讨论 → 已确认规则文档 [[mk2-scale-rules]]
- [x] 零件清单 → 93 个零件（70 非引擎 + 23 引擎另议），已录入 [[mk2-scale-rules]]
- [ ] 实施补丁编写

## P0 — 历史（已完成 2026-06-08）

### ~~4. 引擎 VABO 分类查漏~~ ✅
- 新增 `SeparationMotors` 子分类，迁移 17 个分离/反推/ullage 电机
- MK2Expansion 补漏 5 个喷气引擎
- NovaPunch 补漏 2 个 Thor 着陆引擎
- CA_SRB4 补漏 solidEngines

### ~~5. KSP Stock 分类错误修复~~ ✅
- CZ-10 Booster Separator: Engine → Coupling
- KK Raptor Vac Mini/LEM: Propulsion → Engine
- CA_SRBnose: Engine → Aero
- Falcon Landing Leg: 配置文件无异常，疑为 VABOrganizer 运行时问题

## P1 — 中高优先级

### 1. MK2E 引擎调整
MK2 Expansion 中需要调整的引擎：
- 所有 JetEngine 需要调整比冲
- LE-25 "Corgi" 需要修改引擎配置
- R-44 需要修改引擎配置
- X-44 需要调整比冲

### 6. 错误适配检查
检查是否有 mod 存在错误适配（如 Starship Expansion Project）。找出问题，整理报告，和用户讨论修改方案。

## P2 — 中等优先级

### 2. Part 类 Mod 适配完整性审计
检查原始 mod 文件夹中：
- 有哪些 part 类 mod 完全未进行适配
- 有哪些 part 类 mod 只进行了部分适配
整理成完整的审计报告。

## P3 — 大工程，需先讨论

### 3. KPBS 整体尺寸调整
KPBS（Kerbal Planetary Base Systems）目前是坎星尺寸（~1.25m 级高度），需要调整为真实尺寸。这是大工程，需要先 brainstorming 讨论方案，制定计划后再动工。

## P4 — 低优先级，长期规划 / 遗留项

### 7. 燃料箱分类规则重新制定
需要重新制定燃料箱的分类规则。属于长期规划性质的工作。

### 9. Jet Engine 体系 Overhaul（遗留项）
对整个 Jet Engine ISP 体系做系统性翻修。需参考真实 TSFC→ISP 数据，按发动机类型（大涵道涡扇/小涵道涡扇/涡喷/冲压/超燃冲压/涡桨/组合循环/核热喷气/VTOL涡扇）分别制定 ISP 区间，然后批量修改所有 mod 中的空气喷气发动机参数。讨论记录见 2026-06-08 对话。
