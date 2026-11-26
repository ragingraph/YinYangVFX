# YinYang VFX

給自己多款 Unity 2D 遊戲共用的特效包（URP）。目前內容：發光刀劍揮擊，9 種顏色。

## 需求

- Unity 6（6000.0 以上）
- **URP**（2D Renderer 或一般 URP 都可以）。內建渲染管線（Built-in）不能用，Shader 會變粉紅色。
- 想要發光效果：URP 設定開啟 HDR，場景有開 Bloom 的 Volume，相機勾選 Post Processing。
  （測試場景已經設定好，可以照著複製。）

## 安裝

1. 開啟遊戲專案，選單 `Window > Package Manager`。
2. 左上角 `+` > `Add package from git URL...`
3. 貼上：

   ```
   https://github.com/ragingraph/YinYangVFX.git
   ```

4. 等 Unity 匯入完成。

### 更新到新版本

Package Manager 裡選 `YinYang VFX`，按 `Update`。
如果沒有出現更新，把 `Packages/packages-lock.json` 裡 `com.ragingraph.yinyangvfx` 那一段刪掉，再回到 Unity。

## 第一次使用

1. 選單 `Tools > YinYang VFX > Build Sword Slash`（跳出儲存視窗時選 Save 或 Don't Save，不要選 Cancel）。
2. 會在你的專案產生 `Assets/YinYangVFX/SwordSlash/`，裡面有：
   - `Prefabs/VFX_SwordSlash_<顏色>.prefab`（藍、青、綠、金、橘、紅、紫、粉、白）
   - `Materials`、`Textures`、`Settings`（Bloom）、`Scenes/VFX_SwordSlash_Test.unity`
3. 開測試場景按 Play，會看到 9 種顏色同時揮擊。

> 套件資料夾本身是唯讀的，所以 Prefab 和材質是產生在你自己的 `Assets` 裡，可以隨意修改。
> 重新執行建置會覆蓋這些檔案，想保留自己的修改，請先複製一份再改。

## 橫向角色向右橫劈：由遠到近的環繞刀光（Forward Sweep）

給橫向捲軸遊戲用。刀光從一個小光點開始（遠處），往右邊飛出去、逐漸變粗，
接著繞向鏡頭這一側，變成最粗最亮，最後回到角色身前附近，然後燒蝕消失。
位置參數就是「起點和終點的中間」，也就是角色身前（揮劍的位置）。

- 選單 `Tools > YinYang VFX > Build Sword Slash (Forward Sweep)`：9 種發光顏色，Prefab 叫 `VFX_SwordSlashS_<顏色>`
- 選單 `Tools > YinYang VFX > Build Ink Slash (Forward Sweep, Dark)`：4 種暗色，Prefab 叫 `VFX_InkSlashS_<名稱>`

```csharp
// 角色朝右：預設就是往右；朝左就 mirror = true。size 調大小（1 約 3.4 個世界單位寬）
var slash = SlashEffect.Spawn(sweepSlash, swordPoint.position, 0f, facingLeft, 1f);

// 命中瞬間（預設揮出後約 0.28 秒，大約在最粗的時候）
slash.onImpact.AddListener(() => cameraShake.Play());
```

調整造型：Prefab 上 `SlashEffect` 的 `Radius Ramp`（越大，後半段越大、越靠近鏡頭）；材質上的 `Width Ramp Power`（越大，前段越細、後段才突然變粗）。
如果想要軌跡更扁或更圓，調 Prefab 裡 `ArcMain` 和 `ArcGlow` 兩個子物件的 Scale 的 Y（例如 2.6 改 3.2 會更圓）。

## 朝鏡頭劈來的刀光（遠到近）

刀光從畫面深處開始，又小又暗，一邊揮一邊變大、變亮、越揮越快，最後大幅度劈過目標，像劈到螢幕前面。
以你給的位置為中心放大。

- 選單 `Tools > YinYang VFX > Build Sword Slash (Toward Camera)`：9 種發光顏色，Prefab 叫 `VFX_SwordSlashZ_<顏色>`
- 選單 `Tools > YinYang VFX > Build Ink Slash (Toward Camera, Dark)`：4 種暗色，Prefab 叫 `VFX_InkSlashZ_<名稱>`

```csharp
// 劈在敵人身上；角度可以調斜，例如 -25 度；朝左就 mirror = true
var slash = SlashEffect.Spawn(approachSlash, enemy.position, -25f, facingLeft, 1f);

// 命中的瞬間（預設揮出後 0.22 秒）：接鏡頭震動、頓幀、命中火花
slash.onImpact.AddListener(() => cameraShake.Play());
```

