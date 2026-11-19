# Changelog

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
