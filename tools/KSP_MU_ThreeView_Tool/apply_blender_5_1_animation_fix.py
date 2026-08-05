"""Apply the Blender 5.1 Action Slot compatibility fix to io_object_mu_continued.

Usage:
  python apply_blender_5_1_animation_fix.py <io_object_mu_continued-folder>
  python apply_blender_5_1_animation_fix.py <io_object_mu_continued-folder> --check
"""

import argparse
from pathlib import Path


parser = argparse.ArgumentParser()
parser.add_argument("addon_root", help="Path to the io_object_mu_continued add-on folder")
parser.add_argument("--check", action="store_true", help="Only report whether the fix is already present")
args = parser.parse_args()

target = Path(args.addon_root).resolve() / "import_mu" / "animation.py"
if not target.is_file():
    raise RuntimeError(f"Could not find add-on animation module: {target}")

source = target.read_text(encoding="utf-8")
old_snippets = (
    "def _get_fcurves_collection(action):",
    "slot = action.slots.new(id_type='OBJECT')",
    "def create_fcurve(action, curve, propmap):",
    "fcurves = _get_fcurves_collection(action)",
    "fcurve = create_fcurve(act, curve, fullpropmap)",
    "if action.slots:\n                obj.animation_data.action_slot = act.slots[0]",
)
new_snippets = (
    "def _get_fcurves_collection(action, owner):",
    "id_type=owner.bl_rna.identifier.upper(), name=owner.name)",
    "def create_fcurve(action, curve, propmap, owner):",
    "fcurves = _get_fcurves_collection(action, owner)",
    "fcurve = create_fcurve(act, curve, fullpropmap, obj)",
    "if act.slots:\n                obj.animation_data.action_slot = act.slots[0]",
)

if all(snippet in source for snippet in new_snippets):
    print("Compatibility fix is already installed.")
elif not all(snippet in source for snippet in old_snippets):
    raise RuntimeError("The installed add-on revision is not recognized; no files were changed.")
elif args.check:
    print("Compatibility fix is needed.")
else:
    updated = source.replace(
        "def _get_fcurves_collection(action):",
        "def _get_fcurves_collection(action, owner):",
    ).replace(
        "slot = action.slots.new(id_type='OBJECT')",
        "# Blender 5.1 requires both a slot name and the ID type of the\n"
        "            # data-block that owns the animation (Object, Material, etc.).\n"
        "            slot = action.slots.new(\n"
        "                id_type=owner.bl_rna.identifier.upper(), name=owner.name)",
    ).replace(
        "def create_fcurve(action, curve, propmap):",
        "def create_fcurve(action, curve, propmap, owner):",
    ).replace(
        "fcurves = _get_fcurves_collection(action)",
        "fcurves = _get_fcurves_collection(action, owner)",
    ).replace(
        "fcurve = create_fcurve(act, curve, fullpropmap)",
        "fcurve = create_fcurve(act, curve, fullpropmap, obj)",
    ).replace(
        "if action.slots:\n                obj.animation_data.action_slot = act.slots[0]",
        "if act.slots:\n                obj.animation_data.action_slot = act.slots[0]",
    )
    backup = target.with_suffix(".py.pre_blender_5_1_fix.bak")
    backup.write_text(source, encoding="utf-8")
    target.write_text(updated, encoding="utf-8")
    print(f"Compatibility fix applied. Backup: {backup}")
