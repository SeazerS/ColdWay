using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_MouseLook : MonoBehaviour
{
    public float hassasiyet = 100f;
    public Transform karakterGovde;

    private float xRotasyon = 0f;

    void Start()
    {
        // Cursoru kilitleme bölgesi
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Mouse hareketini al
        float mouseX = Input.GetAxis("Mouse X") *
                       hassasiyet * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") *
                       hassasiyet * Time.deltaTime;

        // Yukarý ve aþaðý bakýþ
        xRotasyon -= mouseY;
        xRotasyon = Mathf.Clamp(xRotasyon, -80f, 80f); // Limit bölgesi
        transform.localRotation = Quaternion.Euler(xRotasyon, 0f, 0f);

        // Sol ve sað bakýþ 
        karakterGovde.Rotate(Vector3.up * mouseX);
    }
}
