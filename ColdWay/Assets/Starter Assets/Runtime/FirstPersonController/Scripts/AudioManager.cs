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

        // customPitch parametresini -1f varsayýlan deðeriyle ekledik. 
        // Böylece deðer göndermezsen orijinal sistemin çalýþmaya devam edecek.
        public void Play(string name, float customPitch = -1f)
        {
            Sounds s = Array.Find(sounds, sound => sound.audioName == name);
            if (s == null)
            {
                Debug.LogWarning("Ses bulunamadý: " + name);
                return;
            }

            // Eðer özel bir pitch gönderilmediyse (-1 ise), eski sistemini çalýþtýr
            if (customPitch == -1f)
            {
                if (name == "Yurume_Sesi" || name == "Magara_Yurume_Sesi" || name == "Ev_Yurume_Sesi")
                {
                    s.source.pitch = s.pitch + UnityEngine.Random.Range(-0.15f, 0.15f);
                }
                else
                {
                    s.source.pitch = s.pitch;
                }
            }
            // Eðer Inventory'den 0.5f veya 1f gibi özel bir pitch geldiyse, direkt onu kullan
            else
            {
                s.source.pitch = customPitch;
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