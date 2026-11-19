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

## 在遊戲裡使用

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
