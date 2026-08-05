"""Vision tool — call MiMo v2.5 multimodal model via Anthropic-format API."""
import base64
import json
import os
import sys
from pathlib import Path
from urllib.error import HTTPError, URLError
from urllib.request import Request, urlopen


ENDPOINT = "https://api.xiaomimimo.com/anthropic/v1/messages"
MODEL = "mimo-v2.5"

def load_env():
    """Load .env from the project root (same dir as this script or CWD)."""
    candidates = [
        Path(__file__).resolve().parent.parent / ".env",
        Path.cwd() / ".env",
    ]
    for p in candidates:
        if not p.exists():
            continue
        for line in p.read_text(encoding="utf-8").splitlines():
            line = line.strip()
            if not line or line.startswith("#") or "=" not in line:
                continue
            k, v = line.split("=", 1)
            k = k.strip()
            v = v.strip().strip('"').strip("'")
            if k not in os.environ:
                os.environ[k] = v


def image_to_base64(path: str) -> tuple[str, str]:
    """Read image file, return (base64_string, media_type)."""
    p = Path(path)
    if not p.exists():
        raise FileNotFoundError(f"Image not found: {path}")
    data = p.read_bytes()
    suffix = p.suffix.lower()
    media_map = {
        ".png": "image/png",
        ".jpg": "image/jpeg",
        ".jpeg": "image/jpeg",
        ".gif": "image/gif",
        ".webp": "image/webp",
    }
    media_type = media_map.get(suffix, "image/png")
    return base64.b64encode(data).decode("ascii"), media_type


def call_vision(image_path: str, prompt: str, max_tokens: int = 2048) -> str:
    """Send image + prompt to MiMo, return text response."""
    load_env()
    api_key = os.environ.get("XIAOMI_MIMO_API_KEY")
    if not api_key:
        raise RuntimeError("XIAOMI_MIMO_API_KEY not set in .env")

    b64, media_type = image_to_base64(image_path)

    body = {
        "model": MODEL,
        "max_tokens": max_tokens,
        "messages": [
            {
                "role": "user",
                "content": [
                    {
                        "type": "image",
                        "source": {
                            "type": "base64",
                            "media_type": media_type,
                            "data": b64,
                        },
                    },
                    {"type": "text", "text": prompt},
                ],
            }
        ],
    }

    req = Request(
        ENDPOINT,
        data=json.dumps(body).encode("utf-8"),
        headers={
            "Content-Type": "application/json",
            "x-api-key": api_key,
            "anthropic-version": "2023-06-01",
        },
        method="POST",
    )

    try:
        with urlopen(req, timeout=120) as resp:
            result = json.loads(resp.read().decode("utf-8"))
    except HTTPError as exc:
        body_text = exc.read().decode("utf-8", errors="replace")
        return f"HTTP {exc.code}: {body_text[:2000]}"
    except URLError as exc:
        return f"Transport error: {exc}"

    # Anthropic Messages API response format
    content = result.get("content", [])
    texts = []
    for block in content:
        if block.get("type") == "text":
            texts.append(block["text"])
    return "\n".join(texts) if texts else json.dumps(result, indent=2, ensure_ascii=False)


if __name__ == "__main__":
    if len(sys.argv) < 3:
        print("Usage: python vision.py <image_path> <prompt>")
        print("Example: python vision.py screenshot.png 'Are there any overlapping elements?'")
        sys.exit(1)

    image_path = sys.argv[1]
    prompt = sys.argv[2]
    result = call_vision(image_path, prompt)
    print(result)
