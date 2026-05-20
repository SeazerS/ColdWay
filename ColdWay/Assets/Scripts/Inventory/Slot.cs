using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool hovering;

    private ItemSO holdItem;
    private int itemAmount;

    private Image iconImage;
    private TextMeshProUGUI amountText;

    private void Awake()
    {
        iconImage = transform.GetChild(0).GetComponent<Image>();
        amountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        
    }
    public ItemSO GetItem()
    {
        return holdItem;
    }

    public int GetAmount()
    {
        return itemAmount;
    }
    
    public void SetItem(ItemSO item, int amount=1)
    {
        holdItem = item;
        itemAmount = amount;

        UpdateSlot();
    }

    public void UpdateSlot()
    {
        if (iconImage==null)
        {
            iconImage = transform.GetChild(0).GetComponent<Image>();
            amountText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        }

        if (holdItem != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = holdItem.icon;
            //amountText.text = itemAmount.ToString();
            amountText.text = holdItem.maxStackSize > 1 ?
                          itemAmount.ToString() : "";
        }
        else
        {
            iconImage.enabled = false;
            amountText.text = "";
        }
    }
    public int AddAmount(int amountToAdd)
    {
        itemAmount += amountToAdd;
        UpdateSlot();
        return itemAmount;

    }
    public int RemoveAmount(int amountToRemove)
    {
        itemAmount -= amountToRemove;
        if (itemAmount<=0)
        {
            ClearSlot();
        }
        else
        {
            UpdateSlot();
        }
        return itemAmount;
    }
    public void ClearSlot()
    {
        holdItem = null;
        itemAmount = 0;
        UpdateSlot();
    }
    public bool HasItem()
    {
        return holdItem != null;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
}
