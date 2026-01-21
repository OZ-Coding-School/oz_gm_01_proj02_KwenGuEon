using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class EffectManager : MonoBehaviour
{
    [SerializeField] private EntityManager entityManager;  

    private void Awake()
    {
        if (entityManager == null)
            entityManager = EntityManager.Instance;
    }

    public void ApplyActualEffect(EffectDef define, Entity caster, Entity target)
    {
        if(target == null) return;

        switch (define.effectType)
        {
            case (EffectType.Damage):
                target.TakeDamage(define.defaultValue);
                break;
            case (EffectType.Heal):
                target.Heal(define.defaultValue);
                break;
            case EffectType.BuffStats:                
                if (define.addAttack != 0)
                {
                    if (define.isTempThisTurn)
                        target.TempAttackThisTurn(define.addAttack);
                    else
                        target.AttackUP(define.addAttack);
                }
                if (define.isTempThisTurn)
                {
                    int rollbackValue = define.addAttack;
                    TurnManager.Instance.RegisterEndTurnRollback(caster.isMine, () =>
                    {
                        if (target == null) return;
                        target.SetAttack(target.attack - rollbackValue); // 또는 AttackUP(-rollbackValue)
                    });
                }
                if (define.addHealth != 0)
                {
                    if (define.isGrantHealth)
                        target.GrantHealth(define.addHealth);
                    else
                        target.Heal(define.addHealth);
                }
                break;
            case EffectType.Kill:                
                if (!target.isBossOrEmpty)
                    target.isDead = true;
                break;
            case EffectType.SetStat:               
                if (define.isSetAttack)
                    target.SetAttack(define.defaultValue);
                else
                    target.SetHealth(define.defaultValue);
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
                entityManager.TryStealMinion(target, caster.isMine);
                bool ok = entityManager.TryStealMinion(target, caster.isMine);                
                break;

            case EffectType.StatusAbnormality:
                if (define.StatusAbnormalityId == 3)
                {                    
                    target.SetAttack(1);
                    target.SetHealth(1);
                }
                // 1=기절, 2=빙결은 “행동 불가”를 어디에 둘지 정책이 필요해서
                // 지금 구조에선 우선 패스하거나, e.attackAble=false 로만 처리해도 됨
                else if (define.StatusAbnormalityId == 1 || define.StatusAbnormalityId == 2)
                {                    
                    target.cantActTurns = Mathf.Max(target.cantActTurns, 1);
                    target.attackAble = false; // 최소 동작
                    target.TurnOnOffOutLine(false);
                }
                break;
        }
    }
    void ApplyEffect(Item item, EffectDef define, Entity caster, Entity target)
    {
        if(!string.IsNullOrEmpty(item.hitSFX))
        {
            SoundManager.instance.PlayOnSFX(item.hitSFX);
        }

        if(item.VFXPrefab != null)
        {
            SpawnVFX(item.VFXPrefab, target.transform.position);
        }

        ApplyActualEffect(define, caster, target);
    }
    public bool RunAbilities(Item item, Entity caster, Entity target)
    {
        if (item == null || item.abilities == null || item.abilities.Count == 0) return false;        

        bool requiresExternalTarget = false;

        if (item.needTarget && item.abilities != null)
        {
            foreach (var a in item.abilities)
            {
                var rule = a.targetRule;

                if (rule.targetGroup == TargetGroup.Target)
                {
                    requiresExternalTarget = true;
                    break;
                }
            }
        }

        if (requiresExternalTarget && target == null) return false;

        foreach (var define in item.abilities)
        {
            List<Entity> entities = GetOneTarget(define.targetRule, caster, target);
            if (entities == null || entities.Count == 0) continue;

            foreach (var entity in entities)
            {
                if (entity == null) continue;

                bool isUseProjectile = IsProjectileType(define, caster, target);

                if(isUseProjectile)
                {   
                    var projectileObj = Instantiate(item.projectilePrefab);
                    var projectileLogic = projectileObj.GetComponent<AbilityProjectile>();

                    projectileLogic.Setup(caster.transform.position, entity, () =>
                    {
                        ApplyEffect(item, define, caster, entity);
                    });
                }
                else
                {            
                    ApplyEffect(item, define, caster, entity);
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
        var boss = entityManager.FindEnemyBoss(caster.isMine);

        switch (rule.targetGroup)
        {
            case TargetGroup.Target:
                if (target != null) result.Add(target);
                break;
            case TargetGroup.EnemyHero:

                if (boss != null) result.Add(boss);
                break;
            case TargetGroup.RandomEnemy:
                int times = Mathf.Max(1, rule.count);
                for (int i = 0; i < times; i++)
                {
                    var r = entityManager.FindRandomEntity(isTargetIsMine: !caster.isMine, isIncludeBoss: true, isOnlyMinion: false);
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
                result.AddRange(entityManager.GetAliveMinions(!caster.isMine));
                if (boss != null && !boss.isDead) result.Add(boss);
                break;

            case TargetGroup.OnlyEnemyMinions:
                result.AddRange(entityManager.GetAliveMinions(!caster.isMine));
                break;

            case TargetGroup.AllMinions:
                result.AddRange(entityManager.GetAliveMinions(true));
                result.AddRange(entityManager.GetAliveMinions(false));
                if(caster != null)
                    result.Remove(caster);
                break;
            default:
                return null;
        }      

        return result;
    }
    private bool IsProjectileType(EffectDef define, Entity caster, Entity target)
    {
        if (define.effectType != EffectType.Damage &&
            define.effectType != EffectType.StatusAbnormality) return false;

        if (define.targetRule.targetGroup == TargetGroup.EnemyAll ||
            define.targetRule.targetGroup == TargetGroup.AllMinions ||
            define.targetRule.targetGroup == TargetGroup.RandomEnemy)
        {            
            if (define.targetRule.targetGroup == TargetGroup.EnemyAll) return false;
            if (define.targetRule.targetGroup == TargetGroup.AllMinions) return false;
        }
        
        return true;
    }
    
    void SpawnVFX(GameObject vfxPrefab, Vector3 pos)
    {
        if (vfxPrefab == null) return;

        GameObject vfxObj = Instantiate(vfxPrefab, pos, Utils.QI);

        var particles = vfxObj.GetComponent<ParticleSystem>();
        if(particles != null) particles.Play();

        Destroy(vfxObj, 2.0f);

    }
}
