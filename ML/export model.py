from ultralytics import YOLO

model = YOLO("yolo26n.pt")
model.export(
    format="onnx",
    imgsz=640,
    opset=13,
    simplify=True,
    dynamic=False,
    nms=False
)