/**
 * AR_Bridge_ESP32.ino
 *
 * Bend_Gyro_ESP_Sender.ino が ESP-NOW でブロードキャストする
 * DriveCmd {left, right} を横から受信し、このESP32自身がWiFiアクセスポイントに
 * なって、スマホから直接 HTTP で JSON 取得できるようにする中継ボード。
 *
 * Bend_Gyro_ESP_Sender.ino / Car_Receiver.ino は変更不要。
 * このボードは受信専用（ブロードキャストを横から受け取るだけ）なので、
 * 既存のラジコン制御チェーンには一切影響しない。
 *
 * 【使い方】
 *   1. このESP32に書き込む
 *   2. スマホのWiFi設定で SSID "AR_Bridge"（パスワードは下記 AP_PASSWORD）に接続する
 *      （インターネットに繋がっていない警告が出ても、ローカル通信はできるのでそのまま接続）
 *   3. Unity側の Server Url を http://192.168.4.1/data に設定する
 *      （ESP32のアクセスポイントは常にこのIPになる）
 *
 * 【うまく受信できない場合】
 *   Bend_Gyro_ESP_Sender.ino 側のWiFiチャンネルと、このボードのAPチャンネルが
 *   ずれている可能性がある。WIFI_CHANNEL の値を 1〜11 で変えて試す。
 */

#include <WiFi.h>
#include <esp_now.h>
#include <WebServer.h>

const char* AP_SSID      = "AR_Bridge";
const char* AP_PASSWORD  = "ar-bridge-2026";
const int   WIFI_CHANNEL = 1;  // Bend_Gyro_ESP_Sender 側のチャンネルに合わせる（要調整の場合あり）

typedef struct {
  int16_t left;
  int16_t right;
} DriveCmd;

volatile int16_t latestLeft = 0;
volatile int16_t latestRight = 0;
volatile unsigned long lastRecvMillis = 0;

WebServer server(80);

void onRecv(const esp_now_recv_info_t *info, const uint8_t *data, int len) {
  if (len == sizeof(DriveCmd)) {
    DriveCmd cmd;
    memcpy(&cmd, data, sizeof(cmd));
    latestLeft = cmd.left;
    latestRight = cmd.right;
    lastRecvMillis = millis();
  }
}

void handleData() {
  bool connected = (millis() - lastRecvMillis) < 1000;
  String json = "{\"left\":" + String(latestLeft) +
                ",\"right\":" + String(latestRight) +
                ",\"connected\":" + (connected ? "true" : "false") +
                ",\"updatedAt\":" + String(lastRecvMillis) + "}";
  server.sendHeader("Access-Control-Allow-Origin", "*");
  server.sendHeader("Cache-Control", "no-store, no-cache, must-revalidate");
  server.send(200, "application/json", json);
}

void handleRoot() {
  server.send(200, "text/plain", "AR_Bridge is running. GET /data for left/right JSON.");
}

void setup() {
  Serial.begin(115200);
  delay(300);

  WiFi.mode(WIFI_AP);
  WiFi.softAP(AP_SSID, AP_PASSWORD, WIFI_CHANNEL);

  Serial.println("========================================");
  Serial.print("AP起動: SSID=");
  Serial.println(AP_SSID);
  Serial.print("IP=");
  Serial.println(WiFi.softAPIP());
  Serial.println("========================================");

  if (esp_now_init() != ESP_OK) {
    Serial.println("ESP-NOW 初期化失敗");
    return;
  }
  esp_now_register_recv_cb(onRecv);
  Serial.println("ESP-NOW 受信待ち...");

  server.on("/", handleRoot);
  server.on("/data", handleData);
  server.begin();
  Serial.println("HTTPサーバー起動（ポート80）");
}

void loop() {
  server.handleClient();
}
