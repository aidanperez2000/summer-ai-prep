import requests

payload = {
    "model": "qwen3",
    "prompt": "Analyze possible interpretations of this message: 'I have a meeting at 3 PM, but I might be late.'",
    "stream": True,
    "think": False,
    "options": {
        "num_predict": 180,
    },
}

with requests.post(
    "http://localhost:11434/api/generate",
    json=payload,
    stream=True,
    timeout=120,
) as response:
    response.raise_for_status()
    for line in response.iter_lines(decode_unicode=True):
        if not line:
            continue
        chunk = requests.models.complexjson.loads(line)
        text = chunk.get("response", "")
        if text:
            print(text, end="", flush=True)

print()