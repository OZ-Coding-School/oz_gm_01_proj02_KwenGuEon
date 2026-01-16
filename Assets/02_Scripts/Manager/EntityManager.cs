using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class EntityManager : MonoBehaviour
{
    public static EntityManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField] private EffectManager effectManager;

    [SerializeField] GameObject entityPrefab;
    [SerializeField] GameObject damagePrefab;
    [SerializeField] List<Entity> myEntities;
    [SerializeField] List<Entity> otherEntities;

    public List<Entity> GetEntities(bool isMine) => isMine ? new List<Entity>(myEntities) : new List<Entity>(otherEntities);
    public Entity GetBoss(bool isMine) => isMine ? myBossEntity : otherBossEntity;
    
    [SerializeField] Entity myEmptyEntity;    
    [SerializeField] Entity myBossEntity;
    [SerializeField] Entity otherBossEntity;

    [SerializeField] GameObject targetPicker;
    [SerializeField] Transform damageCanvas;

    [Range(0f, 5f)][SerializeField] float entitySpacing = 2.3f;
    WaitForSeconds delay1Sc = new WaitForSeconds(1.0f);
    WaitForSeconds delay2Sc = new WaitForSeconds(2.0f);
    bool isCheckBossDead;

    const int MAX_ENTITY_COUNT = 7;
    public bool isFullMyEntities => myEntities.Count >= MAX_ENTITY_COUNT && !ExistMyEmptyEntity;
    bool isFullOtherEntities => otherEntities.Count >= MAX_ENTITY_COUNT;
    bool ExistMyEmptyEntity => myEntities.Exists(x => x == myEmptyEntity);    
    int myEmptyEntityIndex => myEntities.FindIndex(x => x == myEmptyEntity);
    bool canMouseInput => TurnManager.Instance.isMyTurn && !TurnManager.Instance.isLoading;
    bool existTargetPickEntity => targetPickEntity != null;

    Entity selectEntity;
    Entity targetPickEntity;

    [SerializeField] Vector2 damageOffset = new Vector2(0, 50);

    private void Start()
    {
        TurnManager.Instance.UnsubscribeOnTurnStarted(OnTurnStarted);
        TurnManager.Instance.SubscribeOnTurnStarted(OnTurnStarted);
    }
    private void OnDestroy()
    {
        TurnManager.Instance.UnsubscribeOnTurnStarted(OnTurnStarted);
    }
    private void Update()
    {
        ShowTargetPicker(existTargetPickEntity);
    }
    #region Minions List
    public List<Entity> GetAliveMinions(bool isMine)
    {
        var src = isMine ? myEntities : otherEntities;
        var result = new List<Entity>(src.Count);

        foreach(var ent in src)
        {
            if (ent == null) continue;
            if (ent == myEmptyEntity) continue;
            if (ent.isBossOrEmpty) continue;   // empty 포함
            if (ent.isDead) continue;
            result.Add(ent);
        }
        return result;
    }
    public List<Entity> GetAliveTargetCandidates(bool isTargetIsMine, bool includeBoss, bool onlyMinion)
    {
        var candidates = GetAliveMinions(isTargetIsMine);

        if(includeBoss && !onlyMinion)
        {
            var boss = isTargetIsMine ? myBossEntity : otherBossEntity;
            if(boss != null && !boss.isDead) candidates.Add(boss);
        }

        return candidates;
    }
    public List<Entity> GetAliveTargetCandidatesAll(bool includeBoss, bool onlyMinion)
    {
        var result = new List<Entity>();

        result.AddRange(GetAliveMinions(true));
        result.AddRange(GetAliveMinions(false));

        if (includeBoss && !onlyMinion)
        {
            if (myBossEntity != null && !myBossEntity.isDead) result.Add(myBossEntity);
            if (otherBossEntity != null && !otherBossEntity.isDead) result.Add(otherBossEntity);
        }

        return result;
    }
    #endregion
    //타겟이 누군지 보여주는 스프라이트의 로직
    void ShowTargetPicker(bool isShow)
    {
        targetPicker.SetActive(isShow);
        if (existTargetPickEntity)
        {
            Vector3 finalPos = Vector3.zero;

            bool isUITarget = targetPickEntity.GetComponent<RectTransform>() != null;

            if (isUITarget)
            {
                finalPos = targetPickEntity.transform.position;
            }
            else
            {
                finalPos = Camera.main.WorldToScreenPoint(targetPickEntity.transform.position);
            }
            targetPicker.transform.position = finalPos;
        }
    }
    void Attack(Entity attacker, Entity defender)
    {
        attacker.attackAble = false;
        attacker.TurnOnOffOutLine(false);

        var sortingGroup = attacker.GetComponent<SortingGroup>();
        int originOrder = sortingGroup.sortingOrder;
        sortingGroup.sortingOrder = 1000;

        Vector3 dir = (defender.originPos - attacker.originPos).normalized;
        Vector3 hitPos = defender.originPos - (dir * 1.5f);

        Sequence sequence = DOTween.Sequence()
            .Append(attacker.transform.DOMove(hitPos, 0.25f)).SetEase(Ease.InBack)
            .AppendCallback(() =>
            {
                attacker.TakeDamage(defender.attack);
                defender.TakeDamage(attacker.attack);
                SpawnDamage(defender.attack, attacker);
                SpawnDamage(attacker.attack, defender);

                defender.transform.DOShakePosition(0.2f, 0.5f, 20);
            })
            .AppendInterval(0.05f)
            .Append(attacker.transform.DOMove(attacker.originPos, 0.3f)).SetEase(Ease.OutCirc)
            .OnComplete(() =>
            {
                sortingGroup.sortingOrder = originOrder;
                AttackCallback(attacker, defender);
            });
    }
    void AttackCallback(params Entity[] entities)
    {
        foreach (var entity in entities)
        {
            if (!entity.isDead || entity.isBossOrEmpty) continue; // 나중에 보스도 파괴시키는 장면 있어야하면 보스는 빼기 

            DeadEntity(entity);
        }
        CheckBossDeadCo();
    }
    private void DeadEntity(Entity entity)
    {
        if (entity == null) return;
        if (!entity.isDead || entity.isBossOrEmpty) return;

        if (entity.isMine) myEntities.Remove(entity);
        else otherEntities.Remove(entity);

        Sequence sequence = DOTween.Sequence()
                .Append(entity.transform.DOShakePosition(0.5f, 0.5f, 30))
                .Append(entity.transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.OutCirc))
                .OnComplete(() =>
                {
                    EntityAlignment(entity.isMine);
                    Destroy(entity.gameObject);
                });
    }
    public void ResolveDead()
    {
        var myDead = myEntities.FindAll(e => e != null && e.isDead && !e.isBossOrEmpty);
        var otherDead = otherEntities.FindAll(e => e != null && e.isDead && !e.isBossOrEmpty);

        foreach (var e in myDead) DeadEntity(e);
        foreach (var e in otherDead) DeadEntity(e);        
    }
    void SpawnDamage(int damage, Entity targetEntity)
    {
        if (damage <= 0) return;

        var damageComponent = Instantiate(damagePrefab, damageCanvas).GetComponent<Damage>();

        Vector3 screenPos = Vector3.zero;

        if (targetEntity.GetComponent<RectTransform>() != null)
        {
            screenPos = targetEntity.transform.position;
        }
        else
        {
            screenPos = Camera.main.WorldToScreenPoint(targetEntity.originPos);
        }

        screenPos += (Vector3)damageOffset;
        screenPos.z = 0;

        damageComponent.transform.position = screenPos;
        damageComponent.Damaged(damage);
    }
    void OnTurnStarted(bool myTurn)
    {
        AttackableReset(myTurn);

        if (!myTurn)
            StartCoroutine(AICo());
    }
    IEnumerator AICo()
    {
        yield return new WaitUntil(() => !TurnManager.Instance.isLoading);
        yield return delay1Sc;
        while (CardManager.instance.TryPutCard(false))
        {
            yield return delay1Sc;
        }

        //공격로직
        var attackers = new List<Entity>(otherEntities.FindAll(x => x.attackAble == true));

        for (int i = 0; i < attackers.Count; i++)
        {
            int rand = Random.Range(i, attackers.Count);
            Entity temp = attackers[i];
            attackers[i] = attackers[rand];
            attackers[rand] = temp;
        }

        foreach (var attacker in attackers)
        {
            var defenders = new List<Entity>(myEntities);
            defenders.Add(myBossEntity);

            var provocationDefenders = myEntities.FindAll(x => x.isProvocation);

            if (provocationDefenders.Count > 0)
            {
                defenders = provocationDefenders;
            }

            int rand = Random.Range(0, defenders.Count);
            Attack(attacker, defenders[rand]);

            if (TurnManager.Instance.isLoading)
                yield break;

            yield return delay2Sc;
        }
        TurnManager.Instance.EndTurn();
    }
    IEnumerator CheckBossDead()
    {
        isCheckBossDead = true;
        yield return delay2Sc;

        if (myBossEntity.isDead)
            TurnManager.Instance.TriggerOnGameResult(false);

        if (otherBossEntity.isDead)
            TurnManager.Instance.TriggerOnGameResult(true);

        isCheckBossDead = false;
    }
    public void CheckBossDeadCo()
    {
        if (isCheckBossDead) return;        
        StartCoroutine(CheckBossDead());
    }
    //디버깅용
    public void DamageBoss(bool isMine, int damage)
    {
        var targetBpssEntity = isMine ? myBossEntity : otherBossEntity;
        targetBpssEntity.TakeDamage(damage);
        CheckBossDeadCo();
    }

    public void EntityAlignment(bool isMine)
    {
        float targetY = isMine ? -1.62f : 0.59f;
        var targetEntities = isMine ? myEntities : otherEntities;

        for (int i = 0; i < targetEntities.Count; i++)
        {
            float targetX = (targetEntities.Count - 1) * -(entitySpacing / 2) + i * entitySpacing;

            var targetEntity = targetEntities[i];
            targetEntity.originPos = new Vector3(targetX, targetY, 0);
            targetEntity.MoveTransform(targetEntity.originPos, true, 0.5f);
        }
    }
    public void InsertMyEmptyEntity(float xPos)
    {
        if (isFullMyEntities) return;

        if (!ExistMyEmptyEntity)
            myEntities.Add(myEmptyEntity);

        Vector3 emptyEntitiesPos = myEmptyEntity.transform.position;
        emptyEntitiesPos.x = xPos;
        myEmptyEntity.transform.position = emptyEntitiesPos;

        int emptyEntityIndex = myEmptyEntityIndex;
        myEntities.Sort((entity1, entity2) => entity1.transform.position.x.CompareTo(entity2.transform.position.x));

        if (myEmptyEntityIndex != emptyEntityIndex)
            EntityAlignment(true);
    }
    public void RemoveMyEmptyEntity()
    {
        if (!ExistMyEmptyEntity) return;

        myEntities.RemoveAt(myEmptyEntityIndex);
        EntityAlignment(true);
    }
    public bool SpawnEntity(bool isMine, Item item, Vector3 spawnPos, out Entity spawned, Entity target = null)
    {
        spawned = null;

        if (isMine)
        {
            if (isFullMyEntities || !ExistMyEmptyEntity) return false;
        }
        else
        {
            if (isFullOtherEntities) return false;
        }

        if (item.isSpell)
        {
            return false;
        }

        var entityObject = Instantiate(entityPrefab, spawnPos, Utils.QI);
        var entity = entityObject.GetComponent<Entity>();

        if (isMine)
            myEntities[myEmptyEntityIndex] = entity;
        else
            otherEntities.Insert(Random.Range(0, otherEntities.Count), entity);

        entity.isMine = isMine;
        entity.Setup(item);
        EntityAlignment(isMine);       

        spawned = entity;
        return true;
    }
    public bool SpawnEntity(bool isMine, Item item, Vector3 spawnPos, Entity target = null)
    {
        return SpawnEntity(isMine, item, spawnPos, out _, target);
    }
    public bool RunSpell(bool isMIne, Item item, Entity target = null)
    {
        Entity caster = isMIne ? myBossEntity : otherBossEntity;

        return effectManager.RunAbilities(item, caster, target);
    }
    public void EntityMouseDown(Entity entity)
    {
        if (!canMouseInput)
            return;

        selectEntity = entity;
    }
    public void EntityMouseUp()
    {
        if (!canMouseInput) return;

        if (selectEntity && targetPickEntity && selectEntity.attackAble)
            Attack(selectEntity, targetPickEntity);

        selectEntity = null;
        targetPickEntity = null;
    }
    public void EntityMouseDrag()
    {
        if (!canMouseInput || selectEntity == null) return;

        bool existTauntEntity = otherEntities.Exists(x => x.isProvocation);
        bool existTarget = false;

        foreach (var hit in Physics2D.RaycastAll(Utils.MousePos, Vector3.forward))
        {
            Entity entity = hit.collider?.GetComponent<Entity>();
            if (entity != null && !entity.isMine && selectEntity.attackAble)
            {
                if (existTauntEntity && !entity.isProvocation)
                {
                    targetPickEntity = null;
                    continue;                        
                }

                targetPickEntity = entity;
                existTarget = true;
                break;
            }
        }

        if (!existTarget)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                Entity entity = result.gameObject.GetComponent<Entity>();

                if (entity != null && !entity.isMine && selectEntity.attackAble)
                {
                    if (existTauntEntity && !entity.isProvocation)
                    {
                        targetPickEntity = null;
                        continue;
                    }

                    targetPickEntity = entity;
                    existTarget = true;
                    break;
                }
            }
        }

        if (!existTarget)
            targetPickEntity = null;
    }
    public void AttackableReset(bool isMine)
    {
        var targetEntities = isMine ? myEntities : otherEntities;
        targetEntities.ForEach(x => x.attackAble = true);

        foreach(var ent in targetEntities)
        {
            if (ent == null) continue;
            if (ent.isBossOrEmpty) continue;

            if (ent.isCantAct)
            {
                ent.ConsumeCantActOnMyTurnStart(); // 턴 시작에 1 감소
                ent.attackAble = false;           // 이번 턴 행동 불가
            }
        }

        foreach (var entity in myEntities)
        {
            entity.attackAble = isMine;
            entity.TurnOnOffOutLine(entity.attackAble);

            if(isMine && entity.isCantAct)
            {                
                entity.attackAble = false;
                entity.TurnOnOffOutLine(false);
            }
        }
    }
    public bool TryStealMinion(Entity target, bool isToMine)
    {
        if(target == null) return false;
        if(target.isBossOrEmpty) return false;
        if(target.isMine == isToMine) return false;

        if(isToMine)
        {
            if (CountMinions(true) >= MAX_ENTITY_COUNT) return false;
        }
        else
        {
            if (CountMinions(false) >= MAX_ENTITY_COUNT) return false;
        }

        if(target.isMine) myEntities.Remove(target);
        else otherEntities.Remove(target);

        target.isMine = isToMine;

        if(isToMine)
        {
            myEntities.Add(target);
        }
        else
        {
            otherEntities.Insert(Random.Range(0, otherEntities.Count + 1), target);
        }

        EntityAlignment(true);
        EntityAlignment(false);
        return true;
    }
    public int CountMinions(bool isMine)
    {
        var list = isMine ? myEntities : otherEntities;
        int count = 0;

        foreach(var ent in list)
        {
            if(ent == null) continue;
            if(ent.isBossOrEmpty) continue;
            count++;
        }
        return count;
    }
    public Entity FindRandomEntity(bool isTargetIsMine, bool isIncludeBoss, bool isOnlyMinion)
    {
        // 2) 보스 포함 여부       
        var candidates = GetAliveTargetCandidates(isTargetIsMine, isIncludeBoss, isOnlyMinion);
        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }
    public Entity FindEnemyBoss(bool isMine)
    {
        return isMine ? otherBossEntity : myBossEntity;
    }
}