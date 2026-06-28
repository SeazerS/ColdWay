using System.Collections;
using UnityEngine;

public class AyakIziSistemi : MonoBehaviour
{
    [Header("Ayak Ýzi Sprite'larý")]
    public Sprite sagAyakSprite;
    public Sprite solAyakSprite;

    [Header("Ayarlar")]
    public float izYasomreSuresi = 10f;
    public float solmaHizi = 1f;
    public float izBoyutu = 0.3f;
    public float yerdenYukseklik = 0.02f;
    public LayerMask zeminLayer;

    [Header("Adým Ayarlarý")]
    public float adimArasi = 0.6f;

    private bool sagAyakSira = true;
    private float sonAdimMesafesi = 0f;
    private Vector3 sonPozisyon;

    void Start()
    {
        sonPozisyon = transform.position;

        if (zeminLayer == 0)
            zeminLayer = LayerMask.GetMask("Default", "Terrain");
    }

    void Update()
    {
        float mesafe = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(sonPozisyon.x, 0, sonPozisyon.z));

        sonAdimMesafesi += mesafe;
        sonPozisyon = transform.position;

        if (sonAdimMesafesi >= adimArasi)
        {
            sonAdimMesafesi = 0f;
            IzBirak();
        }
    }

    void IzBirak()
    {
        Sprite izSprite = sagAyakSira ? sagAyakSprite : solAyakSprite;
        sagAyakSira = !sagAyakSira;

        if (izSprite == null) return;

        // Zemin pozisyonunu bul
        Vector3 spawnPoz = transform.position;
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f,
            Vector3.down, out hit, 2f, zeminLayer))
        {
            spawnPoz = hit.point + Vector3.up * yerdenYukseklik;
        }
        else
        {
            spawnPoz.y += yerdenYukseklik;
        }

        // Sað/sol offset
        Vector3 sag = transform.right * (sagAyakSira ? -0.15f : 0.15f);
        spawnPoz += sag;

        // Ýz objesi oluþtur
        GameObject iz = new GameObject("AyakIzi");
        iz.transform.position = spawnPoz;
        iz.transform.rotation = Quaternion.Euler(90f, transform.eulerAngles.y, 0f);
        iz.transform.localScale = Vector3.one * izBoyutu;

        SpriteRenderer sr = iz.AddComponent<SpriteRenderer>();
        sr.sprite = izSprite;
        sr.sortingOrder = 1;

        StartCoroutine(IzSol(sr, iz));
    }

    IEnumerator IzSol(SpriteRenderer sr, GameObject iz)
    {
        float gecenSure = 0f;
        Color baslangicRenk = sr.color;

        yield return new WaitForSeconds(izYasomreSuresi - (1f / solmaHizi));

        while (sr != null && sr.color.a > 0f)
        {
            Color renk = sr.color;
            renk.a -= Time.deltaTime * solmaHizi;
            sr.color = renk;
            yield return null;
        }

        if (iz != null) Destroy(iz);
    }
}
