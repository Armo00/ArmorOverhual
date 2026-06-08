---
name: ksp-node-autoscale
description: KSP rescaleFactor automatically scales node_stack and node_attach positions — no manual adjustment needed
metadata:
  type: reference
---

# KSP rescaleFactor 自动缩放节点坐标

KSP 的 `rescaleFactor` 不仅缩放视觉模型（.mu），**也自动缩放 `node_stack_*` 和 `node_attach` 的位置坐标**。

## 含义

- 当 rescaleFactor 从 1.0 → 1.5 时，节点位置自动 ×1.5
- **不需要在补丁中手动调整 node 坐标**
- 如果手动进行了 ×1.5 调整，实际上会导致节点被双倍缩放（1.5² = 2.25），节点会飘在零件外部

## 来源

Dr Armor 于 2026-06-08 MK2 尺寸调整讨论中确认。

## How to apply

当调整零件的 rescaleFactor 时，**永远不要**调整 node_stack_* 和 node_attach 的坐标值。让 KSP 自动处理。
