# プロトタイプをUnityで組み立てる手順

ルーンブリゲード（仮）の最初のプロトタイプを、Unityで遊べる状態にする手順です。
検証すること: **1人で役割を切り替えて合成を決めるのが気持ちいいか**（[企画書](../design/game-concept.md)の「MVP Definition」）。

コードはこのリポジトリの `Assets/` にあり、シーン、プレハブ、データのアセットはまだありません。
以下の手順でUnityのエディタ上で作ります。

---

## 1. プロジェクトを開く

対象: **Unity 6.3 LTS**（`docs/engine-reference/unity/VERSION.md` の固定バージョン）。

このリポジトリにはまだ `ProjectSettings/` と `Packages/` がないので、次の手順で用意します。

1. Unity Hub で **Universal 3D** テンプレートの新しいプロジェクトを、リポジトリの外の一時フォルダに作る。
2. 作ったプロジェクトの `ProjectSettings/` と `Packages/` を、このリポジトリのルートにコピーする。
3. Unity Hub の「追加」→「ディスクから追加」で、このリポジトリのルートを開く。
4. 一時フォルダのプロジェクトは削除してよい。

`Library/`、`Temp/`、`Logs/`、`UserSettings/` などは `.gitignore` で除外済みです。`ProjectSettings/` と `Packages/` はコミットしてください。

## 2. パッケージ

Package Manager で次を入れます（Universal 3D テンプレートに入っていないもの）。

| パッケージ | 用途 |
| --- | --- |
| Input System | キーボードとマウスの入力（`PlayerInputReader`） |
| AI Navigation | NavMeshの焼き込み（`NavMeshSurface`） |
| Test Framework | EditModeテスト（通常は最初から入っている） |

`Edit > Project Settings > Player > Active Input Handling` を **Input System Package (New)** にします。

## 3. フォルダとアセンブリ

| 場所 | 中身 |
| --- | --- |
| `Assets/RuneBrigade/Scripts/Core/` | Unityに依存しないゲームの規則（状態、合成、切り替え、トークン、調整式）。`RuneBrigade.Core.asmdef` |
| `Assets/RuneBrigade/Scripts/Runtime/` | MonoBehaviourとScriptableObject。`RuneBrigade.Runtime.asmdef`（Core と Input System を参照） |
| `Assets/Tests/EditMode/` | Coreのテスト。`RuneBrigade.Tests.EditMode.asmdef` |

コンパイルエラーが出た場合は、まず Input System パッケージが入っているかを確認してください。

## 4. データのアセットを作る

`Assets/RuneBrigade/Data/` を作り、Projectウィンドウの右クリック → **Create > Rune Brigade** から次を作ります。
数値の初期値は企画書の値になっているので、最初はそのままで大丈夫です。

| アセット | 設定 |
| --- | --- |
| `CombatTuning` | そのまま |
| `ComboTable` | 作成時に4つの合成（殲滅、砕氷、爆雷、反射雷）が入る。空なら、インスペクターの右上のメニュー → **Reset** |
| `Role_Warden`（Role Definition） | role: Warden、表示名: 守護者、体力倍率 1.5、移動速度倍率 0.9、色 #F2B33D |
| `Role_Arcanist`（Role Definition） | role: Arcanist、表示名: 術士、体力倍率 0.8、移動速度倍率 1.0、色 #FF5A36 |
| `WardenTuning` | そのまま |
| `ArcanistTuning` | そのまま |
| `Enemy_Goblin`（Enemy Definition） | 体力 60、速度 4.5、攻撃 25、射程 1.8、予兆 0.6、硬直 0.8 |
| `Enemy_Skeleton` | 体力 200、速度 3、攻撃 50、射程 2.2、予兆 0.8、硬直 1.0 |
| `Enemy_Ogre` | 体力 1200、速度 2.5、攻撃 120、射程 3、予兆 1.0、硬直 1.2 |
| `Waves_Arena`（Wave Definition） | 下の表 |

`Waves_Arena` の中身:

| ウェーブ | 敵 |
| --- | --- |
| 1「ゴブリンの群れ」 | ゴブリン ×8 |
| 2「骸骨兵」 | 骸骨兵 ×3、ゴブリン ×4 |
| 3「オーガ」 | オーガ ×1、ゴブリン ×6 |

各 Enemy Definition の `prefab` には、手順6で作るプレハブを入れます。

## 5. シーン

1. 新しいシーン `Assets/RuneBrigade/Scenes/Arena.unity` を作る。
2. レイヤー `Enemy` を追加する（`Project Settings > Tags and Layers`）。
3. 床（40m × 40m程度のPlane）と、外周の壁をいくつか置く。
4. 床に **NavMeshSurface** を付けて **Bake** する。
5. 敵の出現位置として、空のGameObjectを外周に4つ置く（`Spawn_1`〜`Spawn_4`）。

