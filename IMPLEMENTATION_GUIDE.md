# 障害物とプレイヤーの実装

## 実装概要

左右から障害物が出現し、プレイヤーは左右にスワイプして障害物を避けるゲームシステムを実装しました。

## クラス構成

### 1. **PlayerController.cs** (新規作成)
**責務**: プレイヤーの左右移動を管理

- **機能**:
  - スワイプ入力で左右に移動
  - DoTweenを使用した滑らかな移動アニメーション
  - 3つの位置: 左、中央、右
  - リセット機能

- **主要メソッド**:
  - `MoveToLeft()` - 左に移動
  - `MoveToRight()` - 右に移動
  - `MoveToCenter()` - 中央に移動
  - `ResetPosition()` - 初期位置（中央）にリセット

### 2. **Obstacle.cs** (新規作成)
**責務**: 障害物の移動ロジックを管理

- **機能**:
  - 左右それぞれ2つの位置（始点・終点）からの出現
  - DoTweenを使用したアニメーション
    - 画面外から始点へ出現
    - 始点から終点へ移動
    - 終点到達時に自動で非アクティブ化

- **位置設定**:
  - **0: Left Start (上)** - 左上から左下へ移動
  - **1: Left End (下)** - 左下から左上へ移動
  - **2: Right Start (上)** - 右上から右下へ移動
  - **3: Right End (下)** - 右下から右上へ移動

- **主要メソッド**:
  - `Spawn(ObstaclePosition pos, int posIndex)` - 障害物を出現
  - `Despawn()` - 障害物を非アクティブ化

### 3. **ObstacleController.cs** (変更)
**責務**: 障害物のUI表示（ヒント、矢印）を管理

- **機能**:
  - Obstacleクラスへの参照を保持
  - 障害物の位置に応じたヒントテキストと矢印の表示
  - ObstacleとUIの橋渡し

- **変更内容**:
  - 移動ロジックを全てObstacleクラスに移譲
  - UI表示の責務のみに専念

### 4. **RunPhaseController.cs** (変更)
**責務**: ゲームフェーズ全体の管理

- **追加機能**:
  - PlayerControllerの管理
  - 衝突判定ロジック
    - プレイヤーと障害物の位置を比較
    - 左の障害物に左側にいる場合は衝突
    - 右の障害物に右側にいる場合は衝突

- **変更内容**:
  - PlayerControllerの参照を追加
  - `CheckCollision()` メソッドを追加
  - プレイヤーのリセット処理を追加
  - スワイプ処理を簡略化（PlayerControllerが自動処理）

## ゲームフロー

1. **初期化**: プレイヤーは中央からスタート
2. **タップ**: 進捗バーが進む
3. **障害物出現**: 一定の進捗で左右から障害物が出現
4. **プレイヤー移動**: スワイプで左右に移動して障害物を避ける
5. **衝突判定**: 
   - プレイヤーと障害物が同じ位置にいる場合はゲームオーバー
   - 障害物を避けてクリアすると次の障害物へ
6. **ゲームクリア**: 全ての障害物を避けて進捗100%到達

## Inspector設定

### PlayerController
- `leftPositionX`: 左側の位置（デフォルト: -500）
- `rightPositionX`: 右側の位置（デフォルト: 500）
- `centerPositionY`: Y座標（デフォルト: 0）
- `moveDuration`: 移動時間（デフォルト: 0.3秒）

### Obstacle
- `leftStartPos`: 左側始点（デフォルト: -1000, 300）
- `leftEndPos`: 左側終点（デフォルト: -1000, -300）
- `rightStartPos`: 右側始点（デフォルト: 1000, 300）
- `rightEndPos`: 右側終点（デフォルト: 1000, -300）
- `moveInDuration`: 出現時間（デフォルト: 0.2秒）
- `moveDuration`: 移動時間（デフォルト: 2秒）
- `moveOutDuration`: 退出時間（デフォルト: 0.2秒）

### RunPhaseController
- `playerController`: PlayerControllerの参照
- `obstacleController`: ObstacleControllerの参照

## 責務の分離

- **PlayerController**: プレイヤーの移動のみを担当
- **Obstacle**: 障害物の移動ロジックのみを担当
- **ObstacleController**: UI表示（ヒント、矢印）のみを担当
- **RunPhaseController**: ゲーム全体のフロー、衝突判定、進捗管理を担当

