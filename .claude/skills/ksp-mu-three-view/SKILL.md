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

## Key Reference Data

- The three-view tool scripts are in `tools/KSP_MU_ThreeView_Tool/` with a Chinese README (`README_使用说明.md`)
- S2Pila example: `VenStockRevamp/Squad/Parts/Propulsion/Skipper.mu`, hide `Rockomax Fairing` + `Sphere` for clean body view
- Render resolution: 700×800 per view, composite 2100×890
- The `io_object_mu_continued` addon reads .mu directly — geometry is correct (my own OBJ-conversion fallback produced distorted geometry and should NOT be used)