### Systems オブジェクト

空のGameObject `Systems` に次を付けます。

| コンポーネント | 設定 |
| --- | --- |
| CombatDirector | tuning: `CombatTuning`、comboTable: `ComboTable`、feedback: 同じオブジェクトの FeedbackDirector、enemyLayers: `Enemy` |
| FeedbackDirector | cameraShake: プレイヤーの `Shake` オブジェクト（手順の下を参照） |
| WaveSpawner | director、waves: `Waves_Arena`、player: プレイヤーの PlayerHealth、spawnPoints: `Spawn_1`〜`Spawn_4`、playerCount: 1 |
| DebugHud | roles、health、motor: プレイヤーの各コンポーネント、director、spawner |

### プレイヤー

```
Player            (位置 y=1、CharacterController: 高さ 1.8、半径 0.4)
└─ CameraPivot    (位置 y=0.7)
   └─ Shake       (CameraShake)
      └─ Main Camera
```

`Player` に付けるコンポーネント:

| コンポーネント | 設定 |
| --- | --- |
| PlayerInputReader | なし |
| FirstPersonMotor | tuning、input、cameraPivot: `CameraPivot` |
| PlayerHealth | tuning、motor |
| PlayerRoleController | tuning、director、input、motor、health、kits: **WardenAbilities、ArcanistAbilities の順**（1キー＝守護者、2キー＝術士） |
| WardenAbilities | definition: `Role_Warden`、director、motor、health、controller、tuning: `WardenTuning` |
| ArcanistAbilities | definition: `Role_Arcanist`、director、motor、health、controller、tuning: `ArcanistTuning`、boltLayers: Default と Enemy |

`Player` は `Enemy` レイヤーにしないでください。

## 6. 敵のプレハブ

ゴブリン、骸骨兵、オーガで大きさだけ変えた3つを作ります（ゴブリン 0.7倍、骸骨兵 1倍、オーガ 1.8倍）。

```
Enemy_Goblin      (レイヤー Enemy、NavMeshAgent、CapsuleCollider、EnemyHealth、EnemyBrain)
└─ Body           (Capsule のメッシュ。コライダーは外す)
```

- EnemyBrain の `body` に `Body` を入れる（打ち上げで持ち上がる部分）。
- 色は URP Lit のマテリアルで、ムードボードの敵の色（暗い灰色の体）にする。予兆中は `EnemyDefinition` の telegraphColor に光る。
- オーガには、胸の位置に子オブジェクトを作り、SphereCollider と **WeakPoint**（倍率 3）を付ける。レイヤーは Enemy。

## 7. 遊んで確かめること

Playを押し、画面をクリックすると操作が始まります（Escでマウスを解除）。
F1キーで奥義ゲージが満タンになります（確認用）。

| 確かめること | 操作 |
| --- | --- |
| 砕氷 | 2で術士 → 右クリックで氷にする → 敵に通常弾を4発（または Q）で凍結 → 1で守護者 → 左クリック3回の3段目 |
| 殲滅 | 1で守護者 → Q（咆哮）で集める → 2で術士 → F1 → R（隕石）を集めた場所に |
| 爆雷 | 2で術士 → 炎で Q → 右クリックで雷にする → 同じ敵に通常弾4発 |
| 反射雷 | 術士の雷で敵を帯電させる → 1で守護者 → 敵の攻撃の直前に右クリック（ジャストガード） |
| スイッチストライク | 役割を切り替えた直後に、HUDに「スイッチストライク!」が出ること |
| 攻撃権トークン | 敵が多くても、同時に殴ってくるのが3体までであること |

合成が決まると、画面中央に合成名が出て、一瞬止まってからスローになります。

### 振り返りのメモ

遊んだあと、次の点をメモしておくと次の判断に使えます。

- 切り替えから合成までの流れが気持ちいいか、忙しすぎないか。
- 0.4秒の切り替え時間、2秒のスイッチストライクの長さは合っているか。
- どの合成が一番気持ちよかったか、使われなかったのはどれか。

## 8. テストを走らせる

`Window > General > Test Runner` の **EditMode** で **Run All**。
Coreの規則（状態、合成、切り替え、ダッシュ、クールダウン、トークン、調整式）のテストが57件あり、すべて通る想定です。

## プロトタイプでやっていないこと

- 狩人と祈祷師、ボス、ルーン、拠点、マップの分岐。
- ネットワーク。ゲームの規則は `RuneBrigade.Core` に分けてあるので、後でホスト側で動かす形にしやすくしてあります。
- 見た目の作り込み（ローポリのモデル、合成の破片のエフェクト、UI Toolkit のHUD）。HUDは仮のIMGUIです。
- 入力の設定アセット（Input Actions）、コントローラー対応、キー変更。
