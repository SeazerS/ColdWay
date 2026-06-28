using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ButonSesi : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (StarterAssets.AudioManager.instance != null)
        {
            StarterAssets.AudioManager.instance.Play("Button_Týklama");
        }
    }
}
