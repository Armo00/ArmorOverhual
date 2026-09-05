---
name: arc-reactor-module
description: ArmorOverhaul 自定义 ModuleArcReactor 规则 — 功率 slider、负载匹配稳压及反应堆尺寸系列 (2026-08-20)
metadata:
  type: project
---

# Arc Reactor Module (ModuleArcReactor)

自定义 C# 模块（ArmorOverhaul 插件），继承 stock `ModuleResourceConverter`，用于 Arc Reactor 10（`phoenixreactor-0625`）。

**源码**：`Source/ArmorOverhaul/ModuleArcReactor.cs`（用户提交 b2db715，2026-08-11）
**零件**：`Mods/Phoenix Industry/ArcReactor.cfg`（10.8 GW 金属氢聚变反应堆）

## 核心行为

1. **功率 slider**：PAW 里 `Output Power` 0-100%（`powerPercentage`，persistent，UI_FloatRange，步进 `sliderStep`）。输入/输出 Ratio 是 100% 功率的速率，slider 线性缩放整个 recipe；0% 时 converter 保持激活但零消耗零产出
2. **负载匹配稳压**：对 `blocked_when_full = true` 的输出（regulated output），当存储接近 `FillAmount`（默认 0.95 = 95%）目标时自动降低输出匹配实际用电负载；**无启停水位、无定时器**（EC 满不会硬停，而是平滑降功率；EC 低时全速）
3. **功率显示**：飞行中 PAW 显示实际电功率（`1 EC/s = 3.6 kW`），自动 W/kW/MW/GW 单位
4. **状态语义**：`Offline`（停止）/ `Output Disabled`（slider 0%）/ `Standby`（被负载匹配限制到 0）/ `Starting`
5. 保留 stock start/stop/toggle 事件；**不加**预设功率动作

## OUTPUT_RESOURCE 惯例

| 字段 | 值 | 语义 |
|---|---|---|
| `blocked_when_full = true` | EC 输出 | regulated 输出，参与负载匹配；内部翻译为 `DumpExcess = false`（stock 存储上限作为防溢出兜底） |
| `blocked_when_full = false`（默认） | 副产物（LqdHelium）| 未 regulated，`DumpExcess = true` 溢出排空，永不阻塞反应堆 |
| `FlowMode = ALL_VESSEL` | EC 输出 | 输出流入全船电网 |
| `FlowMode = NO_FLOW` | 输入（ArcElement）| 燃料不自动流动，仅本罐 |

## 参数模板

```
MODULE
{
    name = ModuleArcReactor
    ConverterName = Arc Reactor
    StartActionName = Start Arc Reactor
    StopActionName = Stop Arc Reactor
    ToggleActionName = Toggle Arc Reactor
    powerPercentage = 100
    sliderStep = 1
    FillAmount = 0.95        // 稳压目标
    AutoShutdown = false
    GeneratesHeat = false
    UseSpecialistBonus = false
    INPUT_RESOURCE { ResourceName = ArcElement, Ratio = <100%速率>, FlowMode = NO_FLOW }
    OUTPUT_RESOURCE { ResourceName = ElectricCharge, Ratio = <100%速率>, FlowMode = ALL_VESSEL, blocked_when_full = true }
    OUTPUT_RESOURCE { ResourceName = LqdHelium, Ratio = <质量守恒>, blocked_when_full = false }
}
```

## Arc Reactor 10 设计值

- 10.8 GW = 3,000,000 EC/s（100%）；消耗 2.296296e-5 U/s（1.98 U/day）
- 装载 100 U = 75 kg（ArcElement 0.75 kg/U）→ 满功率 ~50 天
- LqdHelium 9.589698e-5 U/s（质量守恒 0.9945），溢出排空
- 40 kg / 0.625m（rescale 0.25）→ VABO `nuclearReactors`
- 聚变比能量 6.3e14 J/kg（能量法：消耗 × 0.75 kg × 6.3e14 = 输出 W）

## Reactor Size Family

All variants are implemented in `Mods/Phoenix Industry/ArcReactor.cfg` and use
`phoenixreactor-0625` as the baseline. Diameter is scaled uniformly; dry mass,
power, resource storage, and resource flow scale with the cube of diameter.
This preserves power density and approximately 50.4 days of full-power runtime.

| Part | Diameter | Volume factor | Dry mass | Power | Entry cost |
|---|---:|---:|---:|---:|---:|
| phoenixreactor-0625 | 0.625 m | 1 | 0.04 t | 10.8 GW | 3,000,000 |
| phoenixreactor-125-v2 | 1.25 m | 8 | 0.32 t | 86.4 GW | 3,150,000 |
| phoenixreactor-1875 | 1.875 m | 27 | 1.08 t | 291.6 GW | 3,300,000 |
| phoenixreactor-250 | 2.5 m | 64 | 2.56 t | 691.2 GW | 3,600,000 |
| phoenixreactor-375 | 3.75 m | 216 | 8.64 t | 2.3328 TW | 4,050,000 |
| phoenixreactor-500 | 5 m | 512 | 20.48 t | 5.5296 TW | 4,500,000 |

Manufacturing `cost` scales with volume. `entryCost` rises slowly with size and
must never exceed 1.5 times the 0.625 m baseline.

## Related

- [[nuclear-reactor-rules]] — 裂变反应堆（SystemHeat 模式）规则；Arc Reactor 10 用自定义模块而非 SystemHeat
- [[resource-energy-rules]] — 1 EC/s = 3.6 kW
