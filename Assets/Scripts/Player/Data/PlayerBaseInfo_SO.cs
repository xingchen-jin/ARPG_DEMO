using UnityEngine;

//玩家基础数据
[CreateAssetMenu(fileName = "PlayerBaseInfo_SO", menuName = "Player/Info")]
public class PlayerBaseInfo_SO : ScriptableObject
{
    public float maxHealth;
    public float currentHealth;
    public float armorValue;//护甲值
    public float defence;

}