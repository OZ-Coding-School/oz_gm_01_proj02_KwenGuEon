using UnityEngine;
[System.Serializable]
public class Item
{
    public enum CardClass
    {
        Normal, //회색 보석
        Rare,   //파란색 보석
        Hero,   //보라색 보석
        Legend  //주황색 보석
    }
    public string name;     //이름
    public int attack;      //공격력
    public int health;      //체력
    public Sprite sprite;   //카드 이미지
    public Sprite backGround;
    public float percent;   //카드가 뽑힐 확률
    public int cardCost;
    [TextArea]
    public string cardInfo; //카드 능력 설명

    [Range(0, 100)]
    public int shopPercent;
}

[CreateAssetMenu(fileName = "ItemSO", menuName = "SeriptableObject/ItemSO")]
public class ItemSO : ScriptableObject
{
    public Item[] items;
}
