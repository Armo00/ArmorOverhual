---
name: modification-date-convention
description: Every cfg file must have its first line updated to the current date when modified
metadata:
  type: project
---

When editing any .cfg file in this project, the first line `// Modified YYYY-MM-DD` must be updated to the current date. This is a hard requirement — never skip this step.

**Why:** The dates help track when patches were last touched, which is critical for debugging MM patch conflicts across 50+ mods.

**How to apply:** After any Edit or Write to a .cfg file, check line 1 and update `// Modified YYYY-MM-DD` to today's date. Do this as part of the same editing pass, not as a separate step.
