using UnityEngine;

public class BaltaSistemi : MonoBehaviour
{
    [Header("Balta Ayarlari")]
    public int maxVurus = 15;
    public int kalanVurus = 15;
    public float vurmaMessafesi = 2.5f;

    [Header("Envanter Baglantisi")]
    public Inventory inventory;
    public ItemSO baltaItem;

    [Header("Enerji")]
    public EnerjiKontrol enerjiKontrol;
    public float enerjiKaybi = 5f;

    [Header("Odun")]
    public GameObject odunPrefab;

    private bool baltaElde = false;
    private int oncekiHotbarIndex = -1;

    void Update()
    {
        if (kalanVurus <= 0) return;

        HotbarKontrol();

        if (baltaElde && Input.GetMouseButtonDown(0))
            Vur();
    }

    void HotbarKontrol()
    {
        if (inventory == null) return;

        int mevcutIndex = inventory.equippedHotbarIndex;
        Slot mevcutSlot = inventory.hotbarSlots[mevcutIndex];

        if (mevcutSlot.HasItem() && mevcutSlot.GetItem() == baltaItem)
        {
            baltaElde = true;
            if (mevcutIndex != oncekiHotbarIndex)
                Debug.Log("Balta ele alindi! Kalan vurus: " + kalanVurus);
        }
        else
        {
            if (baltaElde)
                Debug.Log("Balta birakidi.");
            baltaElde = false;
        }

        oncekiHotbarIndex = mevcutIndex;
    }

    void Vur()
    {
        if (StarterAssets.AudioManager.instance != null)
        {
            StarterAssets.AudioManager.instance.Play("Balta_Savurma");
        }

        // Her sol tikta enerji ve omur azalir
        if (enerjiKontrol != null)
            enerjiKontrol.BaltaKullanildi();

        kalanVurus--;
        Debug.Log("Balta vurus: " + kalanVurus + " / " + maxVurus + " kaldi.");

        // Balta bitti mi?
        if (kalanVurus <= 0)
        {
            baltaElde = false;
            EnvanterdenKaldir();
            Debug.Log("Balta kiridi!");
            return;
        }

        // Agac kesme kontrolu
        Camera cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(
            new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, vurmaMessafesi))
        {
            AgacKesme agac = hit.collider
                .GetComponentInParent<AgacKesme>();
            if (agac != null)
            {
                agac.Vur(transform.position, hit.point);
                return;
            }
        }
    }

    void EnvanterdenKaldir()
    {
        if (inventory == null || baltaItem == null) return;

        foreach (Slot slot in inventory.allSlots)
        {
            if (slot.HasItem() && slot.GetItem() == baltaItem)
            {
                slot.ClearSlot();
                return;
            }
        }
    }
}