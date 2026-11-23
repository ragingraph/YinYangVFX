# Changelog

## 0.31.0

- Solid status effects (all 8, looping): flames, ice crystals, poison drops + skull, lightning bolts, weights/arrows, sealed ring, curse chains + dark orb, blood drops. No particles. Rebuild status prefabs.

## 0.30.0

- Solid hit effects (all 7): ink-brush slash marks, flying shards/rocks, solid stars, stabbing kunai, crit banner, kill skull. No new atlas shapes. Rebuild hit prefabs.

## 0.29.0

- Solid element effects (all 16): layered flames, ice crystals, thick lightning bolts, rocks, water orbs/drops, feathers, skulls, shadow orb. New atlas shapes: Flame, Ice, Bolt, Rock, Orb, DarkOrb, Drop, Feather, Skull. Rebuild all prefabs.

## 0.28.0

- Solid defence effects (all 16): hex shield panels, yin-yang disc, shards, stars, heal crosses and ink-brush contour ghosts. New atlas shapes: Taiji, Hex, Shard, Star5, Cross. Rebuild all prefabs.

## 0.27.0

- Solid shapes: new shape atlas (`VFX_Shapes.png`: sword, spear, hammer, halberd, arrow, kunai, nunchaku stick, ink brush stroke) and `shapeMaterial` (replaces `swordMaterial`). All 21 weapon effects now draw the real weapon plus solid ink-brush swing trails. Rebuild every prefab.

## 0.26.0

- InkSwords: swords now accelerate straight up until they leave the screen (no hover), all light particles removed, airflow added (wind ribbons + vortex rings around the swords). Duration 1.1s. Rebuild the prefab.

## 0.25.0

- Ground effects now shoot upward FAST (explosive launch, quick brake, speed streaks) instead of slowly bulging up. InkSwords: swords launch out of the ground and fly up, with speed trails. Rebuild existing Ground prefabs.

## 0.24.0

- New `InkSwords`: first effect with a solid, concrete shape (slender ink-style swords rising from the ground) via a procedural sword texture on cropped quads. New `swordMaterial` field. Menu: `Tools/YinYang VFX/Ground/Build Ink Swords` (also included in Build ALL Ground).

## 0.23.0

- 6 rise-from-ground effects: spike wave, ground burst, rising blades, tendril rise, pillar rise, rise wall. Menus under `Tools/YinYang VFX/Ground/`.

## 0.22.0

- 6 UI effects: button glow (loop), reward burst, UI level up, achievement, countdown ring, sparkle (loop). Menus under `Tools/YinYang VFX/UI/`.

## 0.21.0

- 6 boss/scene effects: boss entrance, warning circle (loop), dash warning (loop), boss phase change, scene transition, earth shake. Menus under `Tools/YinYang VFX/Boss/`.

## 0.20.0

- 7 pickup/environment effects: coin, chest, item pickup, heal item, save point (loop), mechanism, spike trap. Menus under `Tools/YinYang VFX/World/`.

## 0.19.0

- 6 projectile effects: energy bolt, homing shot, boomerang, beam cannon, laser, shuriken. Menus under `Tools/YinYang VFX/Projectiles/`.

## 0.18.0

- 6 ultimate effects: sky sword rain, full-screen slash, meteor rain, taiji ultimate, dragon qi, barrier expand. Menus under `Tools/YinYang VFX/Ultimate/`.

## 0.17.0

- 6 summon/teleport effects: summon circle, portal (loop), teleport out/in, revive, level up. Menus under `Tools/YinYang VFX/Summon/`.

## 0.16.0

- 5 death effects: monster death, to ash, to light, soul, boss death. Menus under `Tools/YinYang VFX/Death/`.

## 0.15.0

- 5 charge / aura effects: charge aura, rage burst, awaken, berserk, character aura. Menus under `Tools/YinYang VFX/Charge/`.

## 0.14.0

- 6 buff / debuff effects: attack up, speed up, defence up, crit up, attack down, defence down. Menus under `Tools/YinYang VFX/Buffs/`.

## 0.13.0

- 8 looping status effects: burning, frozen, poisoned, paralysed, slowed, silenced, cursed, bleeding. Menus under `Tools/YinYang VFX/Status/`.

## 0.12.0

- 7 hit effects: slash / blunt / elemental / pierce hit, crit hit, crit badge (light behind a text), kill burst. Menus under `Tools/YinYang VFX/Hits/`.

## 0.11.0

- 16 elemental attack effects (fire ball / flamethrower, ice spike / freeze burst, lightning chain / strike, wind blade / tornado, water ball / column, rock spike / falling rocks, poison mist / shot, holy light, shadow orb). Menus under `Tools/YinYang VFX/Elements/`.

## 0.10.0

- Weapon / defence effects: richer look. Strokes are ~30% thicker with a wide halo, mid glow and a winding energy ribbon;
  every stroke glitters with stars; spark and star particles are emitted 2.5x (`particleMultiplier`, `sparkle` on the prefab);
  brighter materials and a stronger bloom profile for the test scenes (`VFX_Bloom_Weapons`). Re-run the build menu to update.
- Recommended Bloom for your game: threshold 0.8, intensity 1.5, scatter 0.7.

## 0.9.0

