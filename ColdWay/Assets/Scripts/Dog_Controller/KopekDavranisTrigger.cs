using UnityEngine;

public class KopekDavranisTrigger : MonoBehaviour
{
    public KopekDavranis kopekDavranis;

    public enum TriggerTip
    {
        Normal,
        Huzursuz,
        GeriCekil
    }

    public TriggerTip triggerTip;

    void OnTriggerEnter(Collider diger)
    {
        if (!diger.CompareTag("Player")) return;

        switch (triggerTip)
        {
            case TriggerTip.Normal:
                kopekDavranis.NormalMod();
                break;

            case TriggerTip.Huzursuz:
                kopekDavranis.HuzursuzMod();
                break;

            case TriggerTip.GeriCekil:
                kopekDavranis.GeriCekilMod();
                break;
        }
    }
}
