using UnityEngine;
using UnityEngine.Audio;

namespace StarterAssets
{
    [System.Serializable]
    public class Sounds
    {
        public string audioName;
        public AudioClip clip;

        public AudioMixerGroup mixerGroup;

        [Range(0f, 1f)] public float volume;
        [Range(0.1f, 3f)] public float pitch;
        public bool loop;

        [HideInInspector] public AudioSource source;
        [HideInInspector] public float originalVolume;
    }
}