- 16 defence effects (single colour, 6 colours each), under `Tools/YinYang VFX/Defense/`:
  shield orb, shield arc, yin-yang shield, block sparks, perfect block, shield ripple, reflect wave, projectile return,
  dodge afterimage, roll dust, blink, iron body, heal pillar, invincible glow, shield break, stun stars.
- `WeaponEffect` looping mode: `loop`, `follow`, `followOffset`, `Stop()`, `Pulse()` (shield / buff / stun effects stay until stopped).

## 0.8.0

- 16 more weapon effects (all single colour, 6 colours each): spear rising / flurry, hammer sweep / quake (charge + ground slam),
  sword slash / whirl / lunge, blade chop / drag (big sabre), halberd crescent / lift-thrust, whip lash, nunchaku flurry,
  palm wave, dart throw, arrow shot. Menus `Tools/YinYang VFX/Weapons/Build <name>`; `Build ALL Weapon Effects` now builds all 21.

## 0.7.2

- Weapon effects: flashes and the hot core are tinted with the effect colour instead of near-white, so each prefab reads as one colour.

## 0.7.1

- Weapon effects are now a single colour each (all layers share one hue). Re-run the Weapons build menu to update existing prefabs.

## 0.7.0

- Chinese-weapon attack effects (new `WeaponEffect` component, 6 colours each, procedural, no sprites needed):
  spear thrust, spear spin sweep, hammer smash (cracks, shockwave, debris), sword qi flying crescent, X cross slash.
  Menus under `Tools/YinYang VFX/Weapons/` (one per effect + `Build ALL Weapon Effects`).
  Prefabs `VFX_<SpearThrust|SpearSweep|HammerSmash|SwordQi|CrossSlash>_<Ice|Crimson|Jade|Gold|Violet|Azure>`.
- `onImpact` event on every weapon effect (screen shake / hit-stop trigger point).

## 0.6.0

- Forward sweep: the side-scroller slash. Starts as a small point far away, runs to the right getting thicker,
  swings round toward the camera and ends big and thick back near the character.
  Menus `Build Sword Slash (Forward Sweep)` (9 glow colours) and `Build Ink Slash (Forward Sweep, Dark)` (4 ink colours).
  Prefabs `VFX_SwordSlashS_<Colour>` / `VFX_InkSlashS_<Name>`.
- `SlashArc` shader: `_Ramp` / `_RampPow` (width grows along the arc) and `_RadiusRamp` (later part is bigger). Defaults keep the crescent look.
- `SlashEffect`: `radiusRamp`, `growAlongBlade` (sparks scale with the blade width); arc half span can go up to 175 degrees.

## 0.5.0

- Toward-camera slash: a chop that starts small and dim far away, speeds up while growing, and arcs over the target.
  Menus `Build Sword Slash (Toward Camera)` (9 glow colours) and `Build Ink Slash (Toward Camera, Dark)` (4 ink colours).
  Prefabs `VFX_SwordSlashZ_<Colour>` / `VFX_InkSlashZ_<Name>`.
- `SlashEffect`: `approach` mode (`startScale`, `endScale`, `scaleCurve`, `startAlpha`, `alphaRiseTime`) and an `onImpact` UnityEvent
  (`impactTime`) for camera shake / hit-stop / hit sparks. Existing slashes behave as before.
- Arc centre offsets now follow the mirror flip.

## 0.4.0

- Horizontal sword slash: wide, flat crescent swinging left to right, centred on the spawn position.
  Menus `Build Sword Slash (Horizontal)` (9 glow colours) and `Build Ink Slash (Horizontal, Dark)` (4 ink colours).
  Prefabs `VFX_SwordSlashH_<Colour>` / `VFX_InkSlashH_<Name>`. Existing vertical slashes are unchanged.
- Builder: slash geometry (centre angle, span, radius, quad size) is now a `SlashShape` preset.

## 0.3.0

- Yin-yang vortex: spinning taiji disc twisted into a vortex, light/dark spiral arms drifting inward, orbiting light and ink motes.
  4 presets (Classic, Ice, Violet, Crimson). Menu `Tools > YinYang VFX > Build Yin-Yang Swirl`.
- `YinYangSwirl` component: one-shot or looping, `Stop()` fades out, pooled via `Spawn`.
- `ParticleAdditive` shader: selectable blend mode (`_SrcBlend` / `_DstBlend`). Defaults keep the old additive look.

## 0.2.0

- Ink (dark) sword slash: opaque black blade with a glowing leading edge, 4 presets (Yin, Violet, Crimson, Jade).
  Menu `Tools > YinYang VFX > Build Ink Slash (Dark)`, test scene on a light background.
- `SlashArc` shader: selectable blend mode (`_SrcBlend` / `_DstBlend`) and `_AlphaBoost`. Defaults keep the old additive look.

## 0.1.0

- Sword slash: procedural crescent shader, glow layer, sparks and motes, pooled playback.
- 9 colour presets: Blue, Cyan, Green, Gold, Orange, Red, Purple, Pink, White.
- One-click builder (`Tools > YinYang VFX > Build Sword Slash`) generating prefabs, materials, bloom profile and a test scene into the project's `Assets/YinYangVFX/SwordSlash`.
