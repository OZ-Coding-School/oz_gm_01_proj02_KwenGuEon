using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField] private EntityManager entityManager;

    [SerializeField] Camera mainCamera;
    [SerializeField] Camera vfxCamera;

    private int debuffCount = 2;

    private void Awake()
    {
        if (entityManager == null)
            entityManager = EntityManager.Instance;
        if(mainCamera == null)
            mainCamera = Camera.main;
    }

    public void ApplyActualEffect(EffectDef define, Entity caster, Entity target)
    {
        if (target == null) return;

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
        if (!string.IsNullOrEmpty(item.hitSFX))
        {
            SoundManager.instance.PlayOnSFX(item.hitSFX);
        }

        if (item.VFXPrefab != null)
        {
            SpawnVFX(item.VFXPrefab, target.transform.position, item.vfxScale,target);
        }
        if(item.debuffVFX != null)
        {
            if (define.effectType == EffectType.StatusAbnormality &&
               (define.StatusAbnormalityId == 1 || define.StatusAbnormalityId == 2))
            {                
                GameObject debuffObj = SpawnDebuffVFX(item.debuffVFX, target.transform.position, item.vfxScale, target);

                int duration = (define.defaultValue > 0) ? define.defaultValue : 1;

                target.SetCC(duration, debuffObj);

                TurnManager.Instance.RegisterEndTurnRollback(target.isMine, () =>
                {
                    if (target == null) return;
                    target.ConsumeCantActOnMyTurnStart();
                });

            }
            else
            {                
                SpawnVFX(item.debuffVFX, target.transform.position, item.vfxScale, target);
            }
        }

        ApplyActualEffect(define, caster, target);
    }
    public bool RunAbilities(Item item, Entity caster, Entity target)
    {
        Debug.Log($"[1] 스킬 시작: {item?.name ?? "이름없음"}");

        if (item == null || item.abilities == null || item.abilities.Count == 0)
        {
            Debug.LogError("[ERROR] 아이템 데이터가 없거나 능력(Abilities) 리스트가 비어있습니다!");
            return false;
        }

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

        if (requiresExternalTarget && target == null)
        {
            Debug.LogError("[ERROR] 타겟이 필요한 카드인데, 타겟(target)이 null입니다!");
            return false;
        }

        foreach (var define in item.abilities)
        {
            List<Entity> entities = GetOneTarget(define.targetRule, caster, target);
            if (entities == null || entities.Count == 0)
            {
                Debug.LogWarning($"[WARNING] 타겟을 찾지 못했습니다. Rule: {define.targetRule.targetGroup}");
                continue;
            }

            foreach (var entity in entities)
            {
                if (entity == null) continue;

                bool isUseProjectile = IsProjectileType(define, caster, target) && item.projectilePrefab != null;

                if (isUseProjectile)
                {
                    Transform canvasTr = GameObject.Find("Canvas").transform;

                    var projectileObj = Instantiate(item.projectilePrefab, canvasTr, false);
                    var projectileLogic = projectileObj.GetComponent<AbilityProjectile>();

                    if (projectileLogic == null)
                    {
                        Debug.LogError($"[ERROR] 프리팹({item.projectilePrefab.name})에 'AbilityProjectile' 스크립트가 없습니다!");
                        Destroy(projectileObj);
                        ApplyEffect(item, define, caster, entity); // 즉시 발동으로 처리
                        entityManager.ResolveDead();
                        entityManager.CheckBossDeadCo();
                        continue;
                    }

                    projectileLogic.Setup(caster, entity, () =>
                    {
                        ApplyEffect(item, define, caster, entity);
                        entityManager.ResolveDead();
                        entityManager.CheckBossDeadCo();
                    });
                }
                else
                {
                    ApplyEffect(item, define, caster, entity);
                    entityManager.ResolveDead();
                    entityManager.CheckBossDeadCo();
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
                if (caster != null)
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

    public void SpawnVFX(GameObject vfxPrefab, Vector3 pos, float scale, Entity target = null)
    {
        Debug.Log($"[SpawnVFX 호출됨] 프리팹 이름: {vfxPrefab?.name ?? "NULL"}");
        
        if (vfxPrefab == null)
        {
            Debug.LogError(" [오류] VFX 프리팹이 비어있습니다(NULL)! 데이터(ItemSO)를 확인하세요.");
            return;
        }

        Vector3 finalSpawnVFXPos = Vector3.zero;

        bool isTargetUI = (target != null) && (target.GetComponent<RectTransform>() != null);

        if (isTargetUI)
        {
            float ratioX = pos.x / Screen.width;
            float ratioY = pos.y / Screen.height;

            finalSpawnVFXPos = vfxCamera.ViewportToWorldPoint(new Vector3(ratioX, ratioY, 10f));
        }
        else
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);

            finalSpawnVFXPos = vfxCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        }       

        GameObject vfxObj = Instantiate(vfxPrefab, finalSpawnVFXPos, Utils.QI);

        if(scale > 0)
        {
            vfxObj.transform.localScale = Vector3.one * scale;
        }

        SetLayerVFX(vfxObj, LayerMask.NameToLayer("UI_VFX"));

        var particles = vfxObj.GetComponent<ParticleSystem>();
        if (particles != null) particles.Play();

        Destroy(vfxObj, 1.0f);

    }
    void SetLayerVFX(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach(Transform child in obj.transform)
        {
            SetLayerVFX(child.gameObject, newLayer);
        }
    }
    public GameObject SpawnDebuffVFX(GameObject vfxPrefab, Vector3 pos, float scale, Entity target = null)
    {       
        if (vfxPrefab == null)
        {            
            return null;
        }

        Vector3 finalSpawnVFXPos = Vector3.zero;

        bool isTargetUI = (target != null) && (target.GetComponent<RectTransform>() != null);

        if (isTargetUI)
        {
            float ratioX = pos.x / Screen.width;
            float ratioY = pos.y / Screen.height;

            finalSpawnVFXPos = vfxCamera.ViewportToWorldPoint(new Vector3(ratioX, ratioY, 10f));
        }
        else
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);

            finalSpawnVFXPos = vfxCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
        }       

        GameObject vfxObj = Instantiate(vfxPrefab, finalSpawnVFXPos, Utils.QI);

        SetLayerVFX(vfxObj, LayerMask.NameToLayer("UI_VFX"));

        var particles = vfxObj.GetComponent<ParticleSystem>();
        if (particles != null) particles.Play();

        if (target != null)
        {
            VFXFollower follower = vfxObj.GetComponent<VFXFollower>();
            if (follower == null) follower = vfxObj.AddComponent<VFXFollower>();
            follower.Setup(target.transform, mainCamera, vfxCamera);
        }

        return vfxObj;

    }    
}
