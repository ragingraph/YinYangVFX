using UnityEngine;

namespace YinYangVFX
{
    /// <summary>
    /// Test helper: fires the slash on a timer, cycling through a few angles and mirrored versions.
    /// Used by the generated VFX_SwordSlash_Test scene.
    /// </summary>
    public class SlashEffectDemo : MonoBehaviour
    {
        public SlashEffect prefab;
        public float interval = 1.1f;
        public float sizeMultiplier = 1f;
        public float startDelay = 0.4f;

        static readonly float[] Angles = { 0f, 0f, 35f, -35f };
        static readonly bool[] Mirrors = { false, true, false, true };

        float _timer;
        int _index;

        void Start()
        {
            _timer = startDelay;
        }

        void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer > 0f)
            {
                return;
            }

            _timer = interval;
            int k = _index++ % Angles.Length;
            SlashEffect.Spawn(prefab, transform.position, Angles[k], Mirrors[k], sizeMultiplier);
        }
    }
}
