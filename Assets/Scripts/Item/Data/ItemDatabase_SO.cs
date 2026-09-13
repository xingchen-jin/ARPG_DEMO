using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase_SO", menuName = "Data/ItemDatabase")]
public class ItemDatabase_SO : ScriptableObject
{
    [SerializeField]
    private List<ItemBase> itemDetailsList = new List<ItemBase>();
    public IReadOnlyList<ItemBase> ItemDetailsList => itemDetailsList;
}