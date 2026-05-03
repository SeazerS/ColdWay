using UnityEngine;
using UnityEngine.AI;

public class KopekAI : MonoBehaviour
{
    private string mevcutDurum = "idle";

    [Header("Baðlantýlar")]
    public Transform oyuncu;
    public Animator kopekAnimator;

    [Header("Mesafe Ayarlarý")]
    public float yurumeBaslangic = 10f;
    public float kosmaBaslangic = 5f;
    public float durmaMessafesi = 2f;

    private Animator animator;
    private NavMeshAgent agent;

    void Start()
    {
        if (kopekAnimator == null)
            kopekAnimator =
                GetComponentInChildren<Animator>();
        animator = kopekAnimator;
        agent = GetComponent<NavMeshAgent>();

        if (oyuncu == null)
        {
            GameObject p =
                GameObject.FindWithTag("Player");
            if (p != null)
                oyuncu = p.transform;
        }
    }

    void Update()
    {
        if (animator == null) return;
        if (agent == null) return;
        if (oyuncu == null) return;

        float mesafe = Vector3.Distance(
            transform.position,
            oyuncu.position);

        if (mevcutDurum == "dur" ||
            mevcutDurum == "idle")
        {
            Vector3 yon = oyuncu.position -
                          transform.position;
            yon.y = 0;

            if (yon != Vector3.zero)
            {
                Quaternion hedefRotasyon =
                    Quaternion.LookRotation(yon);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    hedefRotasyon,
                    15f * Time.deltaTime);
            }
        }

        // Hysteresis — ani geçiþi önler
        if (mevcutDurum == "idle" &&
            mesafe < yurumeBaslangic - 2f)
            mevcutDurum = "walk";
        else if (mevcutDurum == "walk" &&
            mesafe > yurumeBaslangic + 2f)
            mevcutDurum = "idle";
        else if (mevcutDurum == "walk" &&
            mesafe < kosmaBaslangic - 1f)
            mevcutDurum = "run";
        else if (mevcutDurum == "run" &&
            mesafe > kosmaBaslangic + 1f)
            mevcutDurum = "walk";
        else if (mevcutDurum == "run" &&
            mesafe < durmaMessafesi)
            mevcutDurum = "dur";
        else if (mevcutDurum == "dur" &&
            mesafe > durmaMessafesi + 0.5f)
            mevcutDurum = "run";

        switch (mevcutDurum)
        {
            case "idle":
                agent.ResetPath();
                animator.SetBool("yuruyorum", false);
                animator.SetBool("kosuyorum", false);
                break;

            case "walk":
                agent.speed = 2f;
                agent.angularSpeed = 720f; // ekle
                agent.SetDestination(oyuncu.position);

                // Gerçekten hareket ediyorsa walk
                bool hareket = agent.velocity.magnitude > 0.2f;
                animator.SetBool("yuruyorum", hareket);
                animator.SetBool("kosuyorum", false);
                break;

            case "run":
                agent.speed = 4f;
                agent.angularSpeed = 720f; // ekle
                agent.SetDestination(oyuncu.position);

                // Gerçekten hareket ediyorsa run
                bool kosma = agent.velocity.magnitude > 0.2f;
                animator.SetBool("yuruyorum", false);
                animator.SetBool("kosuyorum", kosma);
                break;

            case "dur":
                agent.ResetPath();
                animator.SetBool("yuruyorum", false);
                animator.SetBool("kosuyorum", false);
                break;
        }
    }
}