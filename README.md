# ar-model-viewer

スマートフォンのカメラで床や机などの平面を認識し、その上にアニメーション付きの3Dキャラクターを表示するARアプリです。
Unity + AR Foundation を使用し、iOS(ARKit) / Android(ARCore) の両方に対応しています。

ハッカソン(テックシーカー2026)向けに、曲げセンサーの値を使って現実のラジコンとAR上のキャラクターを同時に操作できるようにしています。

## 主な機能

- **平面検出 + 自動配置**: 床や机などの水平面を検出すると、その場にキャラクターが自動的に出現
- **曲げセンサー連携**: 指に装着した曲げセンサーの値で、AR上のキャラクターがWalk/Idleアニメーションを切り替えながら前進・旋回(現実のラジコンと同じセンサー値を使用)
- **モデル切り替え**: 画面左上のハンバーガーメニュー(≡)から、Mousey / Wolf / Hand の3種類のキャラクターをその場で切り替え可能
- **ピンチズーム**: 2本指のピンチ操作で、配置中のモデルを拡大縮小
- **リセットボタン**: 画面下部のボタンで配置をやり直し

## 動作環境

- Unity 2022.3.62f1 (LTS)
- AR Foundation 5.2.2
- ARKit XR Plugin 5.2.2 / ARCore XR Plugin 5.2.2
- glTFast 6.18.0 (GLBの読み込みに使用)
- Input System Package (New)(Active Input Handlingはこちらのみ。Androidで`Both`は非対応)

## セットアップ

1. Unity Hub からこのプロジェクトを開く(初回はパッケージのダウンロードが走るため少し時間がかかります)。
2. `Assets/Scenes/ARScene.unity` を開く。
3. `Edit > Project Settings > Player > Other Settings` で以下を確認:
   - `Active Input Handling` が `Input System Package (New)` になっていること
   - `Configuration > Allow downloads over HTTP*` が `Always allowed` になっていること(曲げセンサー連携に必要)
4. Android実機の場合、`Other Settings > Configuration` で `Scripting Backend` を `IL2CPP`、`Target Architectures` を `ARM64` に設定。

## 仕組み

### ARシーンの構成
- `AR Session` … ARのセッション管理(トラッキング状態など)。
- `XR Origin` … ARカメラとワールド座標の基準点。以下のコンポーネントが付いています。
  - `ARPlaneManager` … 水平面(床・机など)を検出する。`Requested Detection Mode` は `Horizontal`。
  - `AutoPlaceOnPlane` (`Assets/Scripts/AutoPlaceOnPlane.cs`) … 最初に検出された水平面にモデルを配置する。`SetModelPrefab()` で実行中でもモデルを切り替え可能。
  - `PinchToScaleController` (`Assets/Scripts/PinchToScaleController.cs`) … 2本指ピンチで配置中モデルを拡大縮小。
- `MenuCanvas` … 左上のハンバーガーメニュー。`ModelMenuController` (`Assets/Scripts/ModelMenuController.cs`) がモデル切り替えを制御。
- `ResetButtonCanvas` … 画面下部のリセットボタン。
- `SensorBridge` … `BendSensorClient` (`Assets/Scripts/BendSensorClient.cs`) が曲げセンサーの値をHTTP経由でポーリング。

### キャラクター
- `Assets/Prefabs/WalkingCharacter.prefab`(Hand) / `MouseyCharacter.prefab`(Mousey) / `WolfCharacter.prefab`(Wolf)
- 各キャラクターには `SensorAvatarController` (`Assets/Scripts/SensorAvatarController.cs`) が付いており、曲げセンサーの値(`left`/`right`)からWalk/Idleの切り替えと前進・旋回を計算する。
- センサーのキャリブレーション(`Rest Value` / `Bent Value`)は各キャラクターのInspectorで調整可能。
- Wolfモデルには専用のIdleアニメーションが無いため、待機中はWalkアニメーションを一時停止(`Idle Animator Speed = 0`)させることで表現している。

### 曲げセンサー連携の全体構成
- 曲げセンサー + ジャイロ(ESP32: Bend_Gyro_ESP_Sender)
- → ESP-NOWでブロードキャスト送信
- → ラジコン側ESP32(Car_Receiver)が受信してサーボを駆動 ※既存・未変更
- → 中継用ESP32(ESP32_Bridge/AR_Bridge_ESP32) が同じ電波を横取りしてWiFi APを構築
- → スマホがそのWiFiに接続し、HTTP(/data)でleft/right値をポーリング
- → BendSensorClient → SensorAvatarController がAR上のモデルを操作
中継方法は2種類用意しています。
- `ESP32_Bridge/` … 予備のESP32を使い、スマホが直接そのWiFiに接続する方式(推奨、PC不要)
- `PC_Bridge/` … PCとセンサー用ESP32をUSB接続し、スマホのテザリングでPCと同じネットワークに繋ぐ方式(予備のESP32が無い場合の代替)

詳細な手順は各フォルダの `README.md` を参照。

## 実機ビルド

- **iOS**: Mac + Xcode が必要です。Build Settings で iOS に切り替え、Player Settings でカメラ使用許可の説明文(`Camera Usage Description`)を設定してからビルドしてください。
- **Android**: Player Settings の `Minimum API Level` を Android 7.0 (API 24) 以上に設定し、ARCore対応端末で動作確認してください。

## クレジット

- Wolfモデル: [WOLF - Realistic 3D Model (Demo, Free)](https://sketchfab.com/3d-models/wolf-realistic-3d-model-demo-free-0e8e26879740478981b9cd86ae972281) by WildMesh 3D, licensed under [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/)

## 今後の予定

- 中指の曲げセンサーによる後退(リバース)操作
- Wolf専用のIdleアニメーションの追加