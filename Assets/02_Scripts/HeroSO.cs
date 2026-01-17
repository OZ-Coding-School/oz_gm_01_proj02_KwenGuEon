using UnityEngine;

[CreateAssetMenu(fileName = "HeroData", menuName = "ScriptableObject/HeroData")]
public class HeroSO : ScriptableObject
{

    [Header("¿µ¿õ Á¤º¸")]
    public string heroName;
    public Sprite heroSprite;

    [Header("¿µ¿õ ´É·Â")]
    public Sprite heroAbilitySprite;
    public Item heroAbility;

    public int health = 30;
}
