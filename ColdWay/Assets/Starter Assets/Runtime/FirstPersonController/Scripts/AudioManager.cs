using System;
using System.Collections;
using System.Collections.Generic;
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
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
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
            s.source.Play();
        }

        public void Stop(string name)
        {
            Sounds s = Array.Find(sounds, sound => sound.audioName == name);
            if (s == null)
            {
                Debug.LogWarning("Durdurulacak ses bulunamadý: " + name);
                return;
            }
            s.source.Stop();
        }
    }
}