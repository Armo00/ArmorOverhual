"""Import any KSP .mu model into Blender and render orthographic three views.

Usage (arguments after -- are read by this script):
  blender --background --python render_mu_three_views.py -- <model.mu> [output-directory] [--hide <mesh-name>]
"""

import re
import sys
import argparse
from pathlib import Path

import bpy
from mathutils import Vector


def script_args():
    return sys.argv[sys.argv.index("--") + 1 :] if "--" in sys.argv else []


def point_at(obj, target):
    obj.rotation_euler = (Vector(target) - obj.location).to_track_quat("-Z", "Y").to_euler()


def add_area_light(scene, name, location, energy, size, color, target):
    data = bpy.data.lights.new(name=name, type="AREA")
    data.energy = energy
    data.shape = "DISK"
    data.size = size
    data.color = color
    obj = bpy.data.objects.new(name, data)
    scene.collection.objects.link(obj)
    obj.location = location
    point_at(obj, target)


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


parser = argparse.ArgumentParser(description="Import a KSP .mu and render orthographic views.")
parser.add_argument("mu_file", help="Path to the input .mu file")
parser.add_argument("output_directory", nargs="?", help="Directory for the generated renders and .blend")
parser.add_argument("--hide", action="append", default=[], metavar="NAME", help="Hide meshes whose name contains NAME; may be repeated")
args = parser.parse_args(script_args())

mu_file = Path(args.mu_file).resolve()
if not mu_file.is_file() or mu_file.suffix.lower() != ".mu":
    raise RuntimeError(f"Input must be an existing .mu file: {mu_file}")

safe_name = re.sub(r"[^A-Za-z0-9._-]+", "_", mu_file.stem)
default_output = Path(__file__).resolve().parent / "mu_renders" / safe_name
output_dir = Path(args.output_directory).resolve() if args.output_directory else default_output
output_dir.mkdir(parents=True, exist_ok=True)
output_base = output_dir / safe_name

clear_scene()
scene = bpy.context.scene
scene.render.engine = "BLENDER_EEVEE"
scene.render.resolution_x = 700
scene.render.resolution_y = 800
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = "PNG"
scene.render.image_settings.color_mode = "RGBA"
scene.render.film_transparent = False
scene.world.color = (0.012, 0.018, 0.03)
scene.view_settings.look = "AgX - Medium High Contrast"

result = bpy.ops.import_object.ksp_mu(
    filepath=str(mu_file),
    create_colliders=False,
    force_armature=False,
    force_mesh=False,
)
if result != {"FINISHED"}:
    raise RuntimeError(f"KSP .mu import failed: {result}")

all_meshes = [obj for obj in scene.objects if obj.type == "MESH"]
hidden_meshes = []
for obj in all_meshes:
    if any(token.lower() in obj.name.lower() for token in args.hide):
        obj.hide_render = True
        obj.hide_set(True)
        hidden_meshes.append(obj.name)
meshes = [obj for obj in all_meshes if not obj.hide_render]
if not meshes:
    raise RuntimeError("Import completed but yielded no visible mesh objects.")

corners = [obj.matrix_world @ Vector(corner) for obj in meshes for corner in obj.bound_box]
mins = Vector(tuple(min(point[i] for point in corners) for i in range(3)))
maxs = Vector(tuple(max(point[i] for point in corners) for i in range(3)))
center = (mins + maxs) / 2
size = maxs - mins

# A neutral material ensures legacy KSP shader presets do not break the render
# in current Blender versions while retaining all imported geometry.
material = bpy.data.materials.new("Technical Illustration Finish")
material.use_nodes = True
nodes = material.node_tree.nodes
nodes.clear()
bsdf = nodes.new("ShaderNodeBsdfPrincipled")
output = nodes.new("ShaderNodeOutputMaterial")
material.node_tree.links.new(bsdf.outputs["BSDF"], output.inputs["Surface"])
bsdf.inputs["Base Color"].default_value = (0.11, 0.19, 0.29, 1.0)
bsdf.inputs["Metallic"].default_value = 0.78
bsdf.inputs["Roughness"].default_value = 0.28
for obj in meshes:
    obj.data.materials.clear()
    obj.data.materials.append(material)

camera_data = bpy.data.cameras.new("Orthographic Camera")
camera_data.type = "ORTHO"
camera_data.ortho_scale = max(size.x, size.y, size.z) * 1.38
camera = bpy.data.objects.new("Orthographic Camera", camera_data)
scene.collection.objects.link(camera)
scene.camera = camera

span = max(size) * 8.0
add_area_light(scene, "Key", center + Vector((-span * 0.25, -span * 0.35, span * 0.35)), 950, span * 0.3, (0.76, 0.88, 1.0), center)
add_area_light(scene, "Fill", center + Vector((span * 0.25, -span * 0.25, span * 0.08)), 430, span * 0.25, (0.35, 0.58, 1.0), center)
add_area_light(scene, "Rim", center + Vector((0, span * 0.20, span * 0.30)), 1050, span * 0.22, (0.60, 0.78, 1.0), center)

views = (
    ("front", center + Vector((0, -span, 0))),
    ("side", center + Vector((span, 0, 0))),
    ("top", center + Vector((0, 0, span))),
)
for view_name, camera_location in views:
    camera.location = camera_location
    point_at(camera, center)
    scene.render.filepath = str(output_base.with_name(f"{safe_name}_{view_name}.png"))
    bpy.ops.render.render(write_still=True)

blend_path = output_base.with_suffix(".blend")
bpy.ops.wm.save_as_mainfile(filepath=str(blend_path))
print(f"MU_FILE={mu_file}")
print(f"MESH_COUNT={len(meshes)}")
print(f"HIDDEN_MESHES={hidden_meshes}")
print(f"BOUNDS_MIN={tuple(round(value, 5) for value in mins)}")
print(f"BOUNDS_MAX={tuple(round(value, 5) for value in maxs)}")
print(f"OUTPUT_DIR={output_dir}")
print(f"BLEND_FILE={blend_path}")
