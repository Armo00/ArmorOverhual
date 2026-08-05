---
name: ksp-mu-three-view
description: |
  Render orthographic three-view images (front/side/top) of any KSP .mu part model using Blender. Use this skill whenever the user asks to render a KSP part's three views, generate a model preview, inspect a part's geometry/shape without opening the game, or determine what a part looks like from its model. Triggers on: "渲染三视图", "三视图", "render three views", "看下这个part长什么样", "模型长什么样", "mu模型预览".
---

# KSP MU Three-View Rendering

Render front/side/top orthographic views of a KSP `.mu` part model, then compose a labeled composite PNG. Use with the `ksp-image-vision` skill to describe the rendered model.

## Location

| Item | Path |
|------|------|
| Tool scripts | `tools/KSP_MU_ThreeView_Tool/` |
| Blender | `D:\Program Files\tools\Blender\blender.exe` |
| Output (temp) | `C:\Users\26549\AppData\Local\Temp\mu_renders\<model>\` |
| Output (project) | `techtree_design/` (copy composite here for user reference) |

## Prerequisites

- Blender 5.1 installed with `io_object_mu_continued` addon enabled (already done on Mainframe-0)
- Python with Pillow: `python -m pip install Pillow`

## Core Process

### Phase 1: Locate the .mu file

1. Find the part's config in GameData (the mod that defines the part)
2. Extract the `MODEL { model = <path> }` line — the `.mu` file is at `<GameData>/<path>.mu`
3. Verify the file exists

### Phase 2: Render three views

Run the Blender render script:

```bash
"<blender.exe>" --background -noaudio --python "<tool>/render_mu_three_views.py" -- "<model.mu>" "<output-dir>" [--hide '<name>']...
```

Example:

```bash
"D:/Program Files/tools/Blender/blender.exe" --background -noaudio --python "tools/KSP_MU_ThreeView_Tool/render_mu_three_views.py" -- "GameData/VenStockRevamp/Squad/Parts/Propulsion/Skipper.mu" "C:/Users/26549/AppData/Local/Temp/mu_renders/Skipper" --hide 'Rockomax Fairing' --hide 'Shroud'
```

**`--hide` usage**: hides meshes whose name contains the token (case-insensitive), repeatable. Use it for fairings/shrouds/decals that occlude the main body. The script prints `MESH_COUNT`, `HIDDEN_MESHES`, `BOUNDS_MIN/MAX` — check these to confirm import worked and see what was hidden.

**If the model looks wrong or occluded**:
1. Open the output `.blend` and list mesh names: `bpy.ops.wm.open_mainfile(...)` then print `bpy.data.objects` of type MESH
2. Add `--hide '<suspicious-name>'` and re-render

### Phase 3: Compose the labeled composite

```bash
python "<tool>/compose_mu_three_views.py" "<output-dir>" "<model-stem>"
```

Produces `<output-dir>/<stem>_three_views.png` (FRONT/SIDE/TOP labeled).

### Phase 4: Copy to project for user reference

```bash
cp "<output-dir>/<stem>_three_views.png" "techtree_design/<PartName>_three_views.png"
```

### Phase 5: Describe the model with MiMo vision

Use the `ksp-image-vision` skill to analyze the composite (do NOT use ASCII art — call the vision model instead). Ask for: overall silhouette, nozzle shape/expansion ratio, pump/combustion section, struts/pipes, and engine-type judgement.

## Engine Engineering Analysis (for engine parts)

When the rendered part is an **engine**, run a dedicated engineering analysis focused on three questions:

1. **喷管扩张比** — determines sea-level vs vacuum-optimized:
   - Compare nozzle exit diameter vs throat (narrowest) diameter
   - ~10:1–20:1 area ratio → sea-level/general purpose
   - ~40:1–80:1 → vacuum-optimized (upper stage)
2. **燃气发生器** — gas generator cycle evidence:
   - Exhaust pipes / small nozzles / turbine vent outlets on the pump section sides
   - Complex pipe runs around pump section → open-cycle GG; preburner → staged combustion
3. **TVC 结构** — thrust vector control:
   - Gimbal joint / spherical hinge at nozzle root
   - Hydraulic actuators / 作动筒 on support structure
   - Cross-check mesh names (e.g. `Obj_Gimbal`, `Piston*`) in the imported model

Standard prompt (Chinese):

```
这是KSP发动机三视图（FRONT/SIDE/TOP）。请做工程级分析，重点关注：
1) 喷管扩张比：对比喷管出口直径与喉部（最窄处）直径，估算扩张比（面积比）。出口直径约为喉部直径的几倍？这决定是海平面型还是真空特化型。
2) 燃气发生器：在泵体/涡轮段侧面或喷管上方，有没有排气管、小喷管、或涡轮废气出口结构？
3) TVC结构：喷管根部或上方有没有万向节、液压执行器、作动筒结构？
4) 泵段：顶部结构内部有没有明显的涡轮泵（圆筒+管道）特征？
请分点详细回答，用中文。
```

Also cross-reference part config for ground truth: `engineID`, `atmosphereCurve` (vac ISP vs sl ISP), `gimbalRange` (TVC), cycle notes in description. Mesh names from the .blend can confirm gimbal/actuator presence.

## Key Reference Data

- The three-view tool scripts are in `tools/KSP_MU_ThreeView_Tool/` with a Chinese README (`README_使用说明.md`)
- S2Pila example: `VenStockRevamp/Squad/Parts/Propulsion/Skipper.mu`, hide `Rockomax Fairing` + `Sphere` for clean body view
- Render resolution: 700×800 per view, composite 2100×890
- The `io_object_mu_continued` addon reads .mu directly — geometry is correct (my own OBJ-conversion fallback produced distorted geometry and should NOT be used)
