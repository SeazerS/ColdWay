using UnityEngine;
using UnityEditor;

public class TerrainSmoother : EditorWindow
{
    int smoothPass = 20;
    float smoothStrength = 0.8f;

    [MenuItem("Tools/Terrain Smoother")]
    static void OpenWindow()
    {
        GetWindow<TerrainSmoother>("Terrain Smoother");
    }

    void OnGUI()
    {
        GUILayout.Label("Terrain Smooth Ayarlarý", EditorStyles.boldLabel);

        smoothPass = EditorGUILayout.IntSlider("Smooth Geçiþ Sayýsý", smoothPass, 1, 50);
        smoothStrength = EditorGUILayout.Slider("Smooth Gücü", smoothStrength, 0.1f, 1f);

        GUILayout.Space(10);

        Terrain selected = GetSelectedTerrain();
        if (selected != null)
            EditorGUILayout.HelpBox("Hedef: " + selected.name, MessageType.Info);
        else
            EditorGUILayout.HelpBox("Hierarchy'den bir Terrain seç!", MessageType.Warning);

        GUILayout.Space(5);

        if (GUILayout.Button("SMOOTH UYGULA", GUILayout.Height(40)))
        {
            if (selected != null)
                ApplySmooth(selected);
            else
                Debug.LogWarning("Önce Hierarchy'den bir Terrain seç!");
        }
    }

    Terrain GetSelectedTerrain()
    {
        if (Selection.activeGameObject != null)
            return Selection.activeGameObject.GetComponent<Terrain>();
        return null;
    }

    void ApplySmooth(Terrain terrain)
    {
        TerrainData data = terrain.terrainData;
        int res = data.heightmapResolution;
        float[,] heights = data.GetHeights(0, 0, res, res);

        for (int pass = 0; pass < smoothPass; pass++)
        {
            float[,] smoothed = (float[,])heights.Clone();

            for (int y = 1; y < res - 1; y++)
            {
                for (int x = 1; x < res - 1; x++)
                {
                    float avg =
                        heights[y, x] * 0.20f +
                        heights[y + 1, x] * 0.125f +
                        heights[y - 1, x] * 0.125f +
                        heights[y, x + 1] * 0.125f +
                        heights[y, x - 1] * 0.125f +
                        heights[y + 1, x + 1] * 0.075f +
                        heights[y - 1, x - 1] * 0.075f +
                        heights[y + 1, x - 1] * 0.075f +
                        heights[y - 1, x + 1] * 0.075f;

                    smoothed[y, x] = Mathf.Lerp(heights[y, x], avg, smoothStrength);
                }
            }
            heights = smoothed;
        }

        data.SetHeights(0, 0, heights);
        Debug.Log($"{terrain.name} smooth tamamlandý! {smoothPass} geçiþ uygulandý.");
    }
}
