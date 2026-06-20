---
name: description-append-convention
description: How to append text to part descriptions using @description ^= :$:  in ModuleManager (2026-06-19)
metadata:
  type: reference
---

# Description Append Convention

MM syntax for appending text to the end of a part's description:

```
@description ^= :$:  <text to append>:
```

- `^=` — find-and-replace at end of value
- `:$:` — matches end-of-string anchor (nothing)
- `  <text>:` — appended text; colon at end creates a sentence separator

**Example:**
```
@description ^= :$:  Node uses size2.:
```
Results in: `Original description. Node uses size2.`

**Why:** Colon at the end is typical punctuation. If you need no colon, just omit it.

Used in: [[KPBS-scale-rules]]
