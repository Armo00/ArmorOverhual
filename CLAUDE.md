# ArmorOverhual

KSP 半真实化 overhaul 项目。对 50+ 个 mod 的零件参数进行面向真实方向的调整，目标介于原版 KSP 和 Realism Overhaul 之间。
- 注意，你应当在每次回答时，都应当以Dr Armor来称呼用户。这是一个无论如何也要实现的要求。

## 核心原则

- 参数配置优先：优先通过调整 cfg 参数来补缺口。模型文件（.mu）一般不修改
- 数据驱动：引擎参数的调整需要同步记录到 Engine Database 电子表格。也可先在电子表格中做规划，再反写到 cfg
- 不覆盖现有定位：新 mod 入库时，先检查工作区已有的同类零件分布，避免重复定位
- 补丁必须用 `:NEEDS` 守卫，避免在未安装目标 mod 时污染游戏
- 完成一轮适配或修改后，向用户请示是否需要 git 提交。如用户确认，则根据修改内容进行提交并推送到远程

## 目录结构

```
zzzArmorOverhual/
├── GlobalSettings/           # 跨 mod 的全局补丁
│   ├── AddTech.cfg           # 科技树节点
│   ├── Fuel_Conversions.cfg  # RF 燃料箱自动转换（兜底方案）
│   ├── MJ&SAS.cfg            # 全局 MechJeb + SAS
│   ├── RO_Resources.cfg      # 资源定义
│   ├── RSSScienceDefs.cfg    # 科学定义
│   ├── VABO.cfg              # VABOrganizer bulkhead 定义
│   └── tweakscale.cfg        # TweakScale 全局设定
├── Mods/
│   ├── VABO/                 # 统一管理所有 mod 的 VABOrganizer 分类
│   │   ├── a_general_settings.cfg  # 自定义子分类定义
│   │   └── <ModName>.cfg     # 每个 mod 一个文件
│   ├── <ModName>/            # 每个 mod 独立的适配目录，按零件类型分包
│   ├── RemoteTech/           # RemoteTech 补丁，每个 mod 一个文件
│   └── Waterfall/            # Waterfall 羽流配置，按 mod 分子目录
└── SquadPartsOverhaul/       # Squad 原版零件模型/material 覆盖
```

## 适配类别

新 mod 入库时，按以下顺序检查并实施适配：

### 第一层：基础适配（几乎所有 mod 都要做）

**1. VABOrganizer 分类**
- 应在其他专项适配完成后再做，依据修改后的实际属性来分配分类
- 子分类和 bulkhead 定义有三个来源，做分类前应先查看：
  1. VABOrganizer 本体：`GameData/VABOrganizer/Subcategories/*.cfg` 和 `bulkheadProfiles.cfg`
  2. KIU 补充：`GameData/KIU/Common/VABO_size.cfg`
  3. 项目自定义：`Mods/VABO/a_general_settings.cfg` 和 `GlobalSettings/VABO.cfg`
- 在 `Mods/VABO/<ModName>.cfg` 中为新 mod 的零件分配 `organizerSubcategory`

**2. 零件基础属性**
- cost / entryCost：调整为合理值
- mass：调整为真实值（吨）
- rescaleFactor：如有需要，调整零件在游戏中的显示尺寸
- maxTemp / skinMaxTemp
- crashTolerance
- bulkheadProfiles
- title / description / manufacturer / tags / category
- TechRequired：分配合理的科技树节点
- 补丁通常用 `:FINAL` pass

### 第二层：按零件类型适配

**3. 发动机 → RF + ModuleEngineConfigs**
- 将 `ModuleEngines*` 改为 `ModuleEnginesRF`
- 添加 `ModuleEngineConfigs`，包含至少一个 CONFIG
- 每个 CONFIG 定义：推进剂比、ISP 曲线（key=0 真空, key=1 海平面）、推力（min/max）、heatProduction、massMult、ullage、pressureFed、ignitions、IGNITOR_RESOURCE
- 注释中标注设计定位和 TWR

**4. 燃料箱 → ModuleFuelTanks**
- 删除原版 RESOURCE，替换为 ModuleFuelTanks
- volume 根据真实容积设定
- `Fuel_Conversions.cfg` 中的 stock×5 转换仅为无精确数据时的兜底方案
- type 根据用途选择（Default / ServiceModule / Fuselage 等）
- basemass = -1 表示用零件自身 mass

**5. RCS → 真实推进剂**
- 转换为 RF 模式，提供多种推进剂 CONFIG（MMH/NTO, Hydrazine, Aerozine/NTO, UDMH/NTO 等）

**6. Waterfall 羽流配置**
- 在 `Mods/Waterfall/<ModName>/` 下按引擎创建模板文件

**7. RemoteTech**
- 在 `Mods/RemoteTech/` 下创建 `<ModName>RT.cfg`
- 参见编写约定中的 RemoteTech 部分

**8. Thermal / Heat Shield**
- ModuleAblator 参数：lossExp, lossConst, pyrolysisLossFactor, ablationTempThresh
- Ablator 资源量
- 热盾通常搭配 ModuleToggleCrossfeed（fuelCrossFeed = False）

**9. TACLS 生命支持**
- 载人舱添加 Food/Water/Oxygen 罐体，基于 CrewCapacity 和设计天数计算

**10. 结构件**
- 分离器/适配器/起落架：rescaleFactor, cost, mass, maxTemp, TweakScale

**11. TweakScale**
- 添加 `%MODULE[TweakScale]`，type = stack 或 free
- defaultScale 匹配零件 bulkhead 尺寸

### 第三层：特殊零件

