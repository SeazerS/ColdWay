using UnityEngine;
using System.Collections;

public class KopekTrigger : MonoBehaviour
{
    public KopekAI kopek;
    public string animasyonTipi = "Havla";
    public int tekrarSayisi = 3;
    public float bekleme = 1.5f;

    private bool calisiyor = false;

    void OnTriggerEnter(Collider diger)
    {
        if (!diger.CompareTag("Player")) return;
        if (calisiyor) return;

        StartCoroutine(TekrarHavla());
    }

    IEnumerator TekrarHavla()
    {
        calisiyor = true;

        for (int i = 0; i < tekrarSayisi; i++)
        {
            switch (animasyonTipi)
            {
                case "Havla":
                    kopek.Havla();
                    break;
                case "KafaEvet":
                    kopek.KafaEvet();
                    break;
                case "KafaHayir":
                    kopek.KafaHayir();
                    break;
            }

            yield return new WaitForSeconds(bekleme);
        }

        calisiyor = false;
    }
}
