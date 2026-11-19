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
