using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPC_Database_SO", menuName = "NPC/Database_SO")]
public class NPC_Database_SO : ScriptableObject
{
    public List<EnemyData> enemies;
    
}