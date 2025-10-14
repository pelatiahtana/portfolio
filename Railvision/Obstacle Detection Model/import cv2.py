from ultralytics import YOLO # type: ignore
model=YOLO("yolo11n.pt")
results=model(source="0", show=True)