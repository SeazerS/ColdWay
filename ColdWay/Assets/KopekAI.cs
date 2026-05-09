using UnityEngine;
using UnityEngine.AI;

public class KopekAI : MonoBehaviour
{
    private string mevcutDurum = "idle";

    [Header("Baglantilar")]
    public Transform oyuncu;
    public Animator kopekAnimator;

    [Header("Mesafe Ayarlari")]
    public float yurumeBaslangic = 10f;
    public float kosmaBaslangic = 5f;
    public float durmaMessafesi = 2f;

    [Header("Performans")]
    public float hedefGuncellemeSuresi = 0.1f; // Saniyede 10 kez guncelle

    private Animator animator;
    private NavMeshAgent agent;
    private float hedefGuncelmeZamani = 0.05f;
    private Vector3 sonOyuncuPozisyon;

    void Start()
    {
        if (kopekAnimator == null)
            kopekAnimator = GetComponentInChildren<Animator>();
        animator = kopekAnimator;
        agent = GetComponent<NavMeshAgent>();

        // StoppingDistance ayarla — dibine yapismaz
        agent.stoppingDistance = durmaMessafesi;
        agent.angularSpeed = 720f;
        agent.acceleration = 20f; // Hizli tepki

        if (oyuncu == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) oyuncu = p.transform;
        }
    }

    void Update()
    {
        if (animator == null || agent == null || oyuncu == null) return;

        float mesafe = Vector3.Distance(transform.position, oyuncu.position);

        DurumGecisKontrol(mesafe);
        DurumUygula(mesafe);
    }

    void DurumGecisKontrol(float mesafe)
    {
        string yeniDurum = mevcutDurum;

        switch (mevcutDurum)
        {
            case "idle":
                if (mesafe < yurumeBaslangic)
                    yeniDurum = "walk";
                break;
            case "walk":
                if (mesafe > yurumeBaslangic + 2f)
                    yeniDurum = "idle";
                else if (mesafe < kosmaBaslangic)
                    yeniDurum = "run";
                break;
            case "run":
                if (mesafe > kosmaBaslangic + 1f)
                    yeniDurum = "walk";
                else if (mesafe < durmaMessafesi)
                    yeniDurum = "dur";
                break;
            case "dur":
                if (mesafe > durmaMessafesi + 1f)
                    yeniDurum = "run";
                break;
        }

        // Durum degistiyse hemen SetDestination cagir
        if (yeniDurum != mevcutDurum)
        {
            mevcutDurum = yeniDurum;

            // Timer ve pozisyon sifirla — aninda hareket baslat
            hedefGuncelmeZamani = hedefGuncellemeSuresi;
            sonOyuncuPozisyon = Vector3.zero;
        }
    }

    void DurumUygula(float mesafe)
    {
        switch (mevcutDurum)
        {
            case "idle":
                agent.ResetPath();
                AnimasyonAyarla(false, false);
                break;

            case "walk":
                agent.speed = 2f;
                HedefGuncelle();
                bool yuruyor = agent.hasPath &&
                               !agent.pathPending &&
                               agent.velocity.magnitude > 0.2f;
                AnimasyonAyarla(yuruyor, false);
                break;

            case "run":
                agent.speed = 7f;
                HedefGuncelle();
                bool kosuyor = agent.hasPath &&
                               !agent.pathPending &&
                               agent.velocity.magnitude > 0.2f;
                AnimasyonAyarla(false, kosuyor);
                break;

            case "dur":
                agent.ResetPath();
                AnimasyonAyarla(false, false);
                // Oyuncuya don
                YuzeDon();
                break;
        }
    }

    // SetDestination'i her karede degil, belirli aralikla cagir
    void HedefGuncelle()
    {
        hedefGuncelmeZamani += Time.deltaTime;
        if (hedefGuncelmeZamani < hedefGuncellemeSuresi) return;

        hedefGuncelmeZamani = 0f;

        // Oyuncu cok az hareket ettiyse guncelleme
        if (Vector3.Distance(oyuncu.position, sonOyuncuPozisyon) < 0.5f) return;

        sonOyuncuPozisyon = oyuncu.position;
        agent.SetDestination(oyuncu.position);
    }

    void YuzeDon()
    {
        Vector3 yon = oyuncu.position - transform.position;
        yon.y = 0;
        if (yon == Vector3.zero) return;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(yon),
            10f * Time.deltaTime);
    }

    void AnimasyonAyarla(bool yuruyor, bool kosuyor)
    {
        animator.SetBool("yuruyorum", yuruyor);
        animator.SetBool("kosuyorum", kosuyor);
    }

    public void Havla() { animator.SetTrigger("Havla"); }
    public void KafaEvet() { animator.SetTrigger("KafaEvet"); }
    public void KafaHayir() { animator.SetTrigger("KafaHayir"); }
}