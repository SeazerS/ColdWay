using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BaltaSistemi : MonoBehaviour
{
    public KeyCode baltaTusu = KeyCode.Q;
    public float vurusMesafesi = 3f;
    public GameObject odunKupPrefab;
    public Camera fpKamera;
    public GameObject agacbilgi;
    public GameObject agac;


    void Start()
    {
        if (fpKamera == null)
            fpKamera = Camera.main;
        agacbilgi.GetComponent<TextMeshProUGUI>().text = "Kesilebilir agac";
    }




    void Update()
    {
        if (Vector3.Distance(transform.position, agac.transform.position) <= 7f)
        {
            agacbilgi.SetActive(true);
        }
        else
        {
            agacbilgi.SetActive(false);
        }
        if (Input.GetKeyDown(baltaTusu))
        {
            BaltaVur();
        }
    }

    void BaltaVur()
    {
        Ray ray = new Ray(
            fpKamera.transform.position,
            fpKamera.transform.forward);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, vurusMesafesi))
        {
            if (hit.collider.CompareTag("KuruAgac"))
            {
                AgacYonetici agac =
                    hit.collider.GetComponent<AgacYonetici>();

                if (agac != null)
                    agac.DarbeAl(transform.position);
            }
        }
    }
}
