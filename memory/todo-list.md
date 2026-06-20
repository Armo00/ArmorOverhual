---
name: todo-list
description: Active TODO list for the ArmorOverhual project — prioritized tasks awaiting execution (2026-06-09)
metadata:
  type: project
---

# TODO List (2026-06-20)

## P0 — 历史（已完成 2026-06-09）

### ~~8. MK2 家族尺寸调整~~ ✅
MK2 家族 rescaleFactor ×1.5，覆盖 93 个零件（70 非引擎 + 23 引擎）。
- [x] 方案讨论 → 规则文档 [[mk2-scale-rules]]
- [x] 零件清单 → 93 个零件
- [x] 非引擎补丁：17 Squad + 44 MK2E + 9 SpaceTux = 70 零件
- [x] 引擎补丁：23 引擎基础参数 + 推力/热产 + ISP
- [x] IVA 缩放：11 个 crewed 零件
- [x] 引擎数据导出 → EngineDatabase_20260609.xlsx

### ~~1. MK2E 引擎调整~~ ✅
- [x] 所有 JetEngine ISP 调整为 stock×1.15
- [x] LE-25 "Corgi" RF CONFIGs 缩放 (maxThrust×2.25, heat×2.1)
- [x] R-44 "Mongrel" RF CONFIGs 缩放
- [x] X-44 M.A.T.T.O.C.K. ISP 修正 + 推力缩放
- [x] 全部 23 引擎 rescaleFactor + mass/cost/entryCost + thrust/heat + ISP

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

### 6. 错误适配检查
检查是否有 mod 存在错误适配（如 Starship Expansion Project）。找出问题，整理报告，和用户讨论修改方案。

## P1 — 新任务 (2026-06-20)

### 10. 核反应堆批量调整
按照 [[nuclear-reactor-rules]] 规则，批量调整所有 mod 中的核反应堆：
- [ ] 审计所有反应堆的当前热功率/电功率/效率
- [ ] 按 0.216-0.432 效率范围调整 ElectricalGeneration
- [ ] 按 3% 丰度公式重新计算并设置 EnrichedUranium Ratio
- [ ] 标注堆芯装载量和运行时间

### 11. 温室批量调整
按照 [[greenhouse-rules]] 规则，批量调整所有 mod 中的温室：
- [ ] 审计所有温室的当前 EC Ratio、支持人数、单人均耗
- [ ] 按 12-20 kW/person 范围调整 EC Ratio（越大越低）
- [ ] 更新 descriptions 标注 kW 和 kW/person

## P2 — 中等优先级

### 2. Part 类 Mod 适配完整性审计
检查原始 mod 文件夹中：
- 有哪些 part 类 mod 完全未进行适配
- 有哪些 part 类 mod 只进行了部分适配
整理成完整的审计报告。

## P3 — 大工程，需先讨论

### 3. KPBS 整体尺寸调整 ✅ (基本完成 2026-06-19)
- [x] rescaleFactor ×2 — 全部 143 零件
- [x] mass/cost ×4，MFT volume ×8
- [x] 维生资源 crew-day 重新计算
- [x] 电池/反应堆/温室专项调整
- [ ] 28 生产零件 converter rate 待逐一审核

## P4 — 低优先级，长期规划 / 遗留项

### 7. 燃料箱分类规则重新制定
需要重新制定燃料箱的分类规则。属于长期规划性质的工作。

### 9. Jet Engine 体系 Overhaul（遗留项）
对整个 Jet Engine ISP 体系做系统性翻修。需参考真实 TSFC→ISP 数据，按发动机类型（大涵道涡扇/小涵道涡扇/涡喷/冲压/超燃冲压/涡桨/组合循环/核热喷气/VTOL涡扇）分别制定 ISP 区间，然后批量修改所有 mod 中的空气喷气发动机参数。讨论记录见 2026-06-08 对话。
