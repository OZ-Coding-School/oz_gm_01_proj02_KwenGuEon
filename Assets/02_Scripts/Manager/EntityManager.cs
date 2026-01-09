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

    [SerializeField] GameObject entityPrefab;
    [SerializeField] GameObject damagePrefab;
    [SerializeField] List<Entity> myEntities;
    [SerializeField] List<Entity> otherEntities;

    [SerializeField] Entity myEmptyEntity;
    [SerializeField] Entity myBossEntity;
    [SerializeField] Entity otherBossEntity;

    [SerializeField] GameObject targetPicker;
    [SerializeField] Transform damageCanvas;

    [Range(0f, 5f)][SerializeField] float entitySpacing = 2.3f;
    WaitForSeconds delay1Sc = new WaitForSeconds(1.0f);
    WaitForSeconds delay2Sc = new WaitForSeconds(2.0f);

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
    [SerializeField] float damageRandomRange = 30.0f;

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

            if (entity.isMine)
                myEntities.Remove(entity);
            else
                otherEntities.Remove(entity);

            Sequence sequence = DOTween.Sequence()
                .Append(entity.transform.DOShakePosition(0.5f, 0.5f, 30))
                .Append(entity.transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.OutCirc))
                .OnComplete(() =>
                {
                    EntityAlignment(entity.isMine);
                    Destroy(entity.gameObject);
                });
        }
        StartCoroutine(CheckBossDead());
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
        CardManager.instance.TryPutCard(false);
        yield return delay1Sc;

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
        yield return delay2Sc;

        if (myBossEntity.isDead)
            TurnManager.Instance.TriggerOnGameResult(false);
        //StartCoroutine(GameManager.Instance.GameOver(false));

        if (otherBossEntity.isDead)
            TurnManager.Instance.TriggerOnGameResult(true);
        //StartCoroutine(GameManager.Instance.GameOver(true));
    }
    //디버깅용
    public void DamageBoss(bool isMine, int damage)
    {
        var targetBpssEntity = isMine ? myBossEntity : otherBossEntity;
        targetBpssEntity.TakeDamage(damage);
        StartCoroutine(CheckBossDead());
    }
    void EntityAlignment(bool isMine)
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
    public bool SpawnEntity(bool isMine, Item item, Vector3 spawnPos)
    {
        if (isMine)
        {
            if (isFullMyEntities || !ExistMyEmptyEntity) return false;
        }
        else
        {
            if (isFullOtherEntities) return false;
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

        return true;
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

        bool existTarget = false;
        foreach (var hit in Physics2D.RaycastAll(Utils.MousePos, Vector3.forward))
        {
            Entity entity = hit.collider?.GetComponent<Entity>();
            if (entity != null && !entity.isMine && selectEntity.attackAble)
            {
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
    }
}
