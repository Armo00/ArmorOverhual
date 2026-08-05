# KSP `.mu` 导入与三视图工具

这个工具将任意 KSP `.mu` 模型导入 Blender，并输出：

- 一个可继续编辑的 `.blend` 文件；
- `front`、`side`、`top` 三张正交渲染图；
- 一张带标签的三视图合成 PNG。

渲染使用中性技术展示材质，避免旧版 KSP Shader 节点与 Blender 5.1 API 的不兼容影响几何检查。

## 文件说明

- `render_mu_three_views.py`：导入、验证、保存 `.blend`，并渲染三张正交视图。
- `compose_mu_three_views.py`：把三张渲染图合成一张带标签的 PNG。
- `apply_blender_5_1_animation_fix.py`：导入带动画的 `.mu`（例如 Skipper）需要的 Blender 5.1 兼容补丁安装器。

## 前置条件

1. 安装 Blender 5.1。
2. 安装并启用 `io_object_mu_continued` 插件。
3. 系统 Python 安装 Pillow：`python -m pip install Pillow`。

本机当前的插件已应用兼容补丁。如在另一台机器使用并且带动画模型导入报 `ActionSlots` 或 `action_slot` 错误，请执行：

```powershell
python 'D:\KSP\KSP_Model\AI_Blender\KSP_MU_ThreeView_Tool\apply_blender_5_1_animation_fix.py' 'C:\Users\你的用户名\AppData\Roaming\Blender Foundation\Blender\5.1\scripts\addons\io_object_mu_continued'
```

随后重启 Blender。

## 使用方法

在 PowerShell 中运行。将路径替换为自己的 Blender、模型和输出目录：

```powershell
$blender = 'D:\Program Files\tools\Blender\blender.exe'
$tool = 'D:\KSP\KSP_Model\AI_Blender\KSP_MU_ThreeView_Tool'
$model = 'D:\KSP\KSP_1.12.3\TestRun\GameData\VenStockRevamp\Squad\Parts\Propulsion\Skipper.mu'
$output = 'D:\KSP\KSP_Model\AI_Blender\mu_renders\Skipper'

& $blender --background -noaudio --python "$tool\render_mu_three_views.py" -- $model $output --hide 'Rockomax Fairing'
python "$tool\compose_mu_three_views.py" $output 'Skipper'
```

`--hide` 是可选参数，会按名称片段（不区分大小写）隐藏匹配的网格，并且可重复使用。例如：

```powershell
--hide 'Rockomax Fairing' --hide 'Shroud'
```

它适用于导入的整流罩、可分离外壳等会遮挡主体的网格。脚本会在控制台输出导入的网格数量、隐藏的网格名称和可见模型的包围盒；如导入为空会直接报错，不会生成误导性的图片。

## 输出内容

若输出目录是 `$output`，模型名为 `Skipper`，会得到：

- `Skipper.blend`
- `Skipper_front.png`
- `Skipper_side.png`
- `Skipper_top.png`
- `Skipper_three_views.png`
