using UnityEngine;
using UnityEngine.AI;

public class KopekAI : MonoBehaviour
{
    //private Vector3 baslangicScale;

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
        //baslangicScale = transform.localScale;
        
        if (kopekAnimator == null)
            kopekAnimator = GetComponentInChildren<Animator>();

        animator = kopekAnimator;

        agent = GetComponent<NavMeshAgent>();

        if (oyuncu == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null)
                oyuncu = p.transform;
        }
    }


    void Update()
    {
        //transform.localScale = baslangicScale;
        if (animator == null) return;
        if (agent == null) return;
        if (oyuncu == null) return;

        float mesafe = Vector3.Distance(
            transform.position,
            oyuncu.position);

        if (mesafe > yurumeBaslangic)
        {
            agent.ResetPath();
            animator.SetBool("yuruyorum", false);
            animator.SetBool("kosuyorum", false);
        }
        else if (mesafe > kosmaBaslangic)
        {
            agent.speed = 2f;
            agent.SetDestination(oyuncu.position);
            animator.SetBool("yuruyorum", true);
            animator.SetBool("kosuyorum", false);
        }
        else if (mesafe > durmaMessafesi)
        {
            agent.speed = 5f;
            agent.SetDestination(oyuncu.position);
            animator.SetBool("yuruyorum", false);
            animator.SetBool("kosuyorum", true);
        }
        else
        {
            agent.ResetPath();
            animator.SetBool("yuruyorum", false);
            animator.SetBool("kosuyorum", false);
        }
    }
}