using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public ItemSO woodItem;
    public ItemSO axeItem;

    public GameObject hotbarObj;
    public GameObject inventorySlotParent;
    public GameObject inventorySlotBag;

    public GameObject container;

    public Image dragIcon;

    public float pickupRange = 3f;
    //private Item lookedAtItem = null;
    public Material highlightMaterial;
    //private Material[] originalMaterials;
    //private Renderer lookedAtRenderer = null;
    private List<Renderer> lookedAtRenderers = new List<Renderer>();
    private List<Material[]> originalMaterials = new List<Material[]>();

    //private int equippedHotbarIndex = 0;
    public int equippedHotbarIndex = 0;
    public float equippedOpacity = 0.9f;
    public float normalOpacity = 0.58f;

    public List<Slot> inventorySlots = new List<Slot>();
    public List<Slot> hotbarSlots = new List<Slot>();
    public List<Slot> allSlots = new List<Slot>();

    private Slot draggedSlot = null;
    private bool isDragging = false;

    public GameObject selectionIndicator;

    [Header("Baslangic Esyalari")]
    public ItemSO cadirItem;

    private void Awake()
    {
        inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());
        hotbarSlots.AddRange(hotbarObj.GetComponentsInChildren<Slot>());

        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(hotbarSlots);
    }

    void Start()
    {
        if (PlayerPrefs.GetInt("YeniOyun", 0) == 1)
        {
            PlayerPrefs.SetInt("YeniOyun", 0);
            AddItem(cadirItem, 1);
        }
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool isActive = !container.activeInHierarchy;
            container.SetActive(isActive);

            Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isActive;

            if (StarterAssets.FirstPersonController.Instance != null)
            {
                StarterAssets.FirstPersonController.Instance.CanLook = !isActive;
            }

            if (StarterAssets.AudioManager.instance != null)
            {
                StarterAssets.AudioManager.instance.Play("Canta_Acma");
            }
        }


        DetectLookedAtItem();
        Pickup();

        StartDrag();
        UpdateDragItemPosition();
        EndDrag();

        HandleHotBarSelection();
        HandleDropEquippedItem();
        UpdateHotbarOpacity();

    }
    public void AddItem(ItemSO itemToAdd, int amount)
    {
        int remanining = amount;

        foreach (Slot slot in allSlots)
        {
            if (slot.HasItem() && slot.GetItem() == itemToAdd)
            {
                int currentAmount = slot.GetAmount();
                int maxStack = itemToAdd.maxStackSize;

                if (currentAmount < maxStack)
                {
                    int spaceLeft = maxStack - currentAmount;
                    int amountToAdd = Mathf.Min(spaceLeft, remanining);

                    slot.SetItem(itemToAdd, currentAmount + amountToAdd);
                    remanining -= amountToAdd;

                    if (remanining <= 0)
                        return;
                }
            }
        }

        foreach (Slot slot in allSlots)
        {
            if (!slot.HasItem())
            {
                int amountToPlace = Mathf.Min(itemToAdd.maxStackSize, remanining);
                slot.SetItem(itemToAdd, amountToPlace);
                remanining -= amountToPlace;

                if (remanining <= 0)
                    return;
            }
        }


        if (remanining > 0)
        {
            Debug.Log("Inventory Is full, could not add " + remanining + " of " + itemToAdd.itemName);
        }


    }
    private void StartDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Slot hovered = GetHoveredSlot();

            if (hovered != null && hovered.HasItem())
            {
                draggedSlot = hovered;
                isDragging = true;

                dragIcon.sprite = hovered.GetItem().icon;
                dragIcon.color = new Color(1, 1, 1, 0.5f);
                dragIcon.enabled = true;

                if (StarterAssets.AudioManager.instance != null)
                {
                    StarterAssets.AudioManager.instance.Play("Item_Surukleme", 0.3f);
                }
            }
        }
    }
    private void EndDrag()
    {
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Slot hovered = GetHoveredSlot();

            if (hovered != null)
            {
                HandleDrop(draggedSlot, hovered);

                if (StarterAssets.AudioManager.instance != null)
                {
                    StarterAssets.AudioManager.instance.Play("Item_Surukleme", 0.5f);
                }
            }

            // Oyuncu eþyayý slota veya boþa býraktýðýnda farenin ucundan temizle
            dragIcon.enabled = false;
            draggedSlot = null;
            isDragging = false;
        }
    }



    private Slot GetHoveredSlot()
    {
        foreach (Slot s in allSlots)
        {
            if (s.hovering)
                return s;
        }

        return null;
    }
    private void HandleDrop(Slot from, Slot to)
    {
        if (from == to) return;

        if (to.HasItem() && to.GetItem() == from.GetItem())
        {
            int max = to.GetItem().maxStackSize;
            int space = max - to.GetAmount();

            if (space > 0)
            {
                int move = Mathf.Min(space, from.GetAmount());

                to.SetItem(to.GetItem(), to.GetAmount() + move);
                from.SetItem(from.GetItem(), from.GetAmount() - move);

                if (from.GetAmount() <= 0)
                    from.ClearSlot();

                return;
            }
        }

        if (to.HasItem())
        {
            ItemSO tempItem = to.GetItem();
            int tempAmount = to.GetAmount();

            to.SetItem(from.GetItem(), from.GetAmount());
            from.SetItem(tempItem, tempAmount);
            return;
        }

        to.SetItem(from.GetItem(), from.GetAmount());
        from.ClearSlot();
    }
    private void UpdateDragItemPosition()
    {
        if (isDragging)
        {
            dragIcon.transform.position = Input.mousePosition;
        }
    }

    private void Pickup()
    {
        if (lookedAtRenderers.Count > 0 && Input.GetKeyDown(SettingsManager.Instance.GetKey("Interaksiyon")))
        {
            Item item = lookedAtRenderers[0].GetComponentInParent<Item>();
            if (item != null)
            {
                AddItem(item.item, item.amount);

                if (StarterAssets.AudioManager.instance != null)
                {
                    StarterAssets.AudioManager.instance.Play("Item_Alma");
                }

                Destroy(item.gameObject);
                lookedAtRenderers.Clear();
                originalMaterials.Clear();
            }
        }
    }

    private void DetectLookedAtItem()
    {
        // Önceki highlight'ý temizle
        for (int i = 0; i < lookedAtRenderers.Count; i++)
            lookedAtRenderers[i].materials = originalMaterials[i];
        lookedAtRenderers.Clear();
        originalMaterials.Clear();

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            Item item = hit.collider.GetComponent<Item>();
            if (item == null)
                item = hit.collider.GetComponentInParent<Item>();

            if (item != null)
            {
                // Hem objenin hem children'larýn tüm renderer'larýný al
                Renderer[] rends = item.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer rend in rends)
                {
                    originalMaterials.Add(rend.materials);
                    Material[] highlighted = new Material[rend.materials.Length];
                    for (int i = 0; i < highlighted.Length; i++)
                        highlighted[i] = highlightMaterial;
                    rend.materials = highlighted;
                    lookedAtRenderers.Add(rend);
                }
            }
        }
    }
    private void UpdateHotbarOpacity()
    {
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            Image icon = hotbarSlots[i].GetComponent<Image>();
            if (icon != null)
                icon.color = new Color(1, 1, 1, 1f);
        }

        if (selectionIndicator != null)
        {
            RectTransform slotRect = hotbarSlots[equippedHotbarIndex].GetComponent<RectTransform>();
            selectionIndicator.transform.SetParent(slotRect, false);
            selectionIndicator.transform.SetAsFirstSibling(); // ? deðiþen tek þey

            RectTransform indicatorRect = selectionIndicator.GetComponent<RectTransform>();
            indicatorRect.anchorMin = Vector2.zero;
            indicatorRect.anchorMax = Vector2.one;
            indicatorRect.offsetMin = Vector2.zero;
            indicatorRect.offsetMax = Vector2.zero;
        }
    }

    private void HandleHotBarSelection()
    {
        for (int i = 0; i < 6; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                equippedHotbarIndex = i;
                UpdateHotbarOpacity();
            }
        }
    }

    private void HandleDropEquippedItem()
    {
        if (!Input.GetKeyDown(KeyCode.Q)) return;

        Slot equippedSlot = hotbarSlots[equippedHotbarIndex];

        if (!equippedSlot.HasItem()) return;

        ItemSO itemSO = equippedSlot.GetItem();
        GameObject prefab = itemSO.itemPrefab;

        if (prefab == null) return;

        GameObject dropped = Instantiate(prefab, Camera.main.transform.position + Camera.main.transform.forward, Quaternion.identity);

        Item item = dropped.GetComponent<Item>();
        item.item = itemSO;
        item.amount = equippedSlot.GetAmount();

        equippedSlot.ClearSlot();
    }

    public void RemoveItem(ItemSO itemToRemove, int amount)
    {
        foreach (Slot slot in allSlots)
        {
            if (slot.HasItem() && slot.GetItem() == itemToRemove)
            {
                slot.RemoveAmount(amount);
                return;
            }
        }
    }

    public int GetItemCount(ItemSO item)
    {
        int toplam = 0;
        foreach (Slot slot in allSlots)
            if (slot.HasItem() && slot.GetItem() == item)
                toplam += slot.GetAmount();
        return toplam;
    }
}

