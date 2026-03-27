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
        if (yeniIndex >= sahneler.Length)
        {
            Debug.Log("Oyun bitti — Final sahne");
            yield break;
        }

        // Yeni sahneyi yükle
        yield return SceneManager.LoadSceneAsync(
            sahneler[yeniIndex],
            LoadSceneMode.Additive
        );

        Debug.Log("Yüklendi: " + sahneler[yeniIndex]);

        // Önceki sahneyi kaldýr
        // Bölge 1 ? 2 geçiþinde Bölge 1 açýk kalýr
        // Bölge 2 ? 3 geçiþinde Bölge 1 kapanýr
        int kaldirilacak = yeniIndex - 2;
        if (kaldirilacak >= 0)
        {
            yield return SceneManager
                .UnloadSceneAsync(
                    sahneler[kaldirilacak]);
            Debug.Log("Kaldýrýldý: " +
                      sahneler[kaldirilacak]);
        }

        mevcutIndex = yeniIndex;
    }
}
