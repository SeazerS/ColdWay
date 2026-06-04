using UnityEngine;

// === DEÐÝÞÝKLÝK: Sound sýnýfýný da StarterAssets odasýna aldýk ===
namespace StarterAssets
{
    [System.Serializable]
    public class Sounds
    {
        public string audioName;
        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume = 0.7f;

        [Range(.1f, 3f)]
        public float pitch = 1f;

        public bool loop;

        [HideInInspector]
        public AudioSource source;
    }
}