---
name: variable-isp-throttle-resources
description: ModuleVariableIspThrust 的油门联动输入/输出资源规则与 LA-151 配置 (2026-08-20)
metadata:
  type: project
---

# Variable ISP Throttle Resources

`Source/ArmorOverhaul/ModuleVariableIspThrust.cs` 支持零个或多个
`INPUT_RESOURCE` 和 `OUTPUT_RESOURCE`。该机制用于和发动机油门直接关联、但不应参与
KSP 推进剂质量流量计算的反应堆燃料、催化剂与副产物。

## 速率语义

- `ratio` 是 100% 油门下的绝对速率，单位为 U/s
- 实际速率仅为 `ratio * ModuleEnginesFX.currentThrottle`
- 速率与 ISP、推力档位、推进剂质量流量和 `performanceSetting` 无关
- 只在飞行场景、目标发动机已点火且可工作时按 physics tick 处理
- 不再通过 `PROPELLANT + ignoreForIsp` 表达固定消耗，因为该 ratio 仍会随主推进剂流量变化

## 配置规则

```cfg
INPUT_RESOURCE
{
    name = ArcElement
    ratio = 0.00002296296
    flowMode = NO_FLOW
}

OUTPUT_RESOURCE
{
    name = LqdHelium
    ratio = 0.00009589698
    flowMode = NO_FLOW
    dumpExcess = true
}
```

- 支持小写字段，也兼容 `ResourceName`、`Ratio`、`FlowMode`、`DumpExcess`
- `flowMode` 省略时采用资源定义的默认流动模式
- 所有输入组成同一 recipe；任一输入不足时处理最后一个部分 tick，然后关闭发动机
- `dumpExcess = true` 为输出默认值，满仓时排空溢出且不停机
- `dumpExcess = false` 时，无法完整存储输出会在该 tick 后关闭发动机
- 同一种资源不得重复配置，也不得同时作为输入和输出

## LA-151

`SquadPartsOverhaul/Engine/LA-151.cfg` 使用该机制：

- ArcElement：`0.00002296296 U/s`，100 U 在全油门下约使用 50.403 天
- LqdHelium：`0.00009589698 U/s`，允许溢出排空
- Water 是唯一的发动机 `PROPELLANT`
- TVC：`ModuleGimbal` 绑定 ReStock LV-N 的 `thrustTransform`，三轴 `+/-15` 度，响应速度 16；ReStock 模型没有独立喷管 gimbal transform，因此羽流会偏转，但喷管模型不会可见摆动
