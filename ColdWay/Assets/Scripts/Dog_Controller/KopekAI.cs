using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;

public class KopekAI : MonoBehaviour
{
    private string mevcutDurum = "idle";

    [Header("Baglantilar")]
    public Transform oyuncu;
    public Animator kopekAnimator;
    public SicaklikSistemi sicaklikSistemi;
    public EnerjiKontrol enerjiKontrol;

    [Header("Mesafe Ayarlari")]
    public float yurumeBaslangic = 4f;
    public float kosmaBaslangic = 6f;
    public float durmaMessafesi = 1.5f;

    [Header("Yonlendirme Ayarlari")]
    [Range(0f, 1f)]
    public float kritikIsiEsigi = 0.35f;
    public float havlaAraliði = 3f;
    public float atesNoktasiDurmaMessafesi = 3f;

    [Header("Enerji Uyarisi")]
    [Range(0f, 1f)]
    public float kritikEnerjiEsigi = 0.20f;
    public float enerjiUyariHavlaAraligi = 5f;
    private float sonEnerjiHavlaZamani = 0f;
    private bool enerjiUyariAktif = false;

    private Animator animator;
    private NavMeshAgent agent;

    // Yonlendirme
    private bool yonlendirmeAktif = false;
    private Transform enYakinAtesNoktasi;
    private float sonHavlaZamani = 0f;

    private Vector3 sonPozisyon;
    private float takilmaZamani = 0f;
    public float takilmaEsigi = 2f;

