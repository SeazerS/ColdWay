using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    // Oyuncu pozisyonu
    public float pozX, pozY, pozZ;

    public float kopekPozX, kopekPozY, kopekPozZ;

    // Stats
    public float sicaklik;
    public float enerji;

    // Zaman
    public float mevcutSaat;
    public int gunSayisi;

    // Bölge
    public int mevcutBolge;

    // Envanter
    public List<EnvanterItem> envanter = new List<EnvanterItem>();

    // Kayýt zamaný
    public string kayitZamani;

    public string kayitAdi = "Kayit Dosyasi 1";

}

[Serializable]
public class EnvanterItem
{
    public string itemAdi;
    public int miktar;
}

