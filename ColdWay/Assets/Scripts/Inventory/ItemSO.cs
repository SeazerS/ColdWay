using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Item", menuName ="NewItem")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public Sprite iconFull;
    public int maxStackSize;
    public GameObject itemPrefab;
    public GameObject hangItemPrefab;
}
