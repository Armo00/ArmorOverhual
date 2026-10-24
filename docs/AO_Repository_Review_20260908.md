# Armor Overhaul 审查报告

日期：2026-09-08。范围：当前 `GameData/zzzArmorOverhual` 工作树、安装环境的 ModuleManager 最终缓存、最近一次 KSP 日志及现有插件测试。

## 结论

当前最重要的问题不是继续增加零件调整，而是使“设计值 → 补丁结果 → 运行时行为”一致。已有明确的插件启动异常，也存在重复模块、默认发动机配置悬空和热参数互相矛盾。BDB 最近按类型整理和显式容量化的方向是正确的，不建议再推倒重做。

建议顺序：机械臂插件事件故障 → 燃料箱转换与重复模块 → 热参数一致性 → RF 默认配置 → IVA 依赖 → 工程与文档治理。不要把它们合成一次大改。

本轮只审查，未修改生产 cfg、DLL、存档、KK 位置，未启动游戏，未提交或推送。新增了本报告和游戏目录外于 AO 的 `.codex-analysis` 审查脚本。

## 证据范围与限制

- 自动扫描 676 个 cfg，识别 2682 个顶层 PART 补丁/定义块；其中 2446 个使用 FINAL，约 91.2%。这不是错误数量。
- 对当前缓存中的 3953 个 PART 节点检查名称、资源引用、燃料箱类型、默认 RF 配置、重复关键模块和热参数关系。
- 缓存时间为 2026-09-07 21:32:42；日志最后写入为 23:51:53。缓存代表上次补丁执行结果，不保证包含此后所有工作树修改。
- 配置分析使用简化 ConfigNode 解析器，不代替 ModuleManager；已对主要发现交叉核对源文件。没有把“没有发现括号问题”当成完整语法验证。
- 六组现有 PowerShell 测试均通过：CrewListSorter、LA151、ModuleArcReactor、ModuleRFInFlightConfigSwitcher、ModuleVariableIspThrust、RoboticsKJRCompat。它们不等同于飞行场景回归测试。
- 本轮不是每个模型、每种燃料配置、每个存档的实机验收。视觉效果、KK 地形接缝、再入与飞行输入仍需批准后的专项验证。

## P1：优先处理

### 1. RoboticsKJRCompat 飞行控制器仍然启动失败【运行时已确认】

证据：`KSP.log:182255`，异常链为 `EventData<T,U>.EvtDelegate..ctor → EventData<T,U>.Add → RoboticsKjrCompatFlightController.Start`；该类事件异常在本日志出现两次。

源码：`Source/RoboticsKJRCompat/RoboticsKJRCompatAddon.cs:389`。Start 第一个订阅是 `onRoboticPartLockChanged.Add(OnRoboticPartLockChanged)`；回调在同文件中声明为 static。直接读取当前 `Assembly-CSharp.dll` 的 IL，事件包装构造函数调用了 `Delegate.Target.GetType()`，没有处理空 Target。静态回调的 Target 是 null，故此路径必然抛出异常。

影响：Start 在首个订阅处中断，后续订阅和 `SanitizeVessel` 不执行。不能说整个 Harmony 插件都失效，但飞行初始化清理/事件驱动恢复链路是不完整的。机械臂解锁、读档和上下物理轨道时的兼容行为不能视为可靠。

建议：改为生命周期归属清晰的实例回调，订阅状态也按实例管理；逐项订阅/清理，避免中途失败留下半注册状态。新增实际构造 KSP 事件包装的契约测试，以及进飞行、读档、解锁、重锁、切船、退出场景验收。仅增加 try/catch 会隐藏故障，不是修复。

### 2. 全局燃料转换存在不可达规则与错误删除表达式【源码已确认】

文件：`GlobalSettings/Fuel_Conversions.cfg`。

- LF/OX 通用转换在前，会先删除 LF/OX 并创建 ModuleFuelTanks。
- LF/OX/Mono 三资源规则在后，又要求这三种资源存在且没有 ModuleFuelTanks。因此它匹配的原始零件会先被前面的规则消费，不能按预期进入三资源分支。
- 三资源分支还有 `![RESOURCE[LiquidFuel]` 拼写错误。
- 后面的全局 RCS 转换，外层检查某个 MonoPropellant RCS 模块，内层却只选择 `@MODULE[ModuleRCS*]`。对多 RCS 模块零件存在改到不同模块的风险。

影响：落入这条通用路径的混合箱可能只按 LF/OX 计算容量，保留独立 MonoPropellant，不能实现注释描述的统一 ServiceModule；这不意味着所有油箱已经受影响，许多已有专用 MFT 补丁会避开它。

