using UnityEngine;
using System.Collections;

public class KopekDavranis : MonoBehaviour
{
    [Header("Bağlantılar")]
    public KopekAI kopekAI;
    public Transform oyuncu;

    private string mevcutDurum = "normal";

    void Update()
    {
        switch (mevcutDurum)
        {
            case "normal":
                // Normal takip — KopekAI halleder
                break;

            case "huzursuz":
                // Oyuncuyu takip ama havla
                break;

            case "geriCekil":
                // Geri dön, oyuncuya baк
                GeriCekil();
                break;
        }
    }

    // Doğru yol trigger'ından çağrılır
    public void NormalMod()
    {
        mevcutDurum = "normal";
        kopekAI.yurumeBaslangic = 15f;
        kopekAI.kosmaBaslangic = 8f;
    }

    // Yanlış yol trigger'ından çağrılır
    public void HuzursuzMod()
    {
        mevcutDurum = "huzursuz";
        StartCoroutine(HavlaVeUyar());
    }

    // Çıkmaz trigger'ından çağrılır
    public void GeriCekilMod()
    {
        mevcutDurum = "geriCekil";
        StartCoroutine(GeriCekilCoroutine());
    }

    IEnumerator HavlaVeUyar()
    {
        // 3 kez havla
        for (int i = 0; i < 3; i++)
        {
            kopekAI.Havla();
            yield return new WaitForSeconds(1.5f);
        }

        mevcutDurum = "normal";
    }

    IEnumerator GeriCekilCoroutine()
    {
        // Havla
        kopekAI.Havla();
        yield return new WaitForSeconds(1f);

        // KafaHayir
        kopekAI.KafaHayir();
        yield return new WaitForSeconds(1.5f);

        // Tekrar havla
        kopekAI.Havla();
        yield return new WaitForSeconds(1f);

        mevcutDurum = "normal";
    }

    void GeriCekil()
    {
        // Oyuncuya bak
        Vector3 yon = oyuncu.position -
                      transform.position;
        yon.y = 0;

        if (yon != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(yon),
                5f * Time.deltaTime);
        }
    }
}
