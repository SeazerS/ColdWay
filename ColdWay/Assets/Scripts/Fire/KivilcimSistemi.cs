using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class KivilcimSistemi : MonoBehaviour
{
    [Header("Kivilcim Ayarlari")]
    public RectTransform canvas;        // MinigameCanvas
    public RectTransform kibritObj;     // KibritObj
    public Sprite kivilcimSprite;       // Kucuk yuvarlak sprite (Circle)
    public int maxKivilcim = 12;
    public float kivilcimHizi = 300f;
    public float kivilcimOmru = 0.4f;
    public float boyut = 8f;

    private List<KivilcimParcacik> parcaciklar = new List<KivilcimParcacik>();
    private bool aktif = false;

    class KivilcimParcacik
    {
        public Image img;
        public Vector2 hiz;
        public float kalanOmur;
        public float maxOmur;
    }

    void Update()
    {
        // Var olan kývýlcýmlarý guncelle
        for (int i = parcaciklar.Count - 1; i >= 0; i--)
        {
            var p = parcaciklar[i];
            p.kalanOmur -= Time.deltaTime;

            if (p.kalanOmur <= 0)
            {
                Destroy(p.img.gameObject);
                parcaciklar.RemoveAt(i);
                continue;
            }

            // Hareket
            p.hiz += Vector2.down * 500f * Time.deltaTime; // Yerçekimi
            RectTransform rt = p.img.rectTransform;
            rt.anchoredPosition += p.hiz * Time.deltaTime;

            // Soluklaþ
            float oran = p.kalanOmur / p.maxOmur;
            Color c = p.img.color;
            c.a = oran;
            p.img.color = c;

            // Küçül
            float s = boyut * oran;
            rt.sizeDelta = new Vector2(s, s);
        }

        // Aktifse yeni kivilcim olustur
        if (aktif && parcaciklar.Count < maxKivilcim)
        {
            OlusturKivilcim();
        }
    }

    void OlusturKivilcim()
    {
        // Canvas altinda yeni Image olustur
        GameObject go = new GameObject("Kivilcim");
        go.transform.SetParent(canvas, false);

        Image img = go.AddComponent<Image>();
        if (kivilcimSprite != null)
            img.sprite = kivilcimSprite;

        // Renk: sari-turuncu
        float t = Random.value;
        img.color = Color.Lerp(
            new Color(1f, 0.6f, 0f, 1f),
            new Color(1f, 1f, 0.2f, 1f), t);

        RectTransform rt = img.rectTransform;
        rt.sizeDelta = new Vector2(boyut, boyut);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);

        // Kibritin pozisyonuna koy
        float kibritYukseklik = kibritObj.rect.height * 0.5f;
        rt.anchoredPosition = kibritObj.anchoredPosition +
            new Vector2(Random.Range(-3f, 3f), kibritYukseklik);

        // Rastgele hiz - yukari ve yanlara
        float aci = Random.Range(20f, 160f) * Mathf.Deg2Rad;
        float hizMag = Random.Range(kivilcimHizi * 0.5f, kivilcimHizi);
        Vector2 hiz = new Vector2(
            Mathf.Cos(aci) * hizMag,
            Mathf.Sin(aci) * hizMag);

        var p = new KivilcimParcacik
        {
            img = img,
            hiz = hiz,
            kalanOmur = kivilcimOmru * Random.Range(0.7f, 1.3f),
            maxOmur = kivilcimOmru
        };
        p.maxOmur = p.kalanOmur;

        parcaciklar.Add(p);
    }

    public void Baslat() { aktif = true; }
    public void Durdur() { aktif = false; }

    public void Temizle()
    {
        aktif = false;
        foreach (var p in parcaciklar)
            if (p.img != null) Destroy(p.img.gameObject);
        parcaciklar.Clear();
    }
}
