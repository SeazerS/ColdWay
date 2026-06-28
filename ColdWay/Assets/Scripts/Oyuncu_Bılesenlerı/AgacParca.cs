using UnityEngine;

public class AgacParca : MonoBehaviour
{
    [Header("Referanslar")]
    public ItemSO odunItemSO;
    public Inventory inventory;

    private bool oyuncuYakinda = false;
    private bool toplandimi = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        oyuncuYakinda = true;
        IpucuYoneticisi.Instance?.MesajGoster("agac", "E — Odun Al");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        oyuncuYakinda = false;
        IpucuYoneticisi.Instance?.MesajGizle("agac");
    }

    void Update()
    {
        if (!oyuncuYakinda || toplandimi) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        toplandimi = true;

        if (inventory != null && odunItemSO != null)
            inventory.AddItem(odunItemSO, 1);

        IpucuYoneticisi.Instance?.MesajGizle("agac");
        Destroy(gameObject);
    }
}
