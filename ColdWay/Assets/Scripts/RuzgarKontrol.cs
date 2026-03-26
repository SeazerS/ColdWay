using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuzgarKontrol : MonoBehaviour
{
    public WindZone ruzgar;
    public ParticleSystem karPartikul;

    void Start()
    {
        ruzgar = GetComponent<WindZone>();
    }

    public void BolgeGecisi(int bolge)
    {
        var hiz = karPartikul.velocityOverLifetime;
        hiz.enabled = true;

        switch (bolge)
        {
            case 1:
                ruzgar.windMain = 0f;
                ruzgar.windTurbulence = 0f;
                hiz.x = new ParticleSystem.MinMaxCurve(0f);
                break;

            case 2:
                ruzgar.windMain = 0.3f;
                ruzgar.windTurbulence = 0.2f;
                hiz.x = new ParticleSystem.MinMaxCurve(-1f);
                break;

            case 3:
                ruzgar.windMain = 0.7f;
                ruzgar.windTurbulence = 0.5f;
                hiz.x = new ParticleSystem.MinMaxCurve(-3f);
                break;

            case 4:
                ruzgar.windMain = 1f;
                ruzgar.windTurbulence = 0.8f;
                hiz.x = new ParticleSystem.MinMaxCurve(-5f);
                break;
        }
    }
}
