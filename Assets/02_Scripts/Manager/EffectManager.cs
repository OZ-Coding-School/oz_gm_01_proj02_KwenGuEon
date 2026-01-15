using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField] private EntityManager entityManager;

    private void Awake()
    {
        if (entityManager == null)
            entityManager = EntityManager.Instance;
    }
    public bool RunAbilities(Item item, Entity caster, Entity target)
    {
        if (item == null || item.abilities == null || item.abilities.Count == 0) return false;
        if (item.needTarget && target == null) return false;

        foreach (var define in item.abilities)
        {
            List<Entity> entities = GetOneTarget(define.targetRule, caster, target);

            if (entities == null || entities.Count == 0) continue;

            foreach (var entity in entities)
            {
                if (entity == null) continue;

                switch (define.effectType)
                {
                    case (EffectType.Damage):
                        entity.TakeDamage(define.defaultValue);
                        break;
                    case (EffectType.Heal):
                        entity.Heal(define.defaultValue);
                        break;
                    case EffectType.BuffStats:
                        if (define.addAttack != 0)
                        {
                            if (define.isTempThisTurn)
                                entity.TempAttackThisTurn(define.addAttack);
                            else
                                entity.AttackUP(define.addAttack);
                        }
                        if (define.isTempThisTurn)
                        {
                            int rollbackValue = define.addAttack;
                            TurnManager.Instance.RegisterEndTurnRollback(caster.isMine, () =>
                            {
                                if (entity == null) return;
                                entity.SetAttack(entity.attack - rollbackValue); // 또는 AttackUP(-rollbackValue)
                            });
                        }
                        if (define.addHealth != 0)
                        {
                            if (define.isGrantHealth)
                                entity.GrantHealth(define.addHealth);
                            else
                                entity.Heal(define.addHealth);
                        }
                        break;
                    case EffectType.Kill:
                        if (!entity.isBossOrEmpty)
                            entity.isDead = true;
                        break;
                    case EffectType.SetStat:
                        if (define.isSetAttack)
                            entity.SetAttack(define.defaultValue);
                        else
                            entity.SetHealth(define.defaultValue);
                        break;
                    case EffectType.Draw:
                        CardManager.instance.DrawCard(caster.isMine, define.defaultValue);
                        break;

                    case EffectType.Mana:
                        if (define.isJustThisTurnMana)
                            TurnManager.Instance.GainTempMana(caster.isMine, define.defaultValue);
                        else
                            TurnManager.Instance.GainEmptyMana(caster.isMine, define.defaultValue);
                        break;

                    case EffectType.MoveCard:
                        if (define.isMoveDiscard)
                        {
                            if (define.isAffectHand) CardManager.instance.DiscardAllHand(caster.isMine);
                            if (define.isAffectDeck) CardManager.instance.DiscardAllDeck();
                        }
                        break;
                    case EffectType.MoveMinion:
                        if (!define.isMoveToMyField) break;
                        if (define.needEnemyMinionCount > 0)
                        {
                            int enemyMinions = entityManager.CountMinions(!caster.isMine);
                            if (enemyMinions < define.needEnemyMinionCount)
                                break;
                        }
                        entityManager.TryStealMinion(entity, caster.isMine);
                        break;

                    case EffectType.StatusAbnormality:
                        if (define.StatusAbnormalityId == 3)
                        {
                            entity.SetAttack(1);
                            entity.SetHealth(1);
                        }
                        // 1=기절, 2=빙결은 “행동 불가”를 어디에 둘지 정책이 필요해서
                        // 지금 구조에선 우선 패스하거나, e.attackAble=false 로만 처리해도 됨
                        else if (define.StatusAbnormalityId == 1 || define.StatusAbnormalityId == 2)
                        {
                            entity.cantActTurns = Mathf.Max(entity.cantActTurns, 1);
                            entity.attackAble = false; // 최소 동작
                            entity.TurnOnOffOutLine(false);
                        }
                        break;
                }
            }
        }
        entityManager.ResolveDead();
        entityManager.CheckBossDeadCo();

        return true;
    }
    private List<Entity> GetOneTarget(TargetRule rule, Entity caster, Entity target)
    {
        var result = new List<Entity>();

        switch (rule.targetGroup)
        {
            case TargetGroup.Target:
                if (target != null) result.Add(target);
                break;
            case TargetGroup.EnemyHero:
                var boss = entityManager.FindEnemyBoss(caster.isMine);
                if (boss != null) result.Add(boss);
                break;
            case TargetGroup.RandomEnemy:
                int times = Mathf.Max(1, rule.count);
                for (int i = 0; i < times; i++)
                {
                    var r = entityManager.FindRandomEntity(!caster.isMine);
                    if (r != null) result.Add(r);
                }
                break;
            case TargetGroup.Friendly:
                if (rule.isAffectAll)
                    result.AddRange(entityManager.GetEntities(caster.isMine));
                else
                    result.Add(caster);
                break;
            case TargetGroup.EnemyAll:
                result.AddRange(entityManager.GetEntities(!caster.isMine));
                result.Add(entityManager.FindEnemyBoss(caster.isMine));
                break;

            case TargetGroup.OnlyEnemyMinions:
                result.AddRange(entityManager.GetEntities(!caster.isMine));
                break;

            case TargetGroup.AllMinions:
                result.AddRange(entityManager.GetEntities(true));
                result.AddRange(entityManager.GetEntities(false));
                break;
            default:
                return null;
        }

        result.RemoveAll(e =>
           e == null ||
           ((rule.targetGroup == TargetGroup.OnlyEnemyMinions ||
           rule.targetGroup == TargetGroup.AllMinions ||
           rule.isOnlyMinion) && e.isBossOrEmpty) ||
           (rule.isOnlyDamage && e.health >= e.maxHealth));

        return result;
    }
}
