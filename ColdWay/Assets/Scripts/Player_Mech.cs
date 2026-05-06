using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player_Mech : MonoBehaviour
{
    public Inventory inventory;
    public ItemSO woodItemSO;
    public GameObject woodPilePrefab;
    public GameObject fireParticlePrefab;
    public float interactionDistance = 3f;

    [Header("UI Ayarlarý")]
    public Image interactionBar;   // Buraya Bar_Fire (Kýrmýzý Image) gelecek
    public GameObject barContainer; // Buraya Bar_Bg (Gri Arka Plan) gelecek

    private GameObject currentWoodPile;
    private GameObject visualCylinders;

    private int phase = 0; // 0: Boþ, 1: Küreler, 2: Silindirler, 3: Ateþ Yanýyor
    private float holdTimer = 0f;
    private float requiredHoldTime = 3f;

    void Start()
    {
        // Baþlangýçta her þeyi kapatalým
        if (barContainer) barContainer.SetActive(false);
        if (interactionBar) interactionBar.gameObject.SetActive(false);
    }

    void Update()
    {
        // TEK SEFERLÝK BASIÞLAR (Faz 0 ve 1)
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (phase == 0)
            {
                PlaceInitialBase(); // Küreleri direkt koyar
            }
            else if (phase == 1)
            {
                if (CheckAndRemoveWood(3)) // Sadece silindirler için odun eksiltir
                {
                    if (visualCylinders != null) visualCylinders.SetActive(true);
                    phase = 2;
                    Debug.Log("Odunlar dizildi! Ateþ yakmak için F'ye basýlý tut.");
                }
                else
                {
                    Debug.Log("Üst odunlar için 3 odun lazým!");
                }
            }
        }

        // BASILI TUTMA (Sadece Faz 2'de)
        if (phase == 2 && currentWoodPile != null)
        {
            if (Input.GetKey(KeyCode.F))
            {
                UpdateFireLightingProgress();
            }

            if (Input.GetKeyUp(KeyCode.F))
            {
                ResetProgress();
            }
        }
    }

    void PlaceInitialBase()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 spawnPos;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
            spawnPos = hit.point;
        else
            spawnPos = transform.position + transform.forward * 2f;

        currentWoodPile = Instantiate(woodPilePrefab, spawnPos, Quaternion.identity);

        // Prefab içindeki silindirleri bul ve gizle
        Transform cylTransform = currentWoodPile.transform.Find("Cylinders");
        if (cylTransform != null)
        {
            visualCylinders = cylTransform.gameObject;
            visualCylinders.SetActive(false);
        }

        phase = 1;
        Debug.Log("Küreler yerleþtirildi.");
    }

    void UpdateFireLightingProgress()
    {
        // Bar yapýsýný görünür yap
        if (barContainer != null && !barContainer.activeSelf) barContainer.SetActive(true);
        if (interactionBar != null && !interactionBar.gameObject.activeSelf) interactionBar.gameObject.SetActive(true);

        holdTimer += Time.deltaTime;

        if (interactionBar != null)
            interactionBar.fillAmount = holdTimer / requiredHoldTime;

        if (holdTimer >= requiredHoldTime)
        {
            LightFire();
            ResetProgress();
        }
    }

    void LightFire()
    {
        GameObject fire = Instantiate(fireParticlePrefab, currentWoodPile.transform);
        // Ateþi biraz yukarý kaldýr (odunlarýn içinde kalmasýn)
        fire.transform.localPosition = new Vector3(0, 0.5f, 0);
        fire.transform.localRotation = Quaternion.Euler(-90, 0, 0);

        phase = 3;
        Debug.Log("Kamp ateþi yandý!");
    }

    void ResetProgress()
    {
        holdTimer = 0f;
        if (interactionBar)
        {
            interactionBar.fillAmount = 0;
            interactionBar.gameObject.SetActive(false);
        }
        if (barContainer) barContainer.SetActive(false);
    }

    bool CheckAndRemoveWood(int requiredAmount)
    {
        int totalWood = 0;
        foreach (var slot in inventory.allSlots)
        {
            if (slot.HasItem() && slot.GetItem() == woodItemSO)
                totalWood += slot.GetAmount();
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
}