`onImpact` 也可以在 Prefab 的 Inspector 裡直接設定，所有刀光（斜劈、橫砍）都有這個事件。
想調整遠近的感覺：Prefab 上 `SlashEffect` 的 `Start Scale`（起始大小）、`End Scale`（結束大小）、`Start Alpha`（遠處亮度）。

## 橫砍刀光

原本的刀光是由上往下斜劈。橫砍版是一道寬而扁的月牙，由左掃到右，位置正好在你指定的點的中央：

- 選單 `Tools > YinYang VFX > Build Sword Slash (Horizontal)`：9 種發光顏色，Prefab 叫 `VFX_SwordSlashH_<顏色>`
- 選單 `Tools > YinYang VFX > Build Ink Slash (Horizontal, Dark)`：4 種暗色，Prefab 叫 `VFX_InkSlashH_<名稱>`

```csharp
// 角色朝右：由左掃到右；朝左：mirror = true，由右掃到左
SlashEffect.Spawn(horizontalSlash, hitPoint, 0f, facingLeft, 1f);

// 想斜一點：第三個參數是角度（度），例如 20 度往上斜
SlashEffect.Spawn(horizontalSlash, hitPoint, 20f, facingLeft, 1f);
```

橫砍和斜劈共用同一組材質和顏色，兩種都建置也不會重複。

## 暗色（陰）刀光

選單 `Tools > YinYang VFX > Build Ink Slash (Dark)`，會產生 4 種暗色刀光（Yin 白邊、Violet 紫邊、Crimson 紅邊、Jade 綠邊）：
黑色不透明的刀身，前緣有一條發光細邊。Prefab 在 `Assets/YinYangVFX/SwordSlash/Prefabs/VFX_InkSlash_<名稱>.prefab`，
測試場景是 `VFX_InkSlash_Test`（背景刻意用淺灰色，黑色才看得到）。

用法和發光刀光完全一樣，`SlashEffect.Spawn(...)`。
注意：暗色刀光用一般透明混合，在很暗的背景上會看不清楚，適合明亮或中間亮度的場景。

## 陰陽氣旋

選單 `Tools > YinYang VFX > Build Yin-Yang Swirl`，會產生 4 種顏色（Classic 金、Ice 冰藍、Violet 紫、Crimson 紅）：
旋轉的太極圖，中心扭成漩渦，外圍有黑白交錯的螺旋氣流往內吸，加上環繞旋入的光點和墨點。
Prefab 在 `Assets/YinYangVFX/Swirl/Prefabs/VFX_YinYangSwirl_<名稱>.prefab`，測試場景是 `VFX_YinYangSwirl_Test`。

```csharp
public YinYangSwirl swirlPrefab;

// 單次（傳送門閃現、施法）：持續 2.5 秒後自動淡出
YinYangSwirl.Spawn(swirlPrefab, position, size: 1f, duration: 2.5f);

// 循環（光環、蓄力、開著的傳送門）：自己決定什麼時候結束
var swirl = YinYangSwirl.Spawn(swirlPrefab, position, size: 1.2f, loop: true);
// ...
swirl.Stop();   // 淡出後自動回到物件池
```

`size` 是太極圓盤的半徑（世界單位）。圓盤本身用一般透明混合，所以黑色那一半是真的黑色，背景太暗的場景會看不出來。

## 中式武器特效（長槍・槌・劍氣・連斬）

選單 `Tools > YinYang VFX > Weapons >`：`Build Spear Thrust`（長槍刺擊）、`Build Spear Sweep`（長槍旋轉掃）、
`Build Hammer Smash`（槌重砸）、`Build Sword Qi`（劍氣）、`Build Cross Slash (X)`（連斬），或 `Build ALL Weapon Effects` 一次全做。
每種 6 個單色版本（Ice / Crimson / Jade / Gold / Violet / Azure），輸出在 `Assets/YinYangVFX/Weapons/`，並開啟測試場景。

```csharp
[SerializeField] WeaponEffect thrustFx;
WeaponEffect.Spawn(thrustFx, handPosition, angleDegrees: 0f, mirror: facingLeft, size: 1f);
```

全部動作（每種 6 個單色版本，prefab 名 `VFX_<Id>_<Colour>`）：

