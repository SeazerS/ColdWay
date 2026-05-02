using UnityEngine;

public class ScaleKilitle : MonoBehaviour
{
    private Vector3 hedefScale = Vector3.one;
    public float yOffset = 0f; // Inspector'dan ayarla

    void LateUpdate()
    {
        transform.localScale = hedefScale;

        // Y pozisyonunu düzelt
        Vector3 pos = transform.position;
        pos.y = transform.parent.position.y + yOffset;
        transform.position = pos;
    }
}