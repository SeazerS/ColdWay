using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSistemi : MonoBehaviour
{
    public static SaveSistemi Instance;

    [Header("Referanslar")]
    public Transform oyuncu;
    public SicaklikSistemi sicaklikSistemi;
    public EnerjiKontrol enerjiKontrol;
    public Inventory envanter;
    public GecGunduzSistemi gecGunduz;
    public GunSayaci gunSayaci;
    public BolgeYoneticisi bolgeYoneticisi;

    [Header("Köpek")]
    public Transform kopek;

    // Save dosyasýnýn yolu
    private string savePath =>
        Application.persistentDataPath + "/save.json";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    // ??? KAYDET ???????????????????????????????????????????????????????????

    public void Kaydet()
    {
        SaveData data = new SaveData();

        // Mevcut kayýt adýný koru
        if (File.Exists(savePath))
        {
            string eskiJson = File.ReadAllText(savePath);
            SaveData eskiData = JsonUtility.FromJson<SaveData>(eskiJson);
            data.kayitAdi = eskiData.kayitAdi;
        }
        else
        {
            data.kayitAdi = "Kayit Dosyasi 1";
        }


        // Pozisyon
        data.pozX = oyuncu.position.x;
        data.pozY = oyuncu.position.y;
        data.pozZ = oyuncu.position.z;

        // Köpek pozisyonu
        data.kopekPozX = kopek != null ? kopek.position.x : 0f;
        data.kopekPozY = kopek != null ? kopek.position.y : 0f;
        data.kopekPozZ = kopek != null ? kopek.position.z : 0f;

        // Stats
        data.sicaklik = sicaklikSistemi != null ?
            sicaklikSistemi.mevcutSicaklik : 100f;
        data.enerji = enerjiKontrol != null ?
            enerjiKontrol.mevcutEnerji : 100f;

        // Zaman
        data.mevcutSaat = gecGunduz != null ?
            gecGunduz.baslangicSaati : 8f;
        data.gunSayisi = gunSayaci != null ?
            GunSayaci.Instance.mevcutGun : 1;

        // Bölge
        data.mevcutBolge = bolgeYoneticisi != null ?
            bolgeYoneticisi.mevcutBolge : 1;

        // Envanter
        data.envanter = new List<EnvanterItem>();
        if (envanter != null)
        {
            foreach (Slot slot in envanter.allSlots)
            {
                if (slot.HasItem())
                {
                    data.envanter.Add(new EnvanterItem
                    {
                        itemAdi = slot.GetItem().itemName,
                        miktar = slot.GetAmount()
                    });
                }
            }
        }

        // Kayýt zamaný
        data.kayitZamani = System.DateTime.Now
            .ToString("dd/MM/yyyy HH:mm");

        // JSON'a yaz
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log("Oyun kaydedildi: " + savePath);
    }

    // ??? YÜKLE ????????????????????????????????????????????????????????????

    public void Yukle()
    {
        if (!SaveVar())
        {
            Debug.Log("Save dosyasý bulunamadý.");
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Köpek pozisyonu
        if (kopek != null)
        {
            KopekAI kopekAI = kopek.GetComponent<KopekAI>();
            UnityEngine.AI.NavMeshAgent agent =
                kopek.GetComponent<UnityEngine.AI.NavMeshAgent>();

            // AI'ý geçici durdur
            if (kopekAI != null) kopekAI.enabled = false;
            if (agent != null) agent.enabled = false;

            // Pozisyonu set et
            kopek.position = new Vector3(
                data.kopekPozX, data.kopekPozY, data.kopekPozZ);

            // Bir frame bekleyip tekrar aç
            StartCoroutine(KopekAIAc(kopekAI, agent));
        }

        // Pozisyon
        if (oyuncu != null)
        {
            CharacterController cc = oyuncu.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            oyuncu.position = new Vector3(data.pozX, data.pozY, data.pozZ);

            if (cc != null) cc.enabled = true;
        }

        // Stats
        if (sicaklikSistemi != null)
        {
            sicaklikSistemi.mevcutSicaklik = data.sicaklik;
        }
        if (enerjiKontrol != null)
        {
            enerjiKontrol.mevcutEnerji = data.enerji;
        }

        // Zaman
        if (gecGunduz != null)
            gecGunduz.baslangicSaati = data.mevcutSaat;

        if (gunSayaci != null)
            GunSayaci.Instance.mevcutGun = data.gunSayisi;

        // Bölge
        if (bolgeYoneticisi != null)
            bolgeYoneticisi.BolgeGecisi(data.mevcutBolge);

        // Envanter
        if (envanter != null)
        {
            // Önce temizle
            foreach (Slot slot in envanter.allSlots)
                slot.ClearSlot();

            // Sonra yükle
            foreach (EnvanterItem item in data.envanter)
            {
                ItemSO itemSO = ItemBul(item.itemAdi);
                if (itemSO != null)
                    envanter.AddItem(itemSO, item.miktar);
            }
        }

        Debug.Log("Oyun yüklendi. Kayýt: " + data.kayitZamani);
    }

    IEnumerator KopekAIAc(KopekAI kopekAI,
    UnityEngine.AI.NavMeshAgent agent)
    {
        yield return new WaitForSeconds(0.2f);

        if (agent != null)
        {
            agent.enabled = true;
            // NavMesh üzerinde geçerli noktaya taþý
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(
                kopek.position, out hit, 5f,
                UnityEngine.AI.NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }

        if (kopekAI != null) kopekAI.enabled = true;
    }

    // ??? SAVE VAR MI ??????????????????????????????????????????????????????

    public bool SaveVar()
    {
        return File.Exists(savePath);
    }

    public string KayitZamaniAl()
    {
        if (!SaveVar()) return "";
        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        return data.kayitZamani;
    }

    // ??? SÝL ??????????????????????????????????????????????????????????????

    public void SaveSil()
    {
        if (File.Exists(savePath))
            File.Delete(savePath);
        Debug.Log("Save silindi.");
    }

    // ??? OTOMATÝK KAYIT ???????????????????????????????????????????????????

    public void OtomatikKaydet()
    {
        Kaydet();
        Debug.Log("Otomatik kayýt yapýldý.");
    }

    // ??? ITEM BUL ?????????????????????????????????????????????????????????

    ItemSO ItemBul(string itemAdi)
    {
        // Resources klasöründen bul
        ItemSO[] tumItemlar = Resources.LoadAll<ItemSO>("Items");
        foreach (ItemSO item in tumItemlar)
            if (item.itemName == itemAdi)
                return item;

        Debug.LogWarning("Item bulunamadý: " + itemAdi);
        return null;
    }

    public void KaydetIsimle(string isim)
{
    Kaydet();
    // Kaydedilen dosyayý tekrar okuyup ismi güncelle
    if (System.IO.File.Exists(savePath))
    {
        string json = System.IO.File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        data.kayitAdi = isim;
        json = JsonUtility.ToJson(data, true);
        System.IO.File.WriteAllText(savePath, json);
    }
}
}
