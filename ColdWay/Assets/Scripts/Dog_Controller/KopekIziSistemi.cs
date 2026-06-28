using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KopekIziSistemi : MonoBehaviour
{
    [Header("Pençe Ýzi")]
    public Sprite penceIziSprite;

    [Header("Pati Pozisyonlarý (Varsa Sürükle)")]
    public Transform onSagPati;
    public Transform onSolPati;
    public Transform arkaSagPati;
    public Transform arkaSolPati;

    [Header("Manuel Offset")]
    public float ileriOffset = 0.2f;
    public float yanOffset = 0.15f;
    public float yukseklikOffset = 0f;

    [Header("Ýz Ayarlarý")]
    public float izYasomreSuresi = 10f;
    public float solmaHizi = 1f;
    public float izBoyutu = 0.25f;
    public float yerdenYukseklik = 0.02f;
    public LayerMask zeminLayer;

    [Header("Bina Filtresi")]
    public LayerMask binaLayer; // bina layer'ý seç

    [Header("Ýz Rengi")]
    public Color izRengi = new Color(0.75f, 0.80f, 0.85f, 0.5f);

    [Header("Adým Ayarlarý")]
    public float yuruyusAdimArasi = 0.5f;
    public float kosusAdimArasi = 0.9f;

    [Header("Maksimum Ýz")]
    public int maxIzSayisi = 50;

    private float sonAdimMesafesi = 0f;
    private Vector3 sonPozisyon;
    private int adimSayaci = 0;
    private Vector3 sonHareketYonu = Vector3.forward;
    private Queue<GameObject> aktifIzler = new Queue<GameObject>();

    void Start()
    {
        sonPozisyon = transform.position;
        if (zeminLayer == 0)
            zeminLayer = LayerMask.GetMask("Default", "Terrain");
    }

    void Update()
    {
        Vector3 mevcutPoz = new Vector3(
            transform.position.x, 0, transform.position.z);
        Vector3 sonPoz2D = new Vector3(
            sonPozisyon.x, 0, sonPozisyon.z);

        float mesafe = Vector3.Distance(mevcutPoz, sonPoz2D);

        if (mesafe > 0.01f)
        {
            Vector3 yon = (mevcutPoz - sonPoz2D).normalized;
            sonHareketYonu = Vector3.Slerp(
                sonHareketYonu, yon, 10f * Time.deltaTime);
        }

        sonAdimMesafesi += mesafe;
        sonPozisyon = transform.position;

        float hiz = mesafe / Time.deltaTime;
        float adimArasi = hiz > 3f ? kosusAdimArasi : yuruyusAdimArasi;

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
        Transform[] patilar = { onSagPati, onSolPati,
                                 arkaSagPati, arkaSolPati };
        Transform mevcutPati = patilar[adimSayaci % 4];

        if (mevcutPati != null)
        {
            spawnPoz = mevcutPati.position;
        }
        else
        {
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

        RaycastHit hit;
        if (Physics.Raycast(spawnPoz + Vector3.up * 0.5f,
            Vector3.down, out hit, 2f, zeminLayer))
        {
            // Bina zeminine iz býrakma
            if (binaLayer != 0 &&
                (binaLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
                return;

            // Tag kontrolü — Terrain tag'i yoksa iz býrakma
            if (!hit.collider.CompareTag("Terrain") &&
                !hit.collider.CompareTag("Untagged"))
                return;

            spawnPoz = hit.point + Vector3.up * yerdenYukseklik;
        }
        else
        {
            // Zemin bulunamazsa iz býrakma
            return;
        }

        float yonAci = Mathf.Atan2(sonHareketYonu.x, sonHareketYonu.z)
                       * Mathf.Rad2Deg;
        Quaternion izRotasyon = Quaternion.Euler(90f, yonAci, 0f);

        GameObject iz = new GameObject("KopekIzi");
        iz.transform.position = spawnPoz;
        iz.transform.rotation = izRotasyon;
        iz.transform.localScale = Vector3.one * izBoyutu;

        SpriteRenderer sr = iz.AddComponent<SpriteRenderer>();
        sr.sprite = penceIziSprite;
        sr.sortingOrder = 1;
        sr.color = izRengi;

        aktifIzler.Enqueue(iz);
        if (aktifIzler.Count > maxIzSayisi)
        {
            GameObject eskiIz = aktifIzler.Dequeue();
            if (eskiIz != null) Destroy(eskiIz);
        }

        StartCoroutine(IzSol(sr, iz));
    }

    IEnumerator IzSol(SpriteRenderer sr, GameObject iz)
    {
        yield return new WaitForSeconds(
            izYasomreSuresi - (1f / solmaHizi));

        while (sr != null && sr.color.a > 0f)
        {
            Color renk = sr.color;
            renk.a -= Time.deltaTime * solmaHizi;
            sr.color = renk;
            yield return null;
        }

        if (aktifIzler.Contains(iz))
        {
            Queue<GameObject> yeniKuyruk = new Queue<GameObject>();
            foreach (var item in aktifIzler)
                if (item != iz) yeniKuyruk.Enqueue(item);
            aktifIzler = yeniKuyruk;
        }

        if (iz != null) Destroy(iz);
    }
}