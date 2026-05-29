using System.Collections;
using UnityEngine;

public class KopekIziSistemi : MonoBehaviour
{
    [Header("Pati Pozisyonlarý (Varsa Sürükle)")]
    public Transform onSagPati;
    public Transform onSolPati;
    public Transform arkaSagPati;
    public Transform arkaSolPati;

    [Header("Manuel Offset Ayarý")]
    public float ileriOffset = 0.2f;   // + ileri, - geri
    public float yanOffset = 0.15f;    // sað/sol mesafe
    public float yukseklikOffset = 0f; // yukarý/aþaðý
    [Header("Pençe Ýzi")]
    public Sprite penceIziSprite;

    [Header("Ayarlar")]
    public float izYasomreSuresi = 10f;
    public float solmaHizi = 1f;
    public float izBoyutu = 0.25f;
    public float yerdenYukseklik = 0.02f;
    public LayerMask zeminLayer;

    [Header("Adým Ayarlarý")]
    public float adimArasi = 0.5f;

    private float sonAdimMesafesi = 0f;
    private Vector3 sonPozisyon;
    private int adimSayaci = 0;

    // Köpek 4 ayaklý — 4 farklý offset
    private Vector3[] ayakOffsetleri = new Vector3[]
    {
        new Vector3( 0.15f, 0,  0.2f),  // Sað ön
        new Vector3(-0.15f, 0,  0.2f),  // Sol ön
        new Vector3( 0.15f, 0, -0.2f),  // Sað arka
        new Vector3(-0.15f, 0, -0.2f)   // Sol arka
    };

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
        if (penceIziSprite == null) return;

        Vector3 spawnPoz;

        // Pati transformu varsa direkt kullan
        Transform[] patilar = { onSagPati, onSolPati,
                            arkaSagPati, arkaSolPati };
        Transform mevcutPati = patilar[adimSayaci % 4];

        if (mevcutPati != null)
        {
            spawnPoz = mevcutPati.position;
        }
        else
        {
            // Manuel offset kullan
            Vector3[] offsetler = new Vector3[]
            {
            new Vector3( yanOffset, 0,  ileriOffset),
            new Vector3(-yanOffset, 0,  ileriOffset),
            new Vector3( yanOffset, 0, -ileriOffset),
            new Vector3(-yanOffset, 0, -ileriOffset)
            };
            spawnPoz = transform.position +
                       transform.TransformDirection(offsetler[adimSayaci % 4]);
            spawnPoz.y += yukseklikOffset;
        }

        adimSayaci++;

        // Zemin yüzeyini bul
        RaycastHit hit;
        if (Physics.Raycast(spawnPoz + Vector3.up * 0.5f,
            Vector3.down, out hit, 2f, zeminLayer))
        {
            spawnPoz = hit.point + Vector3.up * yerdenYukseklik;
        }

        GameObject iz = new GameObject("KopekIzi");
        iz.transform.position = spawnPoz;
        iz.transform.rotation = Quaternion.Euler(90f,
            transform.eulerAngles.y, 0f);
        iz.transform.localScale = Vector3.one * izBoyutu;

        SpriteRenderer sr = iz.AddComponent<SpriteRenderer>();
        sr.sprite = penceIziSprite;
        sr.sortingOrder = 1;

        StartCoroutine(IzSol(sr, iz));
    }
    IEnumerator IzSol(SpriteRenderer sr, GameObject iz)
    {
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
