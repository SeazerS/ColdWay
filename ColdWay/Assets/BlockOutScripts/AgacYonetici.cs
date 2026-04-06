using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class AgacYonetici : MonoBehaviour
{
    public int gerekliDarbe = 3;
    public GameObject odunKupPrefab;
    //public GameObject agacbilgi;

    private int mevcutDarbe = 0;

    public void DarbeAl(Vector3 oyuncuKonum)
    {
        //agacbilgi.SetActive(true);
        //agacbilgi.GetComponent<TextMeshProUGUI>().text = "Kesilebilir agaç";
        mevcutDarbe++;
        Debug.Log("Darbe: " + mevcutDarbe +
                  "/" + gerekliDarbe);

        if (mevcutDarbe >= gerekliDarbe)
        {
            // Odun küpü oluþtur
            Vector3 konum = transform.position;
            konum.y = oyuncuKonum.y;
            Instantiate(odunKupPrefab, konum,
                        Quaternion.identity);

            // Aðacý yok et
            //agacbilgi.SetActive(false);
            Destroy(gameObject);
            Debug.Log("Aðaç kesildi!");
        }
    }
}
