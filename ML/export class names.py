import json
import argparse
from pathlib import Path
from ultralytics import YOLO


def export_class_map(model_path: str, output_path: str | None = None, model_id: str | None = None) -> str:
    model = YOLO(model_path)
    names = model.names  # dict[int, str]

    if isinstance(names, dict):
        class_names = [names[i] for i in sorted(names.keys())]
    else:
        class_names = list(names)

    model_file = Path(model_path).name

    payload = {
        "model_id": model_id if model_id else Path(model_path).stem,
        "source_model_file": model_file,
        "version": 1,
        "class_names": class_names
    }

    if output_path is None:
        output_path = str(Path(model_path).with_suffix(".classes.json"))

    with open(output_path, "w", encoding="utf-8") as f:
        json.dump(payload, f, ensure_ascii=False, indent=2)

    return output_path


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("model", help="Path to YOLO .pt model")
    parser.add_argument("-o", "--output", default=None, help="Output json path")
    parser.add_argument("--model-id", default=None, help="Optional custom model id")
    args = parser.parse_args()

    saved = export_class_map(args.model, args.output, args.model_id)
    print(f"Saved class map to: {saved}")