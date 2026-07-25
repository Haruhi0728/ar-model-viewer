#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
AR_Bridge 中継サーバー

Bend_Gyro_ESP_Sender.ino のシリアル出力（"... -> L=<int> R=<int>" を含む行）を
そのまま読み取り、left/right をJSONでHTTP配信する。
ar-model-viewer（Unity）アプリは同じネットワーク上からこの /data をポーリングする。

※ Bend_Gyro_ESP_Sender.ino / Car_Receiver.ino のコードは一切変更していない。
   既存のシリアルprint出力をパースするだけ。

使い方:
  1) pip install pyserial
  2) Arduino IDEのシリアルモニタは閉じる（COMポートを取り合うため）
  3) 下の SERIAL_PORT を自分の環境に合わせる（デバイスマネージャーで確認）
  4) スマホのテザリング（パーソナルホットスポット）をON
  5) このPCをそのテザリングWiFiに接続する
  6) python bridge.py
  7) 表示される「このPCのIP」を確認し、スマホ側アプリにそのIPを設定する
     （例: http://<このPCのIP>:8001/data）
"""

import os
import re
import json
import time
import socket
import threading
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

import serial  # pip install pyserial

# ---- 設定 ----
SERIAL_PORT = "COM5"   # ← 自分の環境に合わせる（デバイスマネージャーで確認）
BAUD = 115200
HTTP_PORT = 8001

# Bend_Gyro_ESP_Sender.ino の出力例:
# 値：1450  曲げ度：50%  X：0.1  Y：-0.2  Z：3.4  | steer=12 -> L=38 R=62
LR_PATTERN = re.compile(r"L=(-?\d+)\s*R=(-?\d+)")

latest = {"left": 0, "right": 0, "connected": False, "updatedAt": 0}
HERE = os.path.dirname(os.path.abspath(__file__))


def get_local_ip():
    s = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    try:
        s.connect(("8.8.8.8", 80))
        return s.getsockname()[0]
    except Exception:
        return "127.0.0.1"
    finally:
        s.close()


def serial_reader():
    while True:
        try:
            ser = serial.Serial(SERIAL_PORT, BAUD, timeout=1)
            print(f"[OK] シリアル接続: {SERIAL_PORT}")
            latest["connected"] = True
            while True:
                line = ser.readline().decode("utf-8", "ignore").strip()
                m = LR_PATTERN.search(line)
                if m:
                    latest["left"] = int(m.group(1))
                    latest["right"] = int(m.group(2))
                    latest["updatedAt"] = time.time()
        except Exception as e:
            latest["connected"] = False
            print(f"[再接続] シリアルエラー: {e}（2秒後に再試行）")
            time.sleep(2)


class Handler(BaseHTTPRequestHandler):
    def log_message(self, *a):
        pass  # アクセスログ抑制

    def do_GET(self):
        if self.path.startswith("/data"):
            body = json.dumps(latest).encode("utf-8")
            self.send_response(200)
            self.send_header("Content-Type", "application/json")
            self.send_header("Access-Control-Allow-Origin", "*")
            self.send_header("Cache-Control", "no-store, no-cache, must-revalidate")
            self.send_header("Content-Length", str(len(body)))
            self.end_headers()
            self.wfile.write(body)
        else:
            self.send_response(200)
            self.send_header("Content-Type", "text/plain; charset=utf-8")
            self.end_headers()
            self.wfile.write(b"AR_Bridge is running. GET /data for left/right JSON.")


if __name__ == "__main__":
    ip = get_local_ip()
    threading.Thread(target=serial_reader, daemon=True).start()
    print("========================================")
    print(f"  このPCのIP: {ip}")
    print(f"  スマホ側に設定するURL: http://{ip}:{HTTP_PORT}/data")
    print("========================================")
    ThreadingHTTPServer(("0.0.0.0", HTTP_PORT), Handler).serve_forever()
