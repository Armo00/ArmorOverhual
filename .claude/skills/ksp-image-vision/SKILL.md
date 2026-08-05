---
name: ksp-image-vision
description: |
  Analyze images using the Xiaomi MiMo vision model. Use this skill whenever the user asks to look at / describe / analyze an image, render preview, screenshot, or part model view — instead of using ASCII art or guessing. Triggers on: "看图", "看看这张图", "describe this image", "分析这张图", "这张图里有什么", and any time a rendered PNG/JPG needs visual inspection. Also used automatically after ksp-mu-three-view renders.
---

# KSP Image Vision (MiMo)

Describe or analyze images using the Xiaomi MiMo multimodal model via the Anthropic-format API. **Never use ASCII art to inspect images — always call the vision model.**

## Location

| Item | Path |
|------|------|
| Vision script | `tools/vision.py` |
| API key | `.env` in project root (`XIAOMI_MIMO_API_KEY`) — do NOT read the real `.env`, only `.env_example` |
| Endpoint | `https://api.xiaomimimo.com/anthropic/v1/messages` |
| Model | `mimo-v2.5` |

## Core Process

### Run the vision call

```bash
PYTHONIOENCODING=utf-8 python tools/vision.py "<image-path>" "<prompt>"
```

- **Always set `PYTHONIOENCODING=utf-8`** — the script outputs UTF-8; without it, the Windows console (GBK) garbles the Chinese response
- Image path: absolute path to PNG/JPG (three-view composites, screenshots, etc.)
- Prompt: ask in Chinese for Chinese answers

### Common prompts

**Part model three-view** (after `ksp-mu-three-view`):
```
这是一张KSP火箭发动机的三视图（FRONT/SIDE/TOP正交视图）。请详细描述这个发动机的几何外形：1)整体轮廓形状 2)喷管部分的形状和扩张比 3)顶部泵体/燃烧室结构 4)有没有喷管支架或辅助结构 5)判断它是哪种类型的发动机。用中文回答。
```

**General image inspection**:
```
请详细描述这张图片的内容，包括所有可见元素、布局、颜色、文字。用中文回答。
```

**Short summary**:
```
请简短总结这张图（100字以内）。用中文回答。
```

## Notes

- The API key lives in `.env` (gitignored). The script auto-loads it from the project root or CWD
- On HTTP errors the script returns the error body — retry once if transient
- Max tokens default 2048; long descriptions may truncate — ask focused questions for detail
- The vision model is good at recognizing KSP part types from renders (nozzle shape, pump section, struts) — use it to judge engine type from geometry before finalizing part parameter proposals
