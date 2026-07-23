# ar-model-viewer

スマートフォンのカメラで床や机などの平面を認識し、その上に3Dモデル(GLB)を表示するARアプリです。
Unity + AR Foundation を使用し、iOS(ARKit) / Android(ARCore) の両方に対応しています。

現状は静止した3Dモデルを表示するところまでを目標としていますが、将来的にはアニメーション付きモデルの再生にも対応予定です。

## 動作環境

- Unity 2022.3.62f1 (LTS)
- AR Foundation 5.2.2
- ARKit XR Plugin 5.2.2 / ARCore XR Plugin 5.2.2
- glTFast 6.18.0 (GLBの読み込みに使用)

## セットアップ

1. Unity Hub からこのプロジェクトを開く(初回はパッケージのダウンロードが走るため少し時間がかかります)。
2. `Assets/Scenes/ARScene.unity` を開く。
3. 表示したい `.glb` ファイルを `Assets/Models/` にドラッグ&ドロップする。
   - glTFastが自動的にインポートし、Prefabのように扱えるオブジェクトが生成されます。
4. Hierarchy の `XR Origin` を選択し、Inspector にある `Auto Place On Plane` コンポーネントの `Model Prefab` に、3.でインポートしたオブジェクトをドラッグして割り当てる。
5. デフォルトでは `PlaceholderModel`(赤いCube)が仮のモデルとして割り当てられています。動作確認用なので、自分のモデルに差し替えてください。

## 仕組み

- `AR Session` … ARのセッション管理(トラッキング状態など)。
- `XR Origin` … ARカメラとワールド座標の基準点。以下のコンポーネントが付いています。
  - `ARPlaneManager` … 水平面(床・机など)を検出する。`Requested Detection Mode` は `Horizontal` に設定済み。
  - `AutoPlaceOnPlane` (`Assets/Scripts/AutoPlaceOnPlane.cs`) … 最初に検出された水平面の中心にモデルを1体だけ配置し、以降の平面検出を止める。

平面検出のたびに新しいモデルが増えたり、タップで配置場所を選べるようにしたい場合は `AutoPlaceOnPlane.cs` を拡張してください(`ARRaycastManager` を使ったタップ配置への切り替えが定番です)。

## 実機ビルド

- **iOS**: Mac + Xcode が必要です。Build Settings で iOS に切り替え、Player Settings でカメラ使用許可の説明文(`Camera Usage Description`)を設定してからビルドしてください。
- **Android**: Player Settings の `Minimum API Level` を Android 7.0 (API 24) 以上に設定し、ARCore対応端末で動作確認してください。

## 今後の予定

- アニメーション付きGLBモデルの再生対応
- タップによる配置位置の指定
- 複数モデルの管理・切り替えUI
