using UnityEngine;

[CreateAssetMenu(menuName = "Effect/Heal")]
public class HealEffectSO : CardEffectSO
{
    public int heal;

    public override void ActivatedEffect(Entity caster, Entity target)
    {
        if (target != null)
        {
            target.Heal(heal);
        }
    }
}
