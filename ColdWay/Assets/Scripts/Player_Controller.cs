using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    public float yuruyusHizi = 2f;
    public float kosmaHizi = 5f;
    public bool kosmakAktif = true; // ? ekle

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Koþma kontrolü
        bool kosuyorMu = Input.GetKey(KeyCode.LeftShift)
                         && kosmakAktif;
        float hiz = kosuyorMu ? kosmaHizi : yuruyusHizi;

        transform.Translate(
            horizontal * hiz * Time.deltaTime,
            0f,
            vertical * hiz * Time.deltaTime,
            Space.Self
        );
    }

    public bool KosuyorMu()
    {
        return Input.GetKey(KeyCode.LeftShift) && kosmakAktif;
    }
}
