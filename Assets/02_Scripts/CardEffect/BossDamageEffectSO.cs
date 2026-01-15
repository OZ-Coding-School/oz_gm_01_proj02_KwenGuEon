public class BossDamageEffectSO : CardEffectSO
{
    public int damage;
    public override void ActivatedEffect(Entity caster, Entity target)
    {
        Entity enemyBoss = EntityManager.Instance.FindEnemyBoss(caster.isMine);

        if (enemyBoss != null)
        {

        }
    }
}
