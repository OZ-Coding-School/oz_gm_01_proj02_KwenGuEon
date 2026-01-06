using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private Transform cardCanvas;

    [SerializeField] private Transform playerCardLeft;
    [SerializeField] private Transform playerCardRight;
    [SerializeField] private Transform aiGamerCardLeft;
    [SerializeField] private Transform aiGamerCardRight;

    [SerializeField] private TurnManager turnManager;

    List<Item> itemBuffer;

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
    }
    private void OnDestroy()
    {
        TurnManager.Instance.UnsubscribeOnAddCard(AddCard);
    }
    private void Update()
    {

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
            int rand = Random.Range(i, itemBuffer.Count);
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
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
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
}