建议：先列出真实命中零件，改为逐件显式容量；如保留全局兜底，必须明确其授权范围、把选择条件改成互斥，并保持外层/内层 RCS 条件一致。不要扩大 BDB 的现场计算范围。

### 3. 最终配置存在重复 ModuleFuelTanks【缓存确认，需逐件合并设计】

| 零件 | 观察 |
| --- | --- |
| KK_SpXCD_capsule | LifeSupportAll 17.745 L 与 ServiceModule 3400 L 并存 |
| KK_SpXCD_capsule_cargo | 两个 ModuleFuelTanks |
| NP_newOdinShield | Default 305 L 与 ServiceModule 800 L 并存 |
| cupola | LifeSupportAll 3.549 L 与 ServiceModule 200 L 并存 |
| KCHS_Tianhe_DZD、KCHS_Lanyue_Crew_Compartment | 两个 ModuleFuelTanks；KIU 相关，只汇报，不擅自修复 |

AO 证据：`Mods/KK_SpXCD/KK_SpXCD_capsule.cfg:43`、`KK_SpXCD_capsule_cargo.cfg:40`、`Mods/NovaPunch/Odin2.cfg:490`、`SquadPartsOverhaul/Command/Cupola.cfg:9` 都有直接新增 MFT 的路径。

影响：两个模块可能分别管理资源、体积、质量和编辑器 UI；这不是每一种组合都必然崩溃，但不是可靠的资源设计方式。不能仅删第二个，否则可能删除你需要的生命保障或推进容量。

建议：按零件设计确定唯一负责模块，把应保留的 TANK 合并，核对容量/资源总量/干质量，并验证旧 craft 与存档加载。KIU 两项单列给用户决定。

### 4. DRE 配置仍存在“局部正确、最终不一致”【缓存与源码确认】

扫描发现多件零件有两个 ModuleAeroReentry，包括 M2X 三件、RealChute 三件、多件 SEP，以及 FTPDeprecated 的两个锥形箱。FTPDeprecated 暂不优先处理。

明确例子：`Mods/Starship Expansion Project/SEP_Starship_BL2.cfg:20` 使用 `%MODULE`，后面第 37 行又新增同名 MODULE；`SEP_SuperHeavy.cfg:29` 与第 126 行同样存在两条路径。缓存 `SEP_24_SHIP_CORE` 的两个模块分别写入 skin operational 2675.835 与 2773.15。M2X_DropshipCockpit 的一个模块完整设置 2430/3240，另一个仅设置 leaveTemp。

影响：修改一个模块不能保证另一个模块不继续运行；默认值、热损伤与提示可能来自另一实例。当前不能仅凭重复就断言烧蚀消耗翻倍，但必须消除重复管理的不确定性。

另有五个最终操作温度高于对应零件最大温度：

| 零件 | 操作温度 | 对应最大温度 |
| --- | ---: | ---: |
| CA_SRBchute | skin 2880 | skin 2600 |
| CA_drogue | skin 2880 | skin 2600 |
| mk2DroneCore | internal 2250 | internal 850 |
| dockingPort25SR | skin 3240 | skin 2700 |
| dockingPort1875AR | skin 3240 | skin 2700 |

`SquadPartsOverhaul/Command/mk2Dronecore.cfg:5` 只显式设置 skinMaxTemp，却写了基于另一 internal 温度的操作阈值；`SquadPartsOverhaul/Utility/1875DockingPort.cfg:40` 和第 75 行的克隆只显式设置 maxTemp，skin 值从模板继承。这说明此前只审查“同一补丁块显式字段”的方法不够。

建议：保持用户既定耐温，不另行提高设计值；每件显式写全 internal/skin 与各自的 0.9 操作温度，保留唯一 DRE 模块。验收检查必须针对最终缓存，而不只是源码中有没有 leaveTemp。重复模块清理前先核对其余 DRE 专用字段，不能丢弃烧蚀/损伤设计。

## P2：配置准确性和依赖完整性

### 5. 14 件零件的 RF 默认配置名称不在自己的 CONFIG 列表中【缓存确认】

这说明默认选择字段悬空，不等于已经证明这些发动机全部不能点火。RF 可能回退到其他配置；实际回退行为需单独验证。

