using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FirearmDetails_SO", menuName = "Data/Weapon/Firearm")]
public class FirearmDetails_SO : ScriptableObject
{
    [SerializeField]
    private  List<FirearmDetails> firearmDetailsList = new List<FirearmDetails>();
    public IReadOnlyList<FirearmDetails> FirearmDetailsList => firearmDetailsList;

}
