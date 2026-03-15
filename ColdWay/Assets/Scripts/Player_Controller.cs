using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Controller : MonoBehaviour
{
    float yuruyushýzý = 2f;
    float kosmahýzý = 5f;

    public float horizontal;
    public float vertical;
    float deger;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        //Koþmanýn kontrol edileceði bölge
        bool kosuyormu = Input.GetKey(KeyCode.LeftShift);
        deger = kosuyormu ? kosmahýzý : yuruyushýzý;

        deger *= Time.deltaTime;

        //Karakterin gideceði yöne göre hareketlenme bölgesi
        transform.Translate(horizontal * deger, 0f, vertical * deger,Space.Self);
    }
}
