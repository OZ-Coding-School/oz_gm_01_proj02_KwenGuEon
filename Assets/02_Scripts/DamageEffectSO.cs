using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Effect/Damage")]
public class DamageEffectSO : CardEffectSO
{
    public int damage;

    public override void ActivatedEffect(Entity caster, Entity target)
    {
        if(target != null)
        {
            bool isDead = target.TakeDamage(damage);
        }
    }
}

[CreateAssetMenu(menuName = "Effect/Heal")]
public class HealEffectSO : CardEffectSO
{
    public int heal;

    public override void ActivatedEffect(Entity caster, Entity target)
    {
        if(target != null)
        {
            target.heath += heal;
        }
    }
}
[CreateAssetMenu(menuName = "Effect/Random Damage")]
public class RandomDamageEffect : CardEffectSO
{
    public int damage;

    public override void ActivatedEffect(Entity caster, Entity target)
    {

    }
}
