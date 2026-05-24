using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IpucuYoneticisi : MonoBehaviour
{
    public static IpucuYoneticisi Instance;

    public GameObject ipucuPanel;
    public TextMeshProUGUI ipucuText;

    private Dictionary<string, string> aktifMesajlar
        = new Dictionary<string, string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        aktifMesajlar.Clear();
        if (ipucuPanel != null)
            ipucuPanel.SetActive(false);
    }

    public void MesajGoster(string kaynak, string mesaj)
    {
        aktifMesajlar[kaynak] = mesaj;
        Guncelle();
    }

    public void MesajGizle(string kaynak)
    {
        if (aktifMesajlar.ContainsKey(kaynak))
            aktifMesajlar.Remove(kaynak);
        Guncelle();
    }

    void Guncelle()
    {
        if (aktifMesajlar.Count == 0)
        {
            ipucuPanel.SetActive(false);
            return;
        }

        ipucuPanel.SetActive(true);
        foreach (var mesaj in aktifMesajlar)
            ipucuText.text = mesaj.Value;
    }
}