**12. 对接端口** — bulkheadProfiles 调整
**13. 降落伞** — RealChute 配置
**14. Procedural Parts** — 程序化油箱类型配置
**15. 科学实验/实验室** — 科学定义和实验参数

## 适配工作流

### 新 mod 入库流程

1. **查看目标 mod 结构**：去目标 mod 目录和 GameData 中了解零件列表和原始 cfg 参数
2. **检查现有定位**：确认工作区已有的同类零件分布，避免新 mod 的参数定位和已有 mod 重叠
3. **基础属性调整**：在 `Mods/<ModName>/` 下按零件类型创建 cfg 文件
4. **RF 适配**（引擎/燃料箱/RCS）
5. **Waterfall 适配**：在 `Mods/Waterfall/<ModName>/` 下创建羽流模板
6. **RemoteTech 适配**：在 `Mods/RemoteTech/` 下创建 `<ModName>RT.cfg`
7. **其他专项适配**：TACLS / RealChute / ProceduralParts 等按需进行
8. **VABOrganizer 分类**：根据修改后的实际属性，在 `Mods/VABO/` 下创建 `<ModName>.cfg`
9. **引擎参数记录**：如有新增或修改引擎参数，使用项目根目录的 `EngineDatabaseTemplate.xlsx` 创建临时 xlsx 填入数据，由用户手动同步到真正的 Engine Database

### Engine Database 同步

- 真正的 Engine Database：`C:\Users\26549\OneDrive\文档\KSP Engine Tweak Chart.xlsx`，该文件为只读，绝对不可修改
- 项目模板：`EngineDatabaseTemplate.xlsx`（22 列，结构与真实 DB 的 Engine Database sheet 一致）
- 工作流：复制模板 → 填入新增/修改的引擎参数 → 用户手动同步到真实 DB

## 编写约定

### ModuleManager 语法

- `%`：key 存在则修改，不存在则新增
- `@`：修改已有 key（必须存在）
- `!`：删除 key 或整个模块
- `$`：引用其他 key 的值
- 补丁 pass 统一用 `:FINAL`，除非有明确的顺序依赖
- `:NEEDS[ModA,ModB]` 用于守卫所有依赖特定 mod 的补丁
- 多零件匹配用 `|` 分隔：`@PART[engineA|engineB]`

### 文件头注释

- 每个 cfg 文件第一行标注最后修改日期：`// Modified YYYY-MM-DD`
- 引擎配置块前可注释标注设计定位、参数来源和 TWR

### 引擎 RF 配置

- 使用 `ModuleEngineConfigs`，每个 CONFIG 代表一种推进剂/性能方案
- `origMass` 记录引擎基础干质量
- 每个 CONFIG 包含：minThrust, maxThrust, heatProduction, massMult, ullage, pressureFed, ignitions, IGNITOR_RESOURCE
- `atmosphereCurve`：`key = 0` 为真空 ISP，`key = 1` 为海平面 ISP
- PROPELLANT 中主燃料标记 `DrawGauge = True`
- 补丁中先全局清理：`!RESOURCE,*{}`，`!MODULE[ModuleAlternator],*{}`，`!MODULE[ModuleEngineConfigs],*{}`

### 燃料箱 RF 配置

- 使用 `ModuleFuelTanks`，`type` 按用途选 Default / ServiceModule / Fuselage
- `volume` 基于真实容积设定
- `basemass = -1` 表示用零件自身 mass 作为干质量
- 推进剂配比用百分比：`maxAmount = xx.xx%`
- 先清理：`!RESOURCE,*{}`，`!MODULE[ModuleFuelTanks]{}`

### RemoteTech

RemoteTech 用以下四个模块替代原版通讯系统：

| 模块 | 用途 |
|------|------|
| `ModuleSPU` | 主动控制核心，使零件成为 RT 控制站 |
| `ModuleRTAntennaPassive` | 内置被动天线，用 `OmniRange` 直接设定范围，始终在线 |
| `ModuleRTAntenna` | 独立天线，全向用 `Mode0OmniRange`/`Mode1OmniRange`，碟形用 `Mode0DishRange`/`Mode1DishRange` + `DishAngle` |
| `ModuleSPUPassive` | 标记零件带有 RT 天线但非控制站，防止其他 mod 重复添加冲突模块 |

适配模式（参考 RemoteTech 官方补丁 `GameData/RemoteTech/RemoteTech_Squad_*.cfg`）：

- **指挥舱/无人芯级**：删除 `ModuleDataTransmitter`，添加 `ModuleSPU` + `ModuleRTAntennaPassive`
- **独立天线零件**：删除 `ModuleDataTransmitter`，添加 `ModuleRTAntenna` + `ModuleSPUPassive`
- **载人零件有天线时**：额外补 `ModuleSPUPassive` 防止 mod 冲突
- 如有 `ModuleDeployableAntenna`，替换为 `ModuleAnimateGeneric`

### VABOrganizer

- 自定义子分类和 bulkhead 定义在 `Mods/VABO/a_general_settings.cfg` 和 `GlobalSettings/VABO.cfg`
- `ORGANIZERSUBCATEGORY` 定义子分类，`ORGANIZERBULKHEAD` 定义 bulkhead
- 分类补丁模板：
```
@PART[partName]:NEEDS[VABOrganizer]:Final
{
    %VABORGANIZER
    {
        %organizerSubcategory = <subcategoryName>
    }
}
```

### TweakScale

- 自由调节尺寸零件：`type = free`
- 标准堆叠零件：`type = stack`，`defaultScale` 匹配 bulkhead 尺寸
- 补丁中先清理已有 TweakScale 再重新添加
