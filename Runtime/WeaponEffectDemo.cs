using UnityEngine;

namespace YinYangVFX
{
    /// <summary>
    /// Test helper: fires a weapon effect on a timer, alternating facing right / left.
    /// Used by the generated VFX_Weapon_*_Test scenes.
    /// </summary>
    public class WeaponEffectDemo : MonoBehaviour
    {
        public WeaponEffect prefab;
        public float interval = 2f;
        public float sizeMultiplier = 1f;
        public float startDelay = 0.4f;
        [Tooltip("Every second shot faces left.")]
        public bool alternateMirror = false;

        public float holdSeconds = 2.6f;

        float _timer;
        int _index;
        WeaponEffect _active;
        float _stopAt;
        float _pulseAt;

        void Start()
        {
            _timer = startDelay;
        }

        void Update()
        {
            if (_active != null && prefab != null && prefab.loop)
            {
                if (_pulseAt > 0f && Time.time >= _pulseAt)
                {
                    _pulseAt = 0f;
                    _active.Pulse();
                }

                if (Time.time >= _stopAt)
                {
                    _active.Stop();
                    _active = null;
                }
            }

            _timer -= Time.deltaTime;
            if (_timer > 0f)
            {
                return;
            }

            _timer = interval;
            bool mirror = alternateMirror && (_index++ % 2 == 1);
            WeaponEffect fx = WeaponEffect.Spawn(prefab, transform.position, 0f, mirror, sizeMultiplier);
            if (prefab.loop)
            {
                _active = fx;
                _stopAt = Time.time + holdSeconds;
                _pulseAt = Time.time + holdSeconds * 0.5f;
            }
        }
    }
}