| 武器 | 動作（Id） | 生成位置 |
|---|---|---|
| 長槍 | 刺擊 SpearThrust、旋轉掃 SpearSweep、上挑 SpearRising、連刺 SpearFlurry | 手／身體 |
| 槌 | 重砸 HammerSmash、橫掃 HammerSweep、蓄力震地 HammerQuake | 手 |
| 劍 | 劍氣 SwordQi、連斬 CrossSlash、揮砍 SwordSlash、旋風斬 SwordWhirl、突刺 SwordLunge | 手／身體／目標 |
| 大刀 | 劈砍 BladeChop、拖刀斬 BladeDrag | 手 |
| 戟 | 月牙橫斬 HalberdCrescent、挑刺 HalberdLiftThrust | 手 |
| 鞭／雙截棍 | 甩擊 WhipLash、連環打 NunchakuFlurry | 手 |
| 拳掌 | 氣功波 PalmWave | 手 |
| 暗器 | 投擲 Dart | 手 |
| 弓箭 | 射擊 ArrowShot | 弓 |

- 生成位置：刺擊、劍氣、重砸＝武器所在的手；旋轉掃＝角色身體；連斬＝目標（X 以此為中心）。
- `mirror = true` 朝左；`angleDegrees` 旋轉整個特效；`size` 整體縮放（1 ≈ 角色高 1 單位）。
- `onImpact`：命中瞬間觸發（重砸落地、刺擊到最遠、連斬交會），可接震屏與頓幀。
- Prefab 上可調：`thickness`（粗細）、`reach`（刺擊距離）、`impactOffset`（槌落點）、`qiTravel`（劍氣飛行距離）、`sweepRadius`、`groundY`、三個 HDR 顏色。
- 美術小提醒：需要 URP 2D + Bloom（測試場景已附）。

## 地面升起特效

選單 `Tools > YinYang VFX > Ground >`（每種一項，或 `Build ALL Ground Effects`）。原點放在地面接觸點（地面高度 `groundY`）；GroundSpikeWave 往右延伸（向左要鏡像）。
GroundSpikeWave 地刺波（依序往前刺出）、GroundBurst 地面爆裂（光柱＋碎石）、RisingBlades 地面升劍、TendrilRise 藤蔓／觸手升起、PillarRise 光柱升起（螺旋）、RiseWall 升起光牆（由中央往兩側）、**InkSwords 水墨升劍**（實體劍從地面「快速向上飛出」：爆發式起飛、持續加速直飛出畫面、劍身周圍有氣流；沒有光點粒子；用貼圖 `VFX_InkSword.png`，建置時自動產生）。

## UI 特效

選單 `Tools > YinYang VFX > UI >`（每種一項，或 `Build ALL UI Effects`）。原點放在 UI 元件中心。
這些是世界座標的特效（線條＋粒子），請搭配 Screen Space - Camera 或 World Space 的 Canvas，或直接放在鏡頭前；`sortingOrder` 要設得比 UI 高。
ButtonGlow 按鈕流光（循環）、RewardBurst 獎勵爆發、UILevelUp 升級橫幅、Achievement 成就徽章、CountdownRing 倒數圓環（`duration` 即倒數秒數，結束觸發 `onImpact`）、SparkleLoop 閃爍星光（循環）。

## Boss／場景特效

選單 `Tools > YinYang VFX > Boss >`（每種一項，或 `Build ALL Boss Effects`）。原點放在 Boss 腳下／場景中心（地面在 `groundY`）。
BossEntrance Boss 登場（地裂＋光柱＋衝擊波）、WarningCircle 地面警示圈（循環）、DashWarning 衝刺警示帶（循環，朝右）、BossPhaseChange Boss 換階段（蓄力後爆發）、SceneTransition 場景轉場（擴散環，覆蓋瞬間觸發 `onImpact` 換場景）、EarthShake 地震裂地。
警示類建議用紅色（Crimson）版本。

## 拾取／環境特效

選單 `Tools > YinYang VFX > World >`（每種一項，或 `Build ALL World Effects`）。原點放在物件中心（地面在 `groundY`）。
CoinPickup 金幣拾取、ChestOpen 寶箱開啟（光芒噴出）、ItemPickup 道具拾取、HealItem 補血道具（十字上升）、SavePoint 存檔點（循環，`loop`）、MechanismActivate 機關啟動、SpikeTrap 地刺陷阱（先閃警示線再刺出）。

## 飛行物特效

