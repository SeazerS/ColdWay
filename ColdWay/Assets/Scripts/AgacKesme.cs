using System.Collections;
using UnityEngine;

public class AgacKesme : MonoBehaviour
{
    [Header("Kesme Ayarlari")]
    public int maxVurus = 4;
    private int kalanVurus;

    [Header("Devrilme")]
    public float devrilmeSuresi = 2f;
    public float devrilmeAcisi = 85f;

    [Header("Parcalar")]
    public GameObject[] parcalar;
    public ItemSO odunItemSO;
    public Inventory inventory;

    [Header("Parca Kayma")]
    public float kaymaMessafesi = 2f;

    [Header("Parca Kayma Yonu")]
    public Vector3 kaymaYonu = Vector3.right;

    private bool devrildi = false;
    private bool devriliyorMu = false;
    private Vector3 devrilmeYonu;
    private int aktifParcaIndex = 0;

    private Collider anaCollider;

    void Start()
    {
        kalanVurus = maxVurus;
        anaCollider = GetComponent<Collider>();
        if (anaCollider != null) anaCollider.enabled = false;
    }

    public void Vur(Vector3 oyuncuPozisyon)
    {
        if (devriliyorMu) return;

        if (!devrildi)
        {
            kalanVurus--;

            if (kalanVurus > 0)
            {
                IpucuYoneticisi.Instance?.MesajGoster(
                    "agac", kalanVurus + " vurus kaldi");
            }
            else
            {
                Vector3 fark = transform.position - oyuncuPozisyon;
                fark.y = 0f;
                devrilmeYonu = fark.normalized;
                StartCoroutine(Devir());
            }
        }
        else
        {
            ParcaKop();
        }
    }

    IEnumerator Devir()
    {
        devriliyorMu = true;
        IpucuYoneticisi.Instance?.MesajGizle("agac");

        Vector3 tabanNoktasi = transform.position;
        Vector3 eksen = Vector3.Cross(
            Vector3.up, devrilmeYonu).normalized;

        float sure = 0f;
        float toplamAci = 0f;

        while (sure < devrilmeSuresi)
        {
            sure += Time.deltaTime;
            float t = sure / devrilmeSuresi;
            float eased = t * t;
            float hedefAci = devrilmeAcisi * eased;
            float deltaAci = hedefAci - toplamAci;
            toplamAci += deltaAci;

            transform.RotateAround(tabanNoktasi, eksen, deltaAci);
            yield return null;
        }

        devrildi = true;
        devriliyorMu = false;

        // Parçalarýn collider'larýný kapat
        foreach (GameObject parca in parcalar)
        {
            if (parca == null) continue;
            Collider col = parca.GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }

        // Ana collider'ý aç
        if (anaCollider != null) anaCollider.enabled = true;

        IpucuYoneticisi.Instance?.MesajGoster(
            "agac", "Aðacý parçalamak için vur");
    }

    void ParcaKop()
    {
        if (aktifParcaIndex >= parcalar.Length) return;

        GameObject parca = parcalar[aktifParcaIndex];
        if (parca == null) return;

        // Ana objeden ayýr — pozisyon deðiþmez
        Vector3 pos = parca.transform.position;
        Quaternion rot = parca.transform.rotation;
        parca.transform.SetParent(null);
        parca.transform.position = pos;
        parca.transform.rotation = rot;

        // Aðacýn sað yönünde kaydýr
        parca.transform.position += transform.TransformDirection(kaymaYonu) * 0.4f;

        // Kopan parçayý belirgin yap
        Renderer rend = parca.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            rend.GetPropertyBlock(mpb);
            mpb.SetColor("_BaseColor", new Color(0.6f, 0.3f, 0.1f, 1f));
            rend.SetPropertyBlock(mpb);
        }

        // AgacParca scripti ekle
        AgacParca ap = parca.GetComponent<AgacParca>();
        if (ap == null) ap = parca.AddComponent<AgacParca>();
        ap.odunItemSO = odunItemSO;
        ap.inventory = inventory;

        // Trigger collider ekle
        Collider col = parca.GetComponent<Collider>();
        if (col != null) col.enabled = false;
        BoxCollider bc = parca.AddComponent<BoxCollider>();
        bc.isTrigger = true;

        aktifParcaIndex++;

        if (aktifParcaIndex >= parcalar.Length)
        {
            IpucuYoneticisi.Instance?.MesajGizle("agac");
            if (anaCollider != null) anaCollider.enabled = false;
            StartCoroutine(AnaObjeyiGizle());
        }
        else
        {
            IpucuYoneticisi.Instance?.MesajGoster(
                "agac", (parcalar.Length - aktifParcaIndex) + " parça kaldi");
        }
    }

    IEnumerator TriggerYap(Collider col, GameObject parca)
    {
        yield return new WaitForSeconds(1.5f);

        if (col != null)
        {
            Destroy(col);
            BoxCollider bc = parca.AddComponent<BoxCollider>();
            bc.isTrigger = true;
        }
    }

    IEnumerator AnaObjeyiGizle()
    {
        yield return new WaitForSeconds(0.5f);
        Renderer[] rends = GetComponents<Renderer>();
        foreach (Renderer r in rends)
            r.enabled = false;

        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}