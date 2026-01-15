using UnityEngine;
[CreateAssetMenu(menuName = "Effect/Damage")]
public class DamageEffectSO : CardEffectSO
{
    public int damage;

    public override void ActivatedEffect(Entity caster, Entity target)
    {
        if (target != null)
        {
            bool isDead = target.TakeDamage(damage);
        }
    }
}



