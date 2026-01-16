using UnityEngine;

[CreateAssetMenu(menuName = "Effect/Random Damage")]
public class RandomDamageEffectSO : CardEffectSO
{
    public int damage;
    public int count;

    public override void ActivatedEffect(Entity caster, Entity target)
    {
        bool targetIsMine = !caster.isMine;

        for (int i = 0; i < count; i++)
        {
            //Entity randomEnemy = EntityManager.Instance.FindRandomEntity(targetIsMine);
            //
            //if (randomEnemy != null)
            //{
            //    randomEnemy.TakeDamage(damage);
            //}
        }
    }
}