| Part | configuration | 实际 CONFIG（选列） |
| --- | --- | --- |
| CA_STBE | Kerolox | ST-26 / ST-26 Vac / ST-26 Boost |
| KCLV_CCRE_900MV | LV-70V | LY-70V / YF-209V / JD-2V |
| eisenhower_rd192V | RD-192v | RD-191v |
| bluedog_Delta_GEM63XL | GEM-63XL | GEM-63 |
| KCHS_Tianhe_FWC | MMH/NTO | Tianhe_Main_Engine |
| KK_GEM63XL | GEM-63XL | GEM-63 |
| KK_ULA_RS-68A | RS68A | RS-68A / RS-68K |
| KK_ULA_Star48B | Star-48B/Long | Star-48B |
| KK_SPX_Merlin1DV+ | Merlin1D++ | Merlin1DV+ |
| nflv-engine-rutherford-vac-1 | Kerolox | KR-1E-V / KR-1E-V Opt |
| rmm_dugong | SSBE | SSBE-BlockII |
| nuclearEngine | LH2 | LV-N "Nerv" Atomic Rocket Motor LH2 / LV-NHOX "Nerv-O" Nuclear Thermal Rocket |
| LiquidEngineLV-T91 | Aerozine/NTO | LR97-11 / LR91-3M |
| LiquidEngineLV-TX87 | Aerozine/NTO | LR87-5 / LR87-11 / LR87-LH2 / LR87-3M |

源码明确例子：`Mods/Eisenhower-Astronautics_Angara/Engines/rd192v.cfg:63`、`Mods/RocketMotorMenagerie/SSBE.cfg:28` 与第 34 行、`Mods/LaunchersPack/SpaceX Engine/Merlin_1D.cfg:233`。

建议：逐件明确默认型号，优先纠正 configuration 而不是随意重命名所有 CONFIG，以免影响存档、B9 联动和科技解锁。KCLV/KCHS 先按 KIU 管控汇报。`bluedog_Delta_GEM63XL` 缓存来源是 Hephaistos，不应仅凭 bluedog 前缀归入 BDB 清理。

### 6. 仍有四件载人零件引用不存在的 INTERNAL【缓存确认】

| Part | 缺失 INTERNAL | AO 路径 |
| --- | --- | --- |
| NP_OdinCapsule2、NP_OdinCapsule2W | MK1-2_ASETInternals | Mods/NovaPunch/Odin2.cfg:93 |
| NP_ThorLEMCapsule | ALCORInternals3 | Mods/NovaPunch/Thor.cfg:29 |
| mk3Cockpit_Shuttle | MK3_ApexInternals | Mods/6 Crew Shuttle/MK3_IVA.cfg:10 |

模型或 FreeIva 适配文件存在，不等于同名 INTERNAL 定义已安装。影响是 IVA/内部空间不可用，不能据此断言 CrewCapacity 无效或乘员消失。

建议：依赖存在时替换，否则保留实际可用的 IVA；核对座位数、FreeIva 和 Reviva。CERV B2 的旧 AT_MK3 引用已被注释，本报告不把它继续列为未修复项。

### 7. BDB 的配置层与基础引擎层还有数值差异【待运行时确认】

未发现 BDB 的容量现场计算，也未发现扫描到的 BDB PROPELLANT 中仍含 Helium；四类固定混合比的 CONFIG 层检查没有发现偏差。

但九件 Hydrolox 零件的基础 ModuleEnginesRF 仍得到约 0.960119 的氢体积分数，而政策为 0.7276：CentaurD_RL10、DeltaIV_RS68、MB60、RL20、RS30、XLR129、Saturn_Engine_J2、Saturn_Engine_J2T、LR87_LH2_Single（均带 bluedog_ 前缀）。

以 RL10 为例，四个可选 CONFIG 都是正确的 0.7276/0.2724。因此不能把基础层差异直接描述为飞行中消耗错误，MEC 应用配置后可能覆盖它。

建议：追踪基础层转换来源，统一默认基础值与选定 CONFIG；验证初次创建、切换配置、B9 切换和重载，不扩展处理用户已允许忽略的特效错配。

## P2/P3：工程治理

### 8. 构建产物隔离规则没有覆盖所有插件

`Directory.Build.props:2` 只覆盖 ArmorOverhaul、ArmorControl、CrewListSorter、RoboticsKJRCompat；fixKRPC 和 ControlledCorridorLink 的 csproj 没有同等中间产物隔离。

本轮没有在 Source 下找到遗留 DLL，因此是下一次常规构建可能重新引入的风险，不是当前重复加载的证据。`.gitignore` 忽略 obj 不会阻止 KSP 扫描它。

