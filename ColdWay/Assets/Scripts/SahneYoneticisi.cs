using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SahneYoneticisi : MonoBehaviour
{
    public static SahneYoneticisi Instance;

    [Header("Sahne Ýsimleri")]
    public string[] sahneler = {
        "Bolge1_Orman",
        "Bolge2_BuzGol",
        "Bolge3_Magara"
    };

    private int mevcutIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void SonrakiSahne()
    {
        StartCoroutine(SahneGecisi(mevcutIndex + 1));
    }

    IEnumerator SahneGecisi(int yeniIndex)
    {
        // Kar sistemini sýfýrla
        KarTakip kar = FindObjectOfType<KarTakip>();
        if (kar != null)
            kar.SahneGecisYenile();

        yield return SceneManager.LoadSceneAsync(
            sahneler[yeniIndex],
            LoadSceneMode.Additive);

        int kaldirilacak = yeniIndex - 2;
        if (kaldirilacak >= 0)
        {
            yield return SceneManager
                .UnloadSceneAsync(
                    sahneler[kaldirilacak]);
        }

        mevcutIndex = yeniIndex;
    }
}
