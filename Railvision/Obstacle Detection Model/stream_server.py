from ultralytics import YOLO
import cv2
from flask import Flask, Response, stream_with_context
import json
import time
from flask_cors import CORS
import threading
from flask_cors import cross_origin

app = Flask(__name__)
CORS(app, origins=["https://localhost:44357"])
model = YOLO("yolo11n.pt")

# Shared frame buffer and lock
latest_frame = {'frame': None}
frame_lock = threading.Lock()
camera_running = threading.Event()

def camera_reader():
    cap = cv2.VideoCapture(0)
    camera_running.set()
    while camera_running.is_set():
        success, frame = cap.read()
        if not success:
            continue
        with frame_lock:
            latest_frame['frame'] = frame.copy()
        time.sleep(0.01)  # Small sleep to reduce CPU usage
    cap.release()

def start_camera_thread():
    if not camera_running.is_set():
        t = threading.Thread(target=camera_reader, daemon=True)
        t.start()
        # Wait until the camera is running and a frame is available
        while latest_frame['frame'] is None:
            time.sleep(0.05)

def estimate_distance(box, frame_width):
    box_width = box[2] - box[0]
    if box_width <= 0:
        return 0
    distance = frame_width / box_width
    return round(distance, 2)

def get_latest_frame():
    with frame_lock:
        frame = latest_frame['frame'].copy() if latest_frame['frame'] is not None else None
    return frame

def gen_frames():
    start_camera_thread()
    while True:
        frame = get_latest_frame()
        if frame is None:
            continue
        results = model(frame)
        annotated_frame = results[0].plot()
        ret, buffer = cv2.imencode('.jpg', annotated_frame)
        frame_bytes = buffer.tobytes()
        yield (b'--frame\r\n'
               b'Content-Type: image/jpeg\r\n\r\n' + frame_bytes + b'\r\n')

@app.route('/yolo_stream')
def yolo_stream():
    return Response(gen_frames(), mimetype='multipart/x-mixed-replace; boundary=frame')

@app.route('/yolo_detections')
@cross_origin(origins=["https://localhost:44357"])
def yolo_detections():
    def generate():
        start_camera_thread()
        last_sent = {}
        while True:
            frame = get_latest_frame()
            if frame is None:
                continue
            results = model(frame)
            detections = []
            frame_width = frame.shape[1]
            if hasattr(results[0], "boxes") and results[0].boxes is not None and len(results[0].boxes) > 0:
                boxes = results[0].boxes.xyxy.cpu().numpy()
                classes = results[0].boxes.cls.cpu().numpy()
                for box, cls in zip(boxes, classes):
                    label = results[0].names[int(cls)]
                    distance = estimate_distance(box, frame_width)
                    now = time.time()
                    if label not in last_sent or now - last_sent[label] > 2:
                        detections.append({'label': label, 'distance': distance})
                        last_sent[label] = now
            if detections:
                yield f"data: {json.dumps(detections)}\n\n"
            time.sleep(0.5)
    return Response(stream_with_context(generate()), mimetype='text/event-stream')

if __name__ == '__main__':
    try:
        app.run(
            host='0.0.0.0',
            port=5001,
            debug=False,
            ssl_context=('localhost+2.pem', 'localhost+2-key.pem')  # or ('cert.pem', 'key.pem')
        )
    finally:
        camera_running.clear()