建议：所有插件统一在 GameData 外生成中间产物；发布时验证 GameData 下程序集名称唯一。测试完成后再部署 DLL，不以构建成功直接替代部署验收。

### 9. FINAL 使用过密，补丁职责与外部模组命名空间混用

约 91.2% 的顶层 PART 块使用 FINAL，意味着不少依赖关系只能靠同一阶段的路径顺序决定。1108 个顶层 PART 块没有 NEEDS，但这包括原版零件和安全的精确选择器，不应视作 1108 个错误。

还存在 `:FOR[RealFuels]`、`:FOR[RealPlume]`、`:FOR[000_ReStock]` 等外部模组名。FOR 同时参与模组存在声明，不能普遍当作“等该 mod 执行完”的写法；其中某些特效补丁确实需要特定时机，也不能机械全部替换。

建议：先为热参数、资源、推进、视觉各确定最终负责人，再逐领域建立 AO 自有阶段与明确 NEEDS/AFTER；暂时保留必要 FINAL。不要全库一键重排，否则可能重新引入 B9/RF/Waterfall 联动故障。

### 10. 设计数据、测试覆盖和工作树发布边界不够清晰

- 仍扫描到 35 个文件中的 278 条资源/容量相关运行时算术行；不等于 278 个错误。BDB 已经显式化，应把它作为其他模组逐步整理的范例，而非本次扩大重构范围。
- 现有测试通过，却漏过实际事件回调故障；还缺少最终缓存的跨字段不变量测试，以及 fixKRPC、ControlledCorridorLink 的专门回归入口。
- CLAUDE.md:132 起仍要求创建引擎 xlsx，与用户已暂停 Excel 工作的要求冲突，容易让后续执行者重复做已取消的工作。
- 工作树同时包含 BDB 大规模旧文件删除/新类型文件、VABO、热参数、Firefly 和构建配置变更。`git diff --stat` 不包含未跟踪文件，不能用其删除行数判断 BDB 内容被丢失。

建议：建立轻量 Markdown/JSON 设计清单（不恢复 Excel），记录 Part、来源 mod、所有者、容量、默认燃料、四个热参数、IVA 依赖、验收状态。增加缓存检查：唯一 Part 名、唯一预期单例模块、CONFIG 默认名称存在、资源引用有效、DRE 比例正确、IVA 定义存在。按主题验收和提交；不进行笼统 git add 全仓，ArmorControl 仍在禁止提交范围。

## 外部环境发现：与 AO 问题分开处理

- NRAP.ModuleTestWeight.UpdateSize 在最近日志抛出 20,301 次异常，是显著日志刷屏源。调用栈定位到 NRAP，当前没有足够证据认定 AO 是原因；应单独检查测试配重零件模型/模块状态，而不是直接写 AO 兜底。
- IRVE3 有两个同名 PART，来自 DeadlyReentry/DeadlyReentry-InflatableShields.cfg 与 DeadlyReentry/IRVE3.cfg。是外部重复克隆；与用户允许暂时忽略的 DRE 旧式 stock 克隆件归在一起，不优先变更。
- 最近 ModuleManager 汇总仍有 RealPlume-RFStockalike/AJE.cfg 1 条、RSS-Icons/RSSOrbitIcons.cfg 32 条 warning。先按来源管理，不能算作 AO 33 个新错误。
- KerbinSide 已知五百余纹理记录按要求不展开。其他零星 ContractConfigurator、Restock 等异常只进入待办，不因为日志出现异常就扩大本批工作。

## 推荐分批与验收标准

1. **插件稳定性批次**：修复 Robotics 事件生命周期和构建隔离；契约测试通过，批准后验证进飞行/重载/解锁/重锁无异常。
2. **资源结构批次**：处理全局转换、四件非 KIU 重复 MFT；按零件提供修改前后资源量、体积、干质量，先确保旧 craft 不丢资源。
3. **热设计批次**：去重 DRE，显式补齐五件矛盾参数；最终缓存满足用户既定温度与 0.9 比例，随后再入专项验证。不要再次凭截图整体提高耐温。
4. **推进与 IVA 批次**：逐件确认默认 RF 型号和可用 IVA；KIU 单列等待用户决定。BDB 基础层只在验证其覆盖链后处理。
5. **整理与发布批次**：补齐设计清单、自动审查和文档，验证 BDB 重构前后覆盖集合；按主题提交，保留用户其他工作。

本轮没有依据要求重做文昌、回收基地或 waypoint。KK 的运行时地形问题不能通过一次 cfg 静态审查保证已解决，更不能覆盖用户最近编辑的坐标来“统一配置”。
