using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardManager : MonoBehaviour
{
    public static CardManager instance;

    private Camera uiCamera;

    [SerializeField] ItemSO itemSO;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] float arcHeight = 70.0f;

    [SerializeField] private List<Card> myCards;
    [SerializeField] private List<Card> otherCards;

    [SerializeField] private Transform cardSpawnPoint;
    [SerializeField] private Transform otherCardSpawnPoint;
    [SerializeField] private RectTransform cardCanvas;

    [SerializeField] private Transform playerCardLeft;
    [SerializeField] private Transform playerCardRight;
    [SerializeField] private Transform aiGamerCardLeft;
    [SerializeField] private Transform aiGamerCardRight;

    private Entity spellTarget;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private EffectManager effectManager;
    private Entity BattleCryCaster;
    private Item BattleCryItem;

    [Header("Enlarge")]
    [SerializeField] float enlargeScale = 3.5f;
    [SerializeField] float enlargeYPos = -4.8f;
    [SerializeField] float enlargeZPos = -100f;

    [Header("Hand Area")]
    [SerializeField] private RectTransform handAreaRect;

    List<Item> itemBuffer;

    [SerializeField] List<Item> myDeck = new List<Item>();
    [SerializeField] List<Item> otherDeck = new List<Item>();

    Card selectCard;
    Card dragCard;
    bool isMyCardDrag;
    [SerializeField] ECardState cardState;

    private bool isCurrentDrawMine = true;
    private int myFatigueDamageCount = 0;
    private int otherFatigueDamageCount = 0;
    private int battleCryArmedFrame = -1;
    enum ECardState { Nothing, CanMouseOver, CanMouseDrag }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (effectManager == null)
            effectManager = FindAnyObjectByType<EffectManager>();
    }
    private void Start()
    {
        //인게임 테스트용
        //SerupItemBuffer();

        SetupGameDecks();

        uiCamera = Camera.main;

        TurnManager.Instance.UnsubscribeOnAddCard(AddCard);
        TurnManager.Instance.SubscribeOnAddCard(AddCard);

        TurnManager.Instance.UnsubscribeOnTurnStarted(OnTurnStarted);
        TurnManager.Instance.SubscribeOnTurnStarted(OnTurnStarted);
    }
    private void OnDestroy()
    {
        TurnManager.Instance.UnsubscribeOnAddCard(AddCard);
        TurnManager.Instance.UnsubscribeOnTurnStarted(OnTurnStarted);
    }
    void OnTurnStarted(bool isMyTurn)
    {
    }
    private void Update()
    {
        if (TurnManager.Instance.isLoading) return;

        if (isMyCardDrag && dragCard != null)
            CardDrag();
        else
            spellTarget = DetectSpellTarget();

        SetECardState();

        if (cardState != ECardState.Nothing)
            DetectCardPointer();

        if (BattleCryCaster != null && BattleCryItem != null)
        {
            //타겟이 필요한 경우에만 갱신
            if (TargetUtil.RequiresExternalTarget(BattleCryItem))
            {
                spellTarget = DetectSpellTarget();
            }

            //하수인을 내려놓은 이후에 마우스 클릭 시 발동
            if (Time.frameCount > battleCryArmedFrame && Input.GetMouseButtonDown(0))
            {
                bool needTarget = TargetUtil.RequiresExternalTarget(BattleCryItem);

                if (needTarget && !IsTargetAllowed(BattleCryItem, spellTarget))
                {
                    return;
                }

                //타겟이 필요 없거나 유효한 타겟이 있는 경우 능력 발동
                effectManager.RunAbilities(BattleCryItem, BattleCryCaster, spellTarget);

                //초기화
                BattleCryCaster = null;
                BattleCryItem = null;
                spellTarget = null;
                battleCryArmedFrame = -1;
            }
        }
    }
    //청소용 사용안할시 삭제 예정
    private void ClearBattleCry()
    {
        BattleCryCaster = null;
        BattleCryItem = null;
        spellTarget = null;
        battleCryArmedFrame = -1;
    }
    public bool IsPointerInHandArea(Vector2 screenPos)
    {
        if (handAreaRect == null) return false;

        return RectTransformUtility.RectangleContainsScreenPoint(handAreaRect, screenPos, null);
    }
    public Item PopItem(bool isMine)
    {

        if(isMine)
        {
            if(myDeck.Count == 0)
            {
                myFatigueDamageCount++;
                EntityManager.Instance.DamageBoss(true, myFatigueDamageCount);
                return null;
            }

            Item item = myDeck[0];
            myDeck.RemoveAt(0);
            return item;
        }
        else
        {
            if(otherDeck.Count == 0)
            {
                otherFatigueDamageCount++;
                EntityManager.Instance.DamageBoss(false, otherFatigueDamageCount);
                return null;
            }
            Item item = otherDeck[0];
            otherDeck.RemoveAt(0);
            return item;
        }
    }
    //테스트용
    void SetupItemBuffer()
    {
        itemBuffer = new List<Item>();
        for (int i = 0; i < itemSO.items.Length; i++)
        {
            Item item = itemSO.items[i];
            for (int j = 0; j < item.percent; j++)
            {
                itemBuffer.Add(item);
            }
        }

        for (int i = 0; i < itemBuffer.Count; i++)
        {
            int rand = UnityEngine.Random.Range(i, itemBuffer.Count);
            Item temp = itemBuffer[i];
            itemBuffer[i] = itemBuffer[rand];
            itemBuffer[rand] = temp;
        }
    }
    void SetupGameDecks()
    {
        if (DeckManager.instance != null && DeckManager.instance.myPlayCardDeck.Count >= 30)
        {
            myDeck = new List<Item>(DeckManager.instance.myPlayCardDeck);
        }
        else
        {
            return;
        }
        RandomDeck(otherDeck, 30);

        ShuffleDeck(myDeck);
        ShuffleDeck(otherDeck);
    }
    void RandomDeck(List<Item> deckList, int cardCount)
    {
        if (itemSO == null) return;
        for(int i = 0; i < cardCount; i++)
        {
            deckList.Add(itemSO.items[Random.Range(0, itemSO.items.Length)]);
        }
    }
    
    //덱 섞기
    void ShuffleDeck(List<Item> deckList)
    {
        for (int i = 0; i < deckList.Count; i++)
        {
            int rand = UnityEngine.Random.Range(i, deckList.Count);
            Item temp = deckList[i];
            deckList[i] = deckList[rand];
            deckList[rand] = temp;
        }
    }
    public void AddCard(bool isMine)
    {
        isCurrentDrawMine = isMine;
        Item item = PopItem(isMine);
        if (item == null) return;

        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI, cardCanvas);
        var card = cardObject.GetComponent<Card>();
        card.Setup(item, isMine);
        (isMine ? myCards : otherCards).Add(card);

        SetOriginorder(isMine);
        CardAlignment(isMine);
    }
    public void DrawCard(bool isMine, int count)
    {
        count = Mathf.Max(0, count);
        for (int i = 0; i < count; i++)
        {
            AddCard(isMine);
        }
    }
    void SetOriginorder(bool isMine)
    {
        int count = isMine ? myCards.Count : otherCards.Count;
        for (int i = 0; i < count; i++)
        {
            var targetCard = isMine ? myCards[i] : otherCards[i];
            targetCard?.GetComponent<SortingOrder>().SetOriginOrder(i);
        }
    }
    void CardAlignment(bool isMine) //카드 정렬
    {
        List<PositionRotationScale> originCardPRSs = new List<PositionRotationScale>();

        if (isMine)
            originCardPRSs = RoundAlignment(playerCardLeft, playerCardRight, myCards.Count, arcHeight, Vector3.one * 1.1f);
        else
            originCardPRSs = RoundAlignment(aiGamerCardLeft, aiGamerCardRight, otherCards.Count, -arcHeight, Vector3.one * 1.1f);

        var targetCards = isMine ? myCards : otherCards;
        for (int i = 0; i < targetCards.Count; i++)
        {
            var targetCard = targetCards[i];

            targetCard.originPRS = originCardPRSs[i];
            targetCard.MoveTransform(targetCard.originPRS, true, 0.7f);
        }
    }
    List<PositionRotationScale> RoundAlignment(Transform leftTr, Transform rightTr, int objCount, float height, Vector3 scale)
    {
        float[] objLerps = new float[objCount];
        List<PositionRotationScale> results = new List<PositionRotationScale>(objCount);

        switch (objCount)
        {
            case 1: objLerps = new float[] { 0.5f }; break;
            case 2: objLerps = new float[] { 0.27f, 0.73f }; break;
            case 3: objLerps = new float[] { 0.1f, 0.5f, 0.9f }; break;
            default:
                float interval = 1.0f / (objCount - 1);
                for (int i = 0; i < objCount; i++)
                {
                    objLerps[i] = interval * i;
                }
                break;
        }

        for (int i = 0; i < objCount; i++)
        {
            var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
            var targetRot = Quaternion.identity;

            if (objCount >= 4)
            {
                float curveY = Mathf.Sin(objLerps[i] * Mathf.PI) * height;
                targetPos.y += curveY;
                targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]);
            }
            results.Add(new PositionRotationScale(targetPos, targetRot, scale));
        }
        return results;
    }
    public bool TryPutCard(bool isMine)
    {
        if (isMine && selectCard != null)
        {
            int cost = selectCard.item.cardCost;
            if (TurnManager.Instance.myMana < cost) return false;            
        }

        if (!isMine && otherCards.Count <= 0)
            return false;

        Card card = isMine ? selectCard : otherCards[UnityEngine.Random.Range(0, otherCards.Count)];

        if (!isMine)
        {
            int cost = card.item.cardCost;
            if (TurnManager.Instance.otherMana < cost) return false;            
        }

        if (!isMine && card.item.isSpell)
        {
            bool onlyMinion = IsOnlyMinionTarget(card.item);
            Entity target = null;
            if (card.item.needTarget)
            {
                bool isPositive = IsPositiveItem(card.item);
                bool isTargetSide = isPositive ? false : true;

                if (onlyMinion)
                    target = EntityManager.Instance.FindRandomEntity(isTargetSide, false, true);
                else
                    target = EntityManager.Instance.FindRandomEntity(isTargetSide, true, false);
            }

            if (card.item.needTarget && target == null) return false;

            return TryUseSpell(false, card, target);
        }

        var spawnPos = isMine ? Utils.MousePos : otherCardSpawnPoint.position;
        var targetCards = isMine ? myCards : otherCards;

        Entity spawned = null;
        bool isMineSpawn = EntityManager.Instance.SpawnEntity(isMine, card.item, spawnPos, out spawned);

        if (isMineSpawn)
        {
            if (isMine) TurnManager.Instance.UseMana(true, card.item.cardCost);
            else TurnManager.Instance.UseMana(false, card.item.cardCost);

            targetCards.Remove(card);
            card.transform.DOKill();
            DestroyImmediate(card.gameObject);
            if (isMine)
            {
                selectCard = null;
            }
            CardAlignment(isMine);

            if (!isMine && spawned != null && card.item.isBattleCry)
            {
                bool onlyMinion = IsOnlyMinionTarget(card.item);
                Entity target = null;
                if (TargetUtil.RequiresExternalTarget(card.item))
                {
                    bool isPositive = IsPositiveItem(card.item);
                    bool targetSide = isPositive ? false : true;

                    target = EntityManager.Instance.FindRandomEntity(isTargetIsMine: targetSide, isIncludeBoss: !onlyMinion, isOnlyMinion: onlyMinion);
                }

                effectManager.RunAbilities(card.item, spawned, target);
            }
            return true;
        }
        else
        {
            targetCards.ForEach(x => x.GetComponent<SortingOrder>().SetMostFrontOrder(false));
            CardAlignment(isMine);
            return false;
        }
    }
    private bool TryPutMinionBattleCry(bool isMine, Card card)
    {
        if (card == null) return false;
        if (card.item == null) return false;
        if (card.item.isSpell) return false;

        int cost = card.item.cardCost;
        if (TurnManager.Instance.myMana < cost) return false;        

        if (card.item.isBattleCry && TargetUtil.RequiresExternalTarget(card.item))
        {
            bool onlyMinion = IsOnlyMinionTarget(card.item);
            var candidates = EntityManager.Instance.GetAliveTargetCandidatesAll(includeBoss: !onlyMinion, onlyMinion: onlyMinion);

            bool isTargetExist = false;
            foreach (var ent in candidates)
            {
                if (IsTargetAllowed(card.item, ent))
                {
                    isTargetExist = true;
                    break;
                }
            }
            if (!isTargetExist) return false; // 타겟 없어서 낼 수 없음
        }

        var spawnPos = Utils.MousePos;

        if (!EntityManager.Instance.SpawnEntity(true, card.item, spawnPos, out var spawned))
            return false;

        TurnManager.Instance.UseMana(true, cost);

        myCards.Remove(card);
        Destroy(card.gameObject);
        CardAlignment(true);

        if (spawned != null && card.item.isBattleCry)
        {
            // 타겟이 필요한 경우 -> Update에서 클릭 대기
            if (TargetUtil.RequiresExternalTarget(card.item))
            {
                BattleCryCaster = spawned;
                BattleCryItem = card.item;
                battleCryArmedFrame = Time.frameCount; // 현재 프레임 저장 (바로 클릭되는 것 방지)
            }
            // 타겟이 필요 없는 경우 -> 즉시 발동
            else
            {
                effectManager.RunAbilities(card.item, spawned, null);
            }
        }

        return true;
    }
    public bool TryUseSpell(bool isMine, Card usedCard, Entity target)
    {
        if (usedCard == null) return false;

        Item item = usedCard.item;
        if (item == null) return false;

        if (isMine)
        {
            if (TurnManager.Instance.myMana < item.cardCost)
            {                
                return false;
            }
        }
        else
        {            
            if (TurnManager.Instance.otherMana < item.cardCost) return false;
        }

        if (item.needTarget && !IsTargetAllowed(item, target)) return false;
        if (TargetUtil.RequiresExternalTarget(item) && target == null) return false;

        bool isUseSpell = EntityManager.Instance.RunSpell(isMine, item, target);
        if (!isUseSpell) return false;

        TurnManager.Instance.UseMana(isMine, item.cardCost);

        var list = isMine ? myCards : otherCards;
        list.Remove(usedCard);

        Destroy(usedCard.gameObject);

        CardAlignment(isMine);

        if (dragCard == usedCard) dragCard = null;
        if (isMine) selectCard = null;

        return true;
    }
    public void DiscardAllHand(bool isMine)
    {
        var list = isMine ? myCards : otherCards;

        for (int i = 0; i < list.Count; i++)
        {
            var card = list[i];
            if (card != null) Destroy(card.gameObject);
        }
        list.Clear();
    }
    public void DiscardAllDeck()
    {
        if (itemBuffer != null) itemBuffer.Clear();
    }
    public void CardMouseOver(Card card)
    {
        if (cardState == ECardState.Nothing) return;

        if (!isMyCardDrag)
        {
            selectCard = card;
            EnlargeCard(true, card);
        }
    }
    public void CardMouseExit(Card card)
    {
        if (!isMyCardDrag)
            EnlargeCard(false, card);
    }
    public void CardMouseDown(Card card)
    {
        if (cardState != ECardState.CanMouseDrag) return;

        isMyCardDrag = true;
        dragCard = card;
    }
    public void CardMouseUp()
    {
        isMyCardDrag = false;

        if (cardState != ECardState.CanMouseDrag) return;        

        if (dragCard != null)
        {
            if (!IsPointerInHandArea(Input.mousePosition))
            {
                if (dragCard.item.isSpell)
                {
                    if (TryUseSpell(true, dragCard, spellTarget))
                    {
                        dragCard = null;
                        spellTarget = null;
                        return;
                    }
                }
                else
                {
                    if (TryPutMinionBattleCry(true, dragCard))
                    {
                        dragCard = null;
                        return;
                    }
                }
            }
            dragCard.GetComponent<SortingOrder>().SetMostFrontOrder(false);

            dragCard.MoveVisualTransform(dragCard.originPRS, false);
            dragCard = null;
            spellTarget = null;
            EntityManager.Instance.RemoveMyEmptyEntity();
        }
        DetectCardPointer();
    }
    void CardDrag()
    {
        bool isHand = IsPointerInHandArea(Input.mousePosition);

        if (isHand)
        {
            dragCard.MoveVisualTransform(dragCard.originPRS, false);
        }
        else
        {
            Vector2 mousePos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)dragCard.transform,
                Input.mousePosition,
                null,
                out mousePos
            );

            dragCard.SetDragPosition(new Vector3(mousePos.x, mousePos.y, 0));
            if (!dragCard.item.isSpell)
            {
                EntityManager.Instance.InsertMyEmptyEntity(Utils.MousePos.x);
            }
            else
            {
                spellTarget = DetectSpellTarget();
            }

        }
    }
    private Entity DetectSpellTarget()
    {
        foreach (var hit in Physics2D.RaycastAll(Utils.MousePos, Vector3.forward))
        {
            var e = hit.collider?.GetComponentInParent<Entity>();
            if (e == null) continue;

            if (!e.isBossOrEmpty ||
            e == EntityManager.Instance.GetBoss(true) ||
            e == EntityManager.Instance.GetBoss(false))
            {
                return e;
            }
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var r in results)
        {
            var e = r.gameObject.GetComponentInParent<Entity>();
            if (e == null) continue;

            if (!e.isBossOrEmpty ||
            e == EntityManager.Instance.GetBoss(true) ||
            e == EntityManager.Instance.GetBoss(false))
            {
                return e;
            }
        }

        return null;
    }
    void DetectCardPointer()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        foreach (var result in results)
        {

            Card card = result.gameObject.GetComponentInParent<Card>();
            if (card != null && card.isFront)
            {
                CardMouseOver(card); // 강제로 마우스 오버 함수 실행
                break;
            }
        }
    }
    void EnlargeCard(bool isEnlarge, Card card)
    {
        if (isEnlarge)
        {
            Vector3 enlargePos = new Vector3(card.originPRS.pos.x, enlargeYPos, enlargeZPos);
            card.MoveVisualTransform(new PositionRotationScale(enlargePos, Utils.QI, Vector3.one * enlargeScale), false);
        }
        else
        {
            card.MoveVisualTransform(card.originPRS, false);
        }

        card.GetComponent<SortingOrder>().SetMostFrontOrder(isEnlarge);
    }
    void SetECardState()
    {
        if (TurnManager.Instance.isLoading)
            cardState = ECardState.Nothing;

        else if (!TurnManager.Instance.isMyTurn) // || myPutCount == 1 || EntityManager.Instance.isFullMyEntities
            cardState = ECardState.CanMouseOver;

        else if (TurnManager.Instance.isMyTurn)
            cardState = ECardState.CanMouseDrag;
    }
    private bool IsOnlyMinionTarget(Item item)
    {
        if (item == null || item.abilities == null) return false;

        foreach (var ab in item.abilities)
        {
            var rule = ab.targetRule;

            if (rule.isOnlyMinion) return true;
            if (rule.targetGroup == TargetGroup.OnlyEnemyMinions) return true;
        }
        return false;
    }
    private bool IsTargetAllowed(Item item, Entity target)
    {
        if (item == null) return false;
        if (!item.needTarget) return true;
        if (target == null) return false;

        bool onlyMinion = IsOnlyMinionTarget(item);
        if (onlyMinion)
        {
            if (target == EntityManager.Instance.GetBoss(true) ||
                target == EntityManager.Instance.GetBoss(false) ||
                target.isBossOrEmpty)
                return false;
        }
        return true;
    }
    private bool IsPositiveItem(Item item)
    {
        if (item == null || item.abilities == null) return false;

        foreach(var ability in item.abilities)
        {
            switch(ability.effectType)
            {
                case (EffectType.Heal):                    
                case (EffectType.BuffStats):                    
                case (EffectType.Mana):                    
                case (EffectType.Draw):                    
                    return true;

                case (EffectType.Damage):                    
                case (EffectType.Kill):                    
                case (EffectType.StatusAbnormality):                    
                case (EffectType.MoveMinion):
                    return false;
            }
        }

        return false;
    }
}
