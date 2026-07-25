# ESP32_Bridge

`AR_Bridge_ESP32/AR_Bridge_ESP32.ino` は、予備のESP32ボード専用の中継プログラム。`PC_Bridge`（PC + スマホのテザリング経由）の代わりに、こちらは **ESP32自身がWiFiの電波を出し、スマホが直接それに繋ぐだけ** で完結する。PCもテザリングも不要になる。

`Bend_Gyro_ESP_Sender.ino` / `Car_Receiver.ino` は変更していない。ESP-NOWのブロードキャストを横から受信するだけなので、ラジコン本体の動作には影響しない。

## 書き込み方

1. Arduino IDEでこのESP32ボードを選択し、`AR_Bridge_ESP32.ino` を開いて書き込む
   - 必要ライブラリ: `WiFi`, `esp_now`, `WebServer`（いずれもESP32ボードマネージャーに標準搭載）
2. 書き込み後、シリアルモニタで以下のような表示が出ることを確認
   ```
   AP起動: SSID=AR_Bridge
   IP=192.168.4.1
   ESP-NOW 受信待ち...
   HTTPサーバー起動（ポート80）
   ```

## 使い方

1. スマホのWiFi設定を開き、SSID `AR_Bridge`（パスワード: `ar-bridge-2026`）に接続する
   - 「インターネット未接続」の警告が出ても、そのまま接続して問題ない（ローカル通信のみ使う）
2. Unity側（`SensorBridge` の `Bend Sensor Client` コンポーネント）の `Server Url` を
   ```
   http://192.168.4.1/data
   ```
   に設定する
3. `Bend_Gyro_ESP_Sender` 側のESP32の電源を入れて曲げセンサーを操作すると、この中継ボードのシリアルには何も出ないが（受信コールバックのみ）、`http://192.168.4.1/data` をブラウザ等で開くと値の変化が確認できる

## うまく受信できない時

ESP-NOWは無線チャンネルが揃っていないと通信できない。`AR_Bridge_ESP32.ino` 内の `WIFI_CHANNEL`（デフォルト1）を1〜11の間で変えて試す。
