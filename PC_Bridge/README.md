# AR_Bridge

`Bend_Gyro_ESP_Sender.ino` のシリアル出力を読み取り、`left`/`right` の値をHTTPでJSON配信する中継サーバー。`ar-model-viewer`（Unity/AR）アプリがこれをポーリングして、AR上のキャラクターをセンサー値と連動させる。

`Bend_Gyro_ESP_Sender.ino` / `Car_Receiver.ino` は変更していない。既存のシリアルprint出力をそのままパースするだけなので、車体側の動作には影響しない。

## 使い方

1. `pip install pyserial`（インストール済みなら不要）
2. センサー側ESP32をPCにUSB接続する
3. Arduino IDEのシリアルモニタは閉じておく（COMポートの競合を避けるため）
4. どのCOMポートに繋がっているか確認したい場合:
   ```
   python list_ports.py
   ```
5. `bridge.py` 内の `SERIAL_PORT` を実際のCOMポート番号に書き換える
6. スマホのテザリング（パーソナルホットスポット）をONにし、このPCをそのWiFiに接続する
7. 実行:
   ```
   python bridge.py
   ```
8. コンソールに表示される `このPCのIP` を確認し、Unityアプリ側の接続先URL（`http://<このPCのIP>:8001/data`）に設定する

## レスポンス形式

```json
{ "left": 38, "right": 62, "connected": true, "updatedAt": 1732345678.12 }
```
