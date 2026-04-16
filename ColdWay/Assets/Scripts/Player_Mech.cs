using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Mech : MonoBehaviour
{
    public Inventory inventory;
    public ItemSO woodItemSO;
    public GameObject woodPilePrefab; 
    public GameObject fireParticlePrefab;
    public float interactionDistance = 3f;

    private GameObject currentWoodPile;
    private bool isFireLit = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            HandleCampfireLogic();
        }
    }

    void HandleCampfireLogic()
    {
        if (currentWoodPile == null)
        {
            if (CheckAndRemoveWood(3))
            {
                PlaceWoodPile();
            }
            else
            {
                Debug.Log("Yeterli odun yok (3 tane lazým)!");
            }
        }
        else if (currentWoodPile != null && !isFireLit)
        {
            LightFire();
        }
    }

    bool CheckAndRemoveWood(int requiredAmount)
    {
        int totalWood = 0;
        foreach (var slot in inventory.allSlots)
        {
            if (slot.HasItem() && slot.GetItem() == woodItemSO)
            {
                totalWood += slot.GetAmount();
            }
        }

        if (totalWood >= requiredAmount)
        {
            int amountToRemove = requiredAmount;
            foreach (var slot in inventory.allSlots)
            {
                if (slot.HasItem() && slot.GetItem() == woodItemSO)
                {
                    int currentAmount = slot.GetAmount();
                    if (currentAmount >= amountToRemove)
                    {
                        slot.SetItem(woodItemSO, currentAmount - amountToRemove);
                        if (slot.GetAmount() <= 0) slot.ClearSlot();
                        break;
                    }
                    else
                    {
                        amountToRemove -= currentAmount;
                        slot.ClearSlot();
                    }
                }
            }
            return true;
        }
        return false;
    }

    void PlaceWoodPile()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * interactionDistance, Color.red, 2f);

        // Iþýn bir þeye çarptý mý?
        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            currentWoodPile = Instantiate(woodPilePrefab, hit.point, Quaternion.identity);
            isFireLit = false;
        }
        else
        {

            Vector3 fallbackPos = transform.position + transform.forward * 2f;
            currentWoodPile = Instantiate(woodPilePrefab, fallbackPos, Quaternion.identity);
        }
    }

    void LightFire()
    {
        if (currentWoodPile == null) return;
        GameObject fire = Instantiate(fireParticlePrefab);
        fire.transform.SetParent(currentWoodPile.transform);
        fire.transform.localPosition = Vector3.zero;
        fire.transform.localPosition = new Vector3(0, 0.5f, 0);
        fire.transform.localRotation = Quaternion.Euler(-90, 0, 0);

        isFireLit = true;
        Debug.Log("Ateþ tam odunlarýn merkezine yerleþtirildi!");
    }
}