選單 `Tools > YinYang VFX > Projectiles >`（每種一項，或 `Build ALL Projectile Effects`）。原點放在發射點，往右飛約 `qiTravel × 1.4～1.5` 單位（向左要鏡像）；抵達時觸發 `onImpact`。
EnergyBolt 能量彈、HomingShot 追蹤彈（弧線）、Boomerang 迴旋鏢（去回）、BeamCannon 光束砲（蓄力＋粗光束）、Laser 雷射、Shuriken 手裡劍。

## 大招特效

選單 `Tools > YinYang VFX > Ultimate >`（每種一項，或 `Build ALL Ultimate Effects`）。尺寸很大（約 8～12 單位寬），原點在施放者身體中心（劍雨、流星雨在目標區域中心）。
SkySwordRain 天降劍雨、FullScreenSlash 全螢幕斬、MeteorRain 流星雨、TaijiUltimate 太極大招、DragonQi 氣龍、BarrierExpand 結界擴張。

## 召喚／傳送特效

選單 `Tools > YinYang VFX > Summon >`（每種一項，或 `Build ALL Summon Effects`）。
SummonCircle 召喚陣（地面魔法陣＋光柱）、PortalOpen 傳送門（循環，`loop`，呼叫 `Stop()` 關閉）、TeleportOut 傳送消失、TeleportIn 傳送出現、Revive 復活、LevelUp 升級光柱。
原點放在地面或角色身體中心；TeleportOut 的 `onImpact` 觸發時隱藏角色，TeleportIn 的 `onImpact` 觸發時顯示角色。

## 死亡／消散特效

選單 `Tools > YinYang VFX > Death >`（每種一項，或 `Build ALL Death Effects`）。放在敵人身體中心，`onImpact` 觸發時再把敵人圖片隱藏或淡出。
DeathMonster 怪物死亡爆散、DeathAsh 化灰、DeathLight 化光點、DeathSoul 魂魄升天、DeathBoss Boss 死亡大爆炸（約 3 秒，前 1.2 秒連續小爆裂＋裂痕，之後大爆炸）。

## 蓄力／氣場特效

選單 `Tools > YinYang VFX > Charge >`（每種一項，或 `Build ALL Charge Effects`）。放在角色身上。
- ChargeAura 蓄力光環（持續；放開時呼叫 `Pulse()` 閃一下，再 `Stop()`）
- RageBurst 怒氣爆發（一次性）
- Awaken 覺醒（一次性約 2 秒：地面法陣→光柱→光翼爆發，爆發瞬間觸發 `onImpact`）
- Berserk 狂暴（持續，鋸齒狀火焰氣場）
- CharacterAura 角色氣場（持續，柔和的氣流外框）

## 強化／弱化特效（獲得時播放一次）

選單 `Tools > YinYang VFX > Buffs >`（每種一項，或 `Build ALL Buff Effects`）。放在角色身上，約 1.3 秒。
BuffAttackUp 攻擊提升（劍形圖示＋上升箭頭）、BuffSpeedUp 速度提升（向右箭頭＋速度線）、BuffDefenseUp 防禦提升（盾形外框）、BuffCritUp 暴擊提升（準星＋旋轉星芒）、
DebuffAttackDown 攻擊下降（斷裂掉落的劍＋下沉箭頭）、DebuffDefenseDown 防禦下降（碎裂的盾＋下沉箭頭）。強化用亮色（Gold／Azure），弱化用暗色（Violet／Crimson）較好分辨。

## 狀態特效（持續型）

選單 `Tools > YinYang VFX > Status >`（每種一項，或 `Build ALL Status Effects`）。全部是持續播放，掛在角色身上：

```csharp
var burn = WeaponEffect.Spawn(burnPrefab, player.position);
burn.follow = player.transform;
burn.Pulse();   // 每次跳傷害時閃一下
burn.Stop();    // 狀態結束
```
StatusBurn 燃燒（Crimson）、StatusFrozen 冰凍（Ice）、StatusPoison 中毒（Jade）、StatusParalyze 麻痺（Gold）、StatusSlow 減速（Azure）、StatusSilence 沉默（Violet）、StatusCurse 詛咒（Violet）、StatusBleed 流血（Crimson）。顏色可自由換。

## 命中特效

選單 `Tools > YinYang VFX > Hits >`（每種一項，或 `Build ALL Hits`）。放在敵人被打中的位置。
HitSlash 斬擊命中、HitBlunt 鈍擊命中、HitElemental 元素命中、HitPierce 刺擊命中、CritHit 爆擊、CritBadge 暴擊字樣背光（文字請自己疊上去）、KillBurst 擊殺爆散。

