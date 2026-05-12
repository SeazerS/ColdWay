using UnityEngine;
using UnityEditor;

public class TerrainStitch : EditorWindow
{
    private Terrain terrainA;
    private Terrain terrainB;
    private int blendGenisligi = 5; // Kac piksel karistirilsin

    [MenuItem("Tools/Terrain Stitch")]
    static void OpenWindow()
    {
        GetWindow<TerrainStitch>("Terrain Stitch");
    }

    void OnGUI()
    {
        GUILayout.Label("Terrain Birlesim Duzeltici", EditorStyles.boldLabel);
        GUILayout.Space(5);

        terrainA = (Terrain)EditorGUILayout.ObjectField(
            "Sol/Ust Terrain", terrainA, typeof(Terrain), true);
        terrainB = (Terrain)EditorGUILayout.ObjectField(
            "Sag/Alt Terrain", terrainB, typeof(Terrain), true);

        blendGenisligi = EditorGUILayout.IntSlider(
            "Karisma Genisligi", blendGenisligi, 1, 20);

        GUILayout.Space(10);

        if (terrainA == null || terrainB == null)
        {
            EditorGUILayout.HelpBox(
                "Iki terrain de secilmeli!", MessageType.Warning);
            return;
        }

        if (GUILayout.Button("BIRLESIM DUZELT", GUILayout.Height(40)))
            StitchTerrains();
    }

    void StitchTerrains()
    {
        TerrainData dataA = terrainA.terrainData;
        TerrainData dataB = terrainB.terrainData;

        int resA = dataA.heightmapResolution;
        int resB = dataB.heightmapResolution;

        float[,] heightsA = dataA.GetHeights(0, 0, resA, resA);
        float[,] heightsB = dataB.GetHeights(0, 0, resB, resB);

        Vector3 posA = terrainA.transform.position;
        Vector3 posB = terrainB.transform.position;

        // Yatay birlesim (A'nin sagi, B'nin solu)
        if (Mathf.Approximately(posA.x + dataA.size.x, posB.x))
        {
            Debug.Log("Yatay birlesim duzeltiliyor...");

            for (int y = 0; y < resA; y++)
            {
                float hedefY = (float)y / (resA - 1) * (resB - 1);
                int iy = Mathf.Clamp(Mathf.RoundToInt(hedefY), 0, resB - 1);

                // Ortalama deger hesapla
                float ortalamaYukseklik = (heightsA[y, resA - 1] +
                                           heightsB[iy, 0]) / 2f;

                // Karisma uygula
                for (int b = 0; b < blendGenisligi; b++)
                {
                    float oran = (float)b / blendGenisligi;
                    int xA = resA - 1 - b;
                    int xB = b;

                    if (xA >= 0)
                        heightsA[y, xA] = Mathf.Lerp(
                            ortalamaYukseklik, heightsA[y, xA], oran);
                    if (xB < resB)
                        heightsB[iy, xB] = Mathf.Lerp(
                            ortalamaYukseklik, heightsB[iy, xB], oran);
                }
            }

            dataA.SetHeights(0, 0, heightsA);
            dataB.SetHeights(0, 0, heightsB);
            Debug.Log("Yatay birlesim tamamlandi!");
        }
        // Dikey birlesim (A'nin alti, B'nin ustu)
        else if (Mathf.Approximately(posA.z + dataA.size.z, posB.z))
        {
            Debug.Log("Dikey birlesim duzeltiliyor...");

            for (int x = 0; x < resA; x++)
            {
                float hedefX = (float)x / (resA - 1) * (resB - 1);
                int ix = Mathf.Clamp(Mathf.RoundToInt(hedefX), 0, resB - 1);

                float ortalamaYukseklik = (heightsA[resA - 1, x] +
                                           heightsB[0, ix]) / 2f;

                for (int b = 0; b < blendGenisligi; b++)
                {
                    float oran = (float)b / blendGenisligi;
                    int zA = resA - 1 - b;
                    int zB = b;

                    if (zA >= 0)
                        heightsA[zA, x] = Mathf.Lerp(
                            ortalamaYukseklik, heightsA[zA, x], oran);
                    if (zB < resB)
                        heightsB[zB, ix] = Mathf.Lerp(
                            ortalamaYukseklik, heightsB[zB, ix], oran);
                }
            }

            dataA.SetHeights(0, 0, heightsA);
            dataB.SetHeights(0, 0, heightsB);
            Debug.Log("Dikey birlesim tamamlandi!");
        }
        else
        {
            Debug.LogWarning(
                "Bu iki terrain birbirine bitisik degil! " +
                "Pozisyonlari kontrol et.");
        }
    }
}