    void Start()
    {

        if (kopekAnimator == null)
            kopekAnimator = GetComponentInChildren<Animator>();
        animator = kopekAnimator;
        agent = GetComponent<NavMeshAgent>();

        agent.stoppingDistance = durmaMessafesi;
        agent.updateRotation = false;
        agent.angularSpeed = 720f;
        agent.acceleration = 80f;

        if (oyuncu == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) oyuncu = p.transform;
        }
        if (sicaklikSistemi == null)
            sicaklikSistemi = FindObjectOfType<SicaklikSistemi>();
        if (enerjiKontrol == null)
            enerjiKontrol = FindObjectOfType<EnerjiKontrol>();
    }

    void Update()
    {
        if (animator == null || agent == null || oyuncu == null) return;

        KritikIsiKontrol();
        KritikEnerjiKontrol();

        if (yonlendirmeAktif) { YonlendirmeGuncelle(); return; }
        if (enerjiUyariAktif) { EnerjiUyariGuncelle(); return; }

        float mesafe = Vector3.Distance(transform.position, oyuncu.position);
        DurumGecisKontrol(mesafe);
        DurumUygula(mesafe);

        TakilmaKontrol();

    }

    void KritikIsiKontrol()
    {
        if (sicaklikSistemi == null)
        {
            Debug.LogError("SicaklikSistemi null!");
            return;
        }

        float isiOrani = sicaklikSistemi.mevcutSicaklik / sicaklikSistemi.maxSicaklik;
        Debug.Log($"Isý oraný: {isiOrani} | Kritik eþik: {kritikIsiEsigi} | Yönlendirme: {yonlendirmeAktif}");

        AtesSistemi yakinAtes = EnYakinYananAtesiGetir();
        if (yakinAtes != null && yakinAtes.YaniyorMu())
        {
            Debug.Log("Yakýnda yanan ateþ var, yönlendirme iptal.");
            if (yonlendirmeAktif)
            {
                yonlendirmeAktif = false;
                agent.stoppingDistance = durmaMessafesi;
            }
            return;
        }

        if (isiOrani < kritikIsiEsigi && !yonlendirmeAktif)
        {
            enYakinAtesNoktasi = EnYakinAtesNoktasiniBul();
            Debug.Log($"Ateþ noktasý bulundu mu: {enYakinAtesNoktasi != null}");
            if (enYakinAtesNoktasi != null)
            {
                yonlendirmeAktif = true;
                agent.stoppingDistance = atesNoktasiDurmaMessafesi;
                Debug.Log("Yönlendirme baþladý!");
            }
        }
    }

    void KritikEnerjiKontrol()
    {
        if (enerjiKontrol == null || yonlendirmeAktif) return;
        float oran = enerjiKontrol.mevcutEnerji / enerjiKontrol.maxEnerji;
        enerjiUyariAktif = oran < kritikEnerjiEsigi;
    }

    void YonlendirmeGuncelle()
    {
        if (enYakinAtesNoktasi == null) { yonlendirmeAktif = false; return; }

        float mesafe = Vector3.Distance(
            transform.position, enYakinAtesNoktasi.position);

        if (mesafe > atesNoktasiDurmaMessafesi + 0.5f)
        {
            agent.isStopped = false; // ? bunu ekle
            agent.speed = 8f;
            agent.SetDestination(enYakinAtesNoktasi.position);

            if (agent.velocity.magnitude > 0.2f)
                AnimasyonAyarla(false, true);
            else
                AnimasyonAyarla(false, false);

            // Yumuþak rotasyon
            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                Quaternion hedef = Quaternion.LookRotation(
                    agent.velocity.normalized);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, hedef, 8f * Time.deltaTime);
            }
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();
            AnimasyonAyarla(false, false);
            OyuncuyaBak();
            HavlaZamanla();
        }
    }

    void EnerjiUyariGuncelle()
    {
        float mesafe = Vector3.Distance(transform.position, oyuncu.position);
        if (mesafe > 2.5f)
        {
            agent.speed = 4f;
            agent.SetDestination(oyuncu.position);
            AnimasyonAyarla(agent.velocity.magnitude > 0.1f, false);
        }

        else
        {
            agent.ResetPath();
            AnimasyonAyarla(false, false);
            OyuncuyaBak();
            if (Time.time - sonEnerjiHavlaZamani >= enerjiUyariHavlaAraligi)
            {
                sonEnerjiHavlaZamani = Time.time;
                animator.SetTrigger("Havla");
            }
        }

        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion hedefRotasyon = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, hedefRotasyon, 8f * Time.deltaTime);
        }
    }

    void DurumGecisKontrol(float mesafe)
    {
        string yeniDurum = mevcutDurum;
        switch (mevcutDurum)
        {
            case "idle":
            case "dur":
                // Herhangi bir mesafede aninda tepki ver
                if (mesafe > durmaMessafesi)
                    yeniDurum = mesafe > kosmaBaslangic ? "run" : "walk";
                break;
            case "walk":
                if (mesafe > kosmaBaslangic)
                    yeniDurum = "run";
                else if (mesafe <= durmaMessafesi)
                    yeniDurum = "dur";
                break;
            case "run":
                if (mesafe <= yurumeBaslangic)
                    yeniDurum = "walk";
                break;
        }

        if (yeniDurum != mevcutDurum)
            mevcutDurum = yeniDurum;
    }

    void DurumUygula(float mesafe)
    {
        void DurumUygula(float mesafe)
        {
            agent.isStopped = false;
            agent.SetDestination(oyuncu.position);
        }
        // Yumuþak rotasyon
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Quaternion hedefRotasyon = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, hedefRotasyon, 8f * Time.deltaTime);
        }
        agent.SetDestination(oyuncu.position);

        if (mesafe > 5f)
        {
            agent.speed = 10f;
            AnimasyonAyarla(false, true);
        }
        else if (mesafe > 2f)
        {
            agent.speed = 3f;
            AnimasyonAyarla(true, false);
        }
        else
        {
            agent.speed = 0f;
            AnimasyonAyarla(false, false);
        }
    }
    void OyuncuyaBak()
    {
        Vector3 yon = oyuncu.position - transform.position;
        yon.y = 0;
        if (yon == Vector3.zero) return;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(yon),
            8f * Time.deltaTime);
    }

    void HavlaZamanla()
    {
        if (Time.time - sonHavlaZamani >= havlaAraliði)
        {
            sonHavlaZamani = Time.time;
            animator.SetTrigger("Havla");
        }
    }

    Transform EnYakinAtesNoktasiniBul()
    {
        AteþNoktasi[] noktalar = FindObjectsOfType<AteþNoktasi>();
        Transform enYakin = null;
        float enYakinMesafe = float.MaxValue;
        foreach (AteþNoktasi nokta in noktalar)
        {
            if (nokta.atesSistemi != null && nokta.atesSistemi.YaniyorMu()) continue;
            float m = Vector3.Distance(transform.position, nokta.transform.position);
            if (m < enYakinMesafe) { enYakinMesafe = m; enYakin = nokta.transform; }
        }
        return enYakin;
    }

    AtesSistemi EnYakinYananAtesiGetir()
    {
        AtesSistemi[] atesler = FindObjectsOfType<AtesSistemi>();
        foreach (AtesSistemi ates in atesler)
        {
            if (ates.YaniyorMu())
            {
                float m = Vector3.Distance(oyuncu.position, ates.transform.position);
                if (m < 15f) return ates;
            }
        }
        return null;
    }

    void AnimasyonAyarla(bool yuruyor, bool kosuyor)
    {
        animator.SetBool("yuruyorum", yuruyor);
        animator.SetBool("kosuyorum", kosuyor);
    }

    void TakilmaKontrol()
    {
        if (agent.velocity.magnitude < 0.1f &&
            Vector3.Distance(transform.position, oyuncu.position) > 3f)
        {
            takilmaZamani += Time.deltaTime;

            if (takilmaZamani >= takilmaEsigi)
            {
                // Takildi - oyuncuya dogru isýnla
                takilmaZamani = 0f;
                NavMeshHit hit;
                // Oyuncunun yakininda gecerli bir NavMesh noktasi bul
                if (NavMesh.SamplePosition(oyuncu.position, out hit, 5f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                    Debug.Log("Kopek takilmadan kurtuldu!");
                }
            }
        }
        else
        {
            takilmaZamani = 0f;
        }
    }

    public void Havla() { animator.SetTrigger("Havla"); }
    public void KafaEvet() { animator.SetTrigger("KafaEvet"); }
    public void KafaHayir() { animator.SetTrigger("KafaHayir"); }
}