## 元素攻擊（火・冰・雷・風・水・土・毒・聖・暗）

選單 `Tools > YinYang VFX > Elements >`（每種一項，或 `Build ALL Elements`）。每種 6 個單色版本，請依元素選顏色（火＝Crimson、冰／水＝Ice／Azure、雷＝Gold、風／毒＝Jade、暗＝Violet…）。
FireBall 火球、Flamethrower 火焰噴射、IceSpike 冰錐、FreezeBurst 冰凍爆、LightningChain 閃電鏈、LightningStrike 落雷、WindBlade 風刃、Tornado 龍捲風、
WaterBall 水球、WaterColumn 水柱、RockSpike 岩刺、FallingRocks 落石、PoisonMist 毒霧、PoisonShot 毒彈、HolyLight 聖光、ShadowOrb 暗影球。
用法同武器特效：`WeaponEffect.Spawn(prefab, 位置, 角度, 是否朝左, 大小)`，從施法者的手發出；`onImpact` 在命中瞬間觸發。

## 實體化（具體外型）

全部特效會分批改成「實體外型」：武器就是真的劍／槍／錘／戟／箭／苦無／雙節棍的實心形狀，揮動時留下實心的水墨筆刷拖尾。所有形狀存在一張自動產生的貼圖 `VFX_Shapes.png`（建置時產生，不用準備圖）。已完成：武器（21 種）、防禦（16 種）、元素（16 種）、命中（7 種）、狀態（8 種）、強化／弱化（6 種）、蓄力／氣場（5 種）、死亡／消散（5 種）、召喚／傳送（6 種）、大招（6 種）、飛行物（6 種）、拾取／環境（7 種）、Boss／場景（6 種）、UI（6 種）、水墨升劍。其餘類別陸續改。

## 防禦特效（護盾・格擋・反彈・迴避・強化・破防）

選單 `Tools > YinYang VFX > Defense >`，每種一項，或 `Build ALL Defense Effects` 全做。每種 6 個單色版本，prefab 名 `VFX_<Id>_<Colour>`。

| 類型 | 效果（Id） | 放置位置 |
|---|---|---|
| 護盾（持續） | 圓形能量盾 ShieldOrb、前方弧形盾 ShieldArc、陰陽護罩 YinYangShield | 角色 |
| 格擋 | 格擋火花 BlockSparks、完美格擋 PerfectBlock、盾牌衝擊 ShieldRipple | 受擊點 |
| 反彈 | 反彈波 ReflectWave、彈返子彈 ProjectileReturn | 角色／格擋點 |
| 迴避 | 閃避殘影 DodgeAfterimage、翻滾塵土 RollDust、瞬步 Blink | 角色 |
| 強化 | 金鐘罩 IronBody（持續）、回復光柱 HealPillar、無敵閃爍 InvincibleGlow（持續） | 角色 |
| 破防 | 護盾破碎 ShieldBreak、破防眩暈 StunStars（持續） | 角色 |

持續型（護盾、金鐘罩、無敵閃爍、眩暈）：

```csharp
var shield = WeaponEffect.Spawn(shieldPrefab, player.position);
shield.follow = player.transform;   // 跟著角色
shield.Pulse();                     // 被打到時閃亮一下
shield.Stop();                      // 淡出並回收
```

- 身體類特效以 `sweepCenter`（預設 0,-0.2）為身體中心、`groundY` 為地面；角色約 1 單位高。
- 受擊點類特效請放在命中的位置（例如盾牌前方）。
- `onImpact` 會在格擋／反彈／破防的瞬間觸發，可接震屏與頓幀。

## 在遊戲裡使用（刀光）

```csharp
using YinYangVFX;

public SlashEffect fireSlash;   // 在 Inspector 拖入 VFX_SwordSlash_Orange

// 攻擊時
SlashEffect.Spawn(fireSlash, attackPoint.position, 0f, facingLeft, 1f);
```

參數：`Spawn(prefab, 位置, 角度, 是否左右翻轉, 大小倍率)`。內建物件池，不用自己管理。

## 新增顏色

打開套件裡的 `Editor/SlashVFXBuilder.cs`，在 `Presets` 清單加一行（名稱、外緣色、核心色，數值大於 1 才會發光），重新執行建置。
（Git 安裝的套件是唯讀的，要修改請在特效包的 repo 裡改，再 push 並更新。）

## 版本

見 `CHANGELOG.md`。
