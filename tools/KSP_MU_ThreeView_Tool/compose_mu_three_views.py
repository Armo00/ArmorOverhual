"""Combine three renders from render_mu_three_views.py into a labeled PNG.

Usage:
  python compose_mu_three_views.py <output-directory> <model-stem>
"""

import sys
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


if len(sys.argv) != 3:
    raise RuntimeError("Usage: compose_mu_three_views.py <output-directory> <model-stem>")

output_dir = Path(sys.argv[1]).resolve()
stem = sys.argv[2]
views = (("FRONT", output_dir / f"{stem}_front.png"), ("SIDE", output_dir / f"{stem}_side.png"), ("TOP", output_dir / f"{stem}_top.png"))
if not all(path.is_file() for _, path in views):
    raise RuntimeError("Expected front, side, and top render files were not found.")

panels = [Image.open(path).convert("RGB") for _, path in views]
panel_width, panel_height = panels[0].size
if any(panel.size != (panel_width, panel_height) for panel in panels):
    raise RuntimeError("The three view renders do not have matching dimensions.")

canvas = Image.new("RGB", (panel_width * 3, panel_height + 90), "#25272d")
draw = ImageDraw.Draw(canvas)
try:
    heading_font = ImageFont.truetype(r"C:\Windows\Fonts\segoeuib.ttf", 26)
    label_font = ImageFont.truetype(r"C:\Windows\Fonts\segoeuib.ttf", 22)
except OSError:
    heading_font = label_font = ImageFont.load_default()

title = f"{stem.upper()}  |  ORTHOGRAPHIC VIEWS"
title_box = draw.textbbox((0, 0), title, font=heading_font)
draw.text(((canvas.width - (title_box[2] - title_box[0])) / 2, 14), title, font=heading_font, fill="#b7dbff")
for index, ((label, _), panel) in enumerate(zip(views, panels)):
    x = index * panel_width
    canvas.paste(panel, (x, 90))
    label_box = draw.textbbox((0, 0), label, font=label_font)
    draw.text((x + (panel_width - (label_box[2] - label_box[0])) / 2, 55), label, font=label_font, fill="#79baff")
    if index:
        draw.line((x, 48, x, canvas.height - 20), fill="#46505d", width=2)

output = output_dir / f"{stem}_three_views.png"
canvas.save(output)
print(f"COMPOSITE={output} SIZE={canvas.size[0]}x{canvas.size[1]}")
