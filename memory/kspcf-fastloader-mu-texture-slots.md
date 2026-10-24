---
name: kspcf-fastloader-mu-texture-slots
description: KSPCommunityFixes FastLoader rejects any .mu that declares more textures than its materials reference — a hard model load failure
metadata:
  type: project
---

KSPCommunityFixes 的 **FastLoader** 补丁用自己的 `MuParser` 完整替换了原版 `PartReader` 的 .mu 解析器（`KSPCommunityFixes/Library/MuParser.cs`，1.41.1 实测）。2026-09-20 在 KCLV_CZ12B 上定位到它的一处硬缺陷：

`ReadTextures()` 读入 `texCount` 后，与 `textureDummies.Count`（**被材质实际引用的贴图槽数量**）比较，不等则打印 `TextureError: <texCount> <slotCount>` 并 **直接 return，不消费 texCount 个贴图条目**。`ReadChild()` 的 `while (index < dataLength)` 循环随后从错位处继续读「条目类型」，把贴图条目的 type 字段（0 = Diffuse）当成 `ET_CHILD_TRANSFORM_START` 递归下去，最终在嵌套的 `ReadVector3()` 撞到文件尾，抛出 `Unable to read beyond the end of the data`。

结果：**该模型整个零件加载失败**（`LOAD FAILED : MU model load error`），零件完全不出现在游戏中，而不是仅仅丢失贴图。日志特征：`[ERR] TextureError: N M` 紧接 `[ERR] Model <dir> error: Unable to read beyond the end of the data`（栈为 `Parse → ReadChild → ReadChild → ReadVector3`），随后 `PartCompiler: Cannot clone model ... as model does not exist`。

**Why:** 这不是模型文件损坏。抽查全游戏 400 个其它 mod 的 .mu，**没有任何一个**存在「声明贴图数 > 实际引用槽数」的情况；而 `io_object_mu`（Blender 社区参考解析器，`io_object_mu_continued` 插件）能 100% 完整解析这些文件。声明了却未被任何材质引用的贴图在 MU 格式里是合法的，原版解析器不会因此报错——是 FastLoader 的相等性假设不成立，且失败时破坏了字节流对齐。上游 master 分支（已重构为 `Library/Model/MuModelCompiler.cs`）同样保留了这个提前 return。

**How to apply:** 写 .mu 导出器时，**贴图条目只写被材质引用到的那些，且槽位下标从 0 连续编号**，保证 `texCount == 引用槽数`。做零件模型时不要让 `textureProperties` 指向不存在的槽，也不要声明用不到的贴图。1.41.1 的 `Settings.cfg` 里没有 FastLoader 开关，无法用配置绕开，只能在模型侧修。
