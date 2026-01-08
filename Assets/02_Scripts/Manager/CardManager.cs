using DG.Tweening;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardManager : MonoBehaviour
{
    public static CardManager instance;

    [SerializeField] ItemSO itemSO;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private float cardOriginScale = 1.1f;
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
    

    [SerializeField] private TurnManager turnManager;

    [Header("Enlarge")]
    [SerializeField] float enlargeScale = 3.5f;
    [SerializeField] float enlargeYPos = -4.8f;
    [SerializeField] float enlargeZPos = -100f;

    [Header("Hand Area")]
    [SerializeField] private RectTransform handAreaRect;

    List<Item> itemBuffer;
    Card selectCard;
    Card dragCard;
    bool isMyCardDrag;
    [SerializeField] ECardState cardState;
    int myPutCount;
    enum ECardState {Nothing, CanMouseOver, CanMouseDrag }

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
    }
    private void Start()
    {
        SerupItemBuffer();
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
        if (isMyTurn)
            myPutCount = 0;
    }
    private void Update()
    {
        if (isMyCardDrag && dragCard != null)
            CardDrag();

        SetECardState();
    }
   
    public bool IsPointerInHandArea(Vector2 screenPos)
    {
        if (handAreaRect == null) return false;

        return RectTransformUtility.RectangleContainsScreenPoint(handAreaRect, screenPos, null);
    }
    public Item PopItem()
    {
        if (itemBuffer.Count == 0)
        {
            //TODO: 데미지 주는 로직 1부터 ++
        }
        Item item = itemBuffer[0];
        itemBuffer.RemoveAt(0);
        return item;
    }
    void SerupItemBuffer()
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
    void AddCard(bool isMine)
    {
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI, cardCanvas);
        var card = cardObject.GetComponent<Card>();
        card.Setup(PopItem(), isMine);
        (isMine ? myCards : otherCards).Add(card);

        SetOriginorder(isMine);
        CardAlignment(isMine);
    }
    void SetOriginorder(bool isMine)
    {
        int count = isMine ? myCards.Count : otherCards.Count;
        for(int i = 0; i < count; i++)
        {
            var targetCard = isMine? myCards[i] : otherCards[i];
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
        for(int i = 0; i < targetCards.Count; i++)
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

        switch(objCount)
        {
            case 1: objLerps = new float[] { 0.5f }; break;
            case 2: objLerps = new float[] { 0.27f, 0.73f }; break;
            case 3: objLerps = new float[] { 0.1f, 0.5f, 0.9f }; break;
            default:
                float interval = 1.0f / (objCount - 1);
                for(int i = 0; i < objCount; i++)
                {
                    objLerps[i] = interval * i;
                }
                break;
        }

        for(int i = 0; i < objCount; i++)
        {
            var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
            var targetRot = Quaternion.identity;

            if(objCount >= 4)
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

        if (isMine && myPutCount >= 1) //카드를 1개만 내면 끝 나중에 마나로 수정
        {                        
            return false;
        }

        if(!isMine && otherCards.Count <= 0)
            return false;

        Card card = isMine ? selectCard : otherCards[UnityEngine.Random.Range(0, otherCards.Count)];        

        var spawnPos = isMine ? Utils.MousePos : otherCardSpawnPoint.position;
        var targetCards = isMine ? myCards : otherCards;       

        if (EntityManager.Instance.SpawnEntity(isMine, card.item, spawnPos))
        {           
            targetCards.Remove(card);
            card.transform.DOKill();
            DestroyImmediate(card.gameObject);
            if(isMine)
            {
                selectCard = null;
                myPutCount++;
            }
            CardAlignment(isMine);
            return true;
        }
        else
        {            
            targetCards.ForEach(x => x.GetComponent<SortingOrder>().SetMostFrontOrder(false));
            CardAlignment(isMine);
            return false;
        }
    }
    public void CardMouseOver(Card card)
    {
        if (cardState == ECardState.Nothing) return;

        if(!isMyCardDrag)
        {
            selectCard = card;
            EnlargeCard(true, card);
        }
    }
    public void CardMouseExit(Card card)
    {
        if(!isMyCardDrag)
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
            if(!IsPointerInHandArea(Input.mousePosition) && TryPutCard(true))
            {
                dragCard = null;
            }
            else
            {
                dragCard.GetComponent<SortingOrder>().SetMostFrontOrder(false);

                dragCard.MoveVisualTransform(dragCard.originPRS, false);
                dragCard = null;
                EntityManager.Instance.RemoveMyEmptyEntity();
            }                
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
            EntityManager.Instance.InsertMyEmptyEntity(Utils.MousePos.x);
        }
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
        if(isEnlarge)
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

        else if(TurnManager.Instance.isMyTurn)
            cardState = ECardState.CanMouseDrag;            
    }
}
