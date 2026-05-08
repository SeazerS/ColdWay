using UnityEngine;

public class BolgeTrigger : MonoBehaviour
{
    [Header("Bu trigger hangi bölge?")]
    public int bolgeNo = 1;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (BolgeYoneticisi.Instance != null)
            BolgeYoneticisi.Instance.BolgeGecisi(bolgeNo);
    }
}

