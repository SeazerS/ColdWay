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

    [Header("Rotasyon")]
    public float rotasyonHizi = 6f;

    [Header("Takilma")]
    public float takilmaEsigi = 2f;

    // Private
    private Animator animator;
    private NavMeshAgent agent;

    private bool yonlendirmeAktif = false;
    private bool enerjiUyariAktif = false;
    private Transform enYakinAtesNoktasi;

    private float sonHavlaZamani = 0f;
    private float sonEnerjiHavlaZamani = 0f;
    private float takilmaZamani = 0f;

    private Vector3 sonHedef = Vector3.zero;
    private float hedefGuncellemeMesafesi = 0.5f;

    [Header("Yonlendirme Ayarlari")]
    public float yakinAtesAlgimaMesafesi = 15f; // ? ekle

    void Start()
    {
        if (kopekAnimator == null)
            kopekAnimator = GetComponentInChildren<Animator>();
        animator = kopekAnimator;
        agent = GetComponent<NavMeshAgent>();

        if (agent == null) { Debug.LogError("NavMeshAgent yok!"); return; }

        agent.stoppingDistance = durmaMessafesi;
        agent.updateRotation = false;
        agent.updatePosition = true;
        agent.isStopped = false;
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

        if (yonlendirmeAktif)
            YonlendirmeGuncelle();
        else if (enerjiUyariAktif)
            EnerjiUyariGuncelle();
        else
        {
            float mesafe = Vector3.Distance(transform.position, oyuncu.position);
            DurumGecisKontrol(mesafe);
            DurumUygula(mesafe);
            TakilmaKontrol();
        }

        // Rotasyon tek yerden yönetiliyor
        RotasyonGuncelle();
    }

    // ??? Kritik Kontroller ????????????????????????????????????????????????

    void KritikIsiKontrol()
    {
        if (sicaklikSistemi == null) return;

        float isiOrani = sicaklikSistemi.mevcutSicaklik /
                         sicaklikSistemi.maxSicaklik;

        // Yakýnda yanan ateþ varsa yönlendirmeyi iptal et
        AtesSistemi yakinAtes = EnYakinYananAtesiGetir();
        if (yakinAtes != null)
        {
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
            if (enYakinAtesNoktasi != null)
            {
                yonlendirmeAktif = true;
                agent.stoppingDistance = atesNoktasiDurmaMessafesi;
            }
        }
        else if (isiOrani >= kritikIsiEsigi && yonlendirmeAktif)
        {
            yonlendirmeAktif = false;
            agent.stoppingDistance = durmaMessafesi;
        }
    }

    void KritikEnerjiKontrol()
    {
        if (enerjiKontrol == null || yonlendirmeAktif) return;
        float oran = enerjiKontrol.mevcutEnerji / enerjiKontrol.maxEnerji;
        enerjiUyariAktif = oran < kritikEnerjiEsigi;
    }

    // ??? Hareket Metodlarý ????????????????????????????????????????????????

    void YonlendirmeGuncelle()
    {
        if (enYakinAtesNoktasi == null) { yonlendirmeAktif = false; return; }

        float mesafe = Vector3.Distance(
            transform.position, enYakinAtesNoktasi.position);

        if (mesafe > atesNoktasiDurmaMessafesi + 0.5f)
        {
            agent.isStopped = false;
            agent.speed = 8f;
            agent.SetDestination(enYakinAtesNoktasi.position);
            AnimasyonAyarla(false, agent.velocity.magnitude > 0.2f);
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();
            AnimasyonAyarla(false, false);
            HavlaZamanla();
        }
    }

    void EnerjiUyariGuncelle()
    {
        float mesafe = Vector3.Distance(transform.position, oyuncu.position);

        if (mesafe > 2.5f)
        {
            agent.isStopped = false;
            agent.speed = 4f;
            agent.SetDestination(oyuncu.position);
            AnimasyonAyarla(agent.velocity.magnitude > 0.1f, false);
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();
            AnimasyonAyarla(false, false);

            if (Time.time - sonEnerjiHavlaZamani >= enerjiUyariHavlaAraligi)
            {
                sonEnerjiHavlaZamani = Time.time;
                animator.SetTrigger("Havla");
            }
        }
    }

    void DurumGecisKontrol(float mesafe)
    {
        string yeniDurum = mevcutDurum;

        switch (mevcutDurum)
        {
            case "idle":
            case "dur":
                if (mesafe > durmaMessafesi)
                    yeniDurum = mesafe > kosmaBaslangic ? "run" : "walk";
                break;
            case "walk":
                if (mesafe > kosmaBaslangic) yeniDurum = "run";
                else if (mesafe <= durmaMessafesi) yeniDurum = "dur";
                break;
            case "run":
                if (mesafe <= yurumeBaslangic) yeniDurum = "walk";
                break;
        }

        if (yeniDurum != mevcutDurum)
            mevcutDurum = yeniDurum;
    }

    void DurumUygula(float mesafe)
    {
        agent.isStopped = false;

        // Oyuncu 0.5 birimden fazla hareket ettiyse güncelle
        if (Vector3.Distance(oyuncu.position, sonHedef) >
            hedefGuncellemeMesafesi)
        {
            sonHedef = oyuncu.position;
            agent.SetDestination(sonHedef);
        }

        float hedefHiz;
        if (mesafe > 5f)
        {
            hedefHiz = 10f;
            AnimasyonAyarla(false, true);
        }
        else if (mesafe > 2f)
        {
            hedefHiz = 3f;
            AnimasyonAyarla(true, false);
        }
        else
        {
            hedefHiz = 0f;
            AnimasyonAyarla(false, false);
        }

        agent.speed = Mathf.Lerp(agent.speed, hedefHiz, Time.deltaTime * 5f);
    }

    // ??? Rotasyon — tek yerden yönetilir ?????????????????????????????????

    void RotasyonGuncelle()
    {
        Vector3 hedefYon = Vector3.zero;

        // Hareket ediyorsa velocity yönüne dön
        if (agent.velocity.sqrMagnitude > 0.05f)
            hedefYon = agent.velocity.normalized;
        // Duruyorsa oyuncuya dön
        else
        {
            Vector3 fark = oyuncu.position - transform.position;
            fark.y = 0;
            if (fark.sqrMagnitude > 0.01f)
                hedefYon = fark.normalized;
        }

        if (hedefYon == Vector3.zero) return;

        hedefYon.y = 0;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(hedefYon),
            rotasyonHizi * Time.deltaTime);
    }

    // ??? Yardýmcýlar ??????????????????????????????????????????????????????

    void HavlaZamanla()
    {
        if (Time.time - sonHavlaZamani >= havlaAraliði)
        {
            sonHavlaZamani = Time.time;
            animator.SetTrigger("Havla");
        }
    }

    void AnimasyonAyarla(bool yuruyor, bool kosuyor)
    {
        animator.SetBool("yuruyorum", yuruyor);
        animator.SetBool("kosuyorum", kosuyor);
    }

    Transform EnYakinAtesNoktasiniBul()
    {
        AteþNoktasi[] noktalar = FindObjectsOfType<AteþNoktasi>();
        Transform enYakin = null;
        float enYakinMesafe = float.MaxValue;

        foreach (AteþNoktasi nokta in noktalar)
        {
            if (nokta.atesSistemi != null &&
                nokta.atesSistemi.YaniyorMu()) continue;

            float m = Vector3.Distance(
                transform.position, nokta.transform.position);
            if (m < enYakinMesafe) { enYakinMesafe = m; enYakin = nokta.transform; }
        }
        return enYakin;
    }

    AtesSistemi EnYakinYananAtesiGetir()
    {
        AtesSistemi[] atesler = FindObjectsOfType<AtesSistemi>();
        foreach (AtesSistemi ates in atesler)
        {
            if (!ates.YaniyorMu()) continue;
            float m = Vector3.Distance(oyuncu.position, ates.transform.position);
            if (m < yakinAtesAlgimaMesafesi) return ates;
        }
        return null;
    }

    private string oncekiDurum = "";

    void TakilmaKontrol()
    {
        // Durum yeni deðiþtiyse sayacý sýfýrla
        if (mevcutDurum != oncekiDurum)
        {
            oncekiDurum = mevcutDurum;
            takilmaZamani = 0f;
            return;
        }

        if (agent.velocity.magnitude < 0.1f &&
            Vector3.Distance(transform.position, oyuncu.position) > 3f)
        {
            takilmaZamani += Time.deltaTime;
            if (takilmaZamani >= takilmaEsigi)
            {
                takilmaZamani = 0f;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(
                    oyuncu.position, out hit, 5f, NavMesh.AllAreas))
                    agent.Warp(hit.position);
            }
        }
        else takilmaZamani = 0f;
    }

    public void Havla() { animator.SetTrigger("Havla"); }
    public void KafaEvet() { animator.SetTrigger("KafaEvet"); }
    public void KafaHayir() { animator.SetTrigger("KafaHayir"); }
}