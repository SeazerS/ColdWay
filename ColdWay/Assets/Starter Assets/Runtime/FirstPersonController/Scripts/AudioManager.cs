using System;
using UnityEngine;

namespace StarterAssets
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;
        public Sounds[] sounds;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            foreach (Sounds s in sounds)
            {
                s.source = gameObject.AddComponent<AudioSource>();
                s.source.clip = s.clip;

                s.source.outputAudioMixerGroup = s.mixerGroup;

                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
                s.originalVolume = s.volume;
            }
        }

        public void Play(string name)
        {
            Sounds s = Array.Find(sounds, sound => sound.audioName == name);
            if (s == null)
            {
                Debug.LogWarning("Ses bulunamadý: " + name);
                return;
            }

            if (name == "Yurume_Sesi" || name == "Magara_Yurume_Sesi")
            {
                s.source.pitch = s.pitch + UnityEngine.Random.Range(-0.15f, 0.15f);
            }
            else
            {
                s.source.pitch = s.pitch;
            }

            s.source.Play();
        }

        public void Stop(string name)
        {
            Sounds s = Array.Find(sounds, sound => sound.audioName == name);
            if (s == null) return;
            s.source.Stop();
        }
    }
}