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
            bool mirror = alternateMirror && (_index++ % 2 == 1);
            WeaponEffect.Spawn(prefab, transform.position, 0f, mirror, sizeMultiplier);
        }
    }
}
