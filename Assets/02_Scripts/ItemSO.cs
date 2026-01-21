using UnityEngine;
using System.Linq;
using JetBrains.Annotations;
using System.Collections.Generic;
[System.Serializable]
public enum EffectType
{
    Damage,
    Heal,
    BuffStats,
    Kill,
    Draw,
    Mana,
    StatusAbnormality, //상태이상
    SetStat,
    MoveCard,
    MoveMinion
}
public enum TargetGroup
{
    Target,
    RandomEnemy,
    EnemyHero,
    Friendly,
    EnemyAll,
    OnlyEnemyMinions,
    AllMinions
}
[System.Serializable]
public struct TargetRule
{
    public TargetGroup targetGroup;

    public bool isAffectAll;
    public int count;

    public bool isOnlyMinion;
    public bool isOnlyDamage;    
}
[System.Serializable]
public struct EffectDef
{
    public EffectType effectType;
    public TargetRule targetRule;

    public int defaultValue;
    public int secondaryValue;

    public int addAttack;
    public int addHealth;
    public bool isGrantHealth;

    public bool isJustThisTurnMana;

    public int StatusAbnormalityId;

    public bool isSetAttack;

    public bool isAffectHand;
    public bool isAffectDeck;
    public bool isMoveDiscard;

    public bool isMoveToMyField;
    public int needEnemyMinionCount;

    public bool isTempThisTurn;
}

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
    public int cardID;
    public enum Cardtype { Minion, Spell }

    public int cardCost;
    public Cardtype cardType;
    public CardClass cardClass;
    public string name;     //이름
    public int attack;      //공격력
    public int health;      //체력
    public Sprite sprite;   //카드 이미지
    public Sprite backGround;
    public float percent;   //카드가 뽑힐 확률    
    [TextArea]
    public string cardInfo; //카드 능력 설명
    

    [Range(0, 100)]
    public int shopPercent;

    [Header("카드 능력")]
    public bool isRush;         //돌진
    public bool isProvocation;  //도발
    public bool isBattleCry;    //전투의함성
    public bool isSpell;        //마법카드
    public bool needTarget;
    public List<EffectDef> abilities;
    public CardEffectSO activeEffect;

    [Header("능력 SFX, VFX")]
    public GameObject projectilePrefab;
    public GameObject VFXPrefab;
    public GameObject debuffVFX;
    public string hitSFX;
}

[CreateAssetMenu(fileName = "ItemSO", menuName = "SeriptableObject/ItemSO")]
public class ItemSO : ScriptableObject
{
    public Item[] items;

    public Item GetCardID(int id)
    {
        return System.Array.Find(items, item => item.cardID == id);
    }
}
