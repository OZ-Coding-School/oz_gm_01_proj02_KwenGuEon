using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Image card;       //카드 전체 프리팹
    [SerializeField] Image character;  //케릭터 스프라이트
    [SerializeField] TMP_Text nameTMP;          // 이름
    [SerializeField] TMP_Text attackTMP;        //공격력
    [SerializeField] TMP_Text cost;             //마나
    [SerializeField] TMP_Text healthTMP;        //체력
    [SerializeField] TMP_Text ability;          //카드 고유 능력
    [SerializeField] Sprite cardFront;          //카드 앞면
    [SerializeField] Sprite cardBack;           //카드 뒷면
    [SerializeField] Transform cardVisual;

    [SerializeField] GameObject cardFrontGroup;
    [SerializeField] GameObject cardBackGround;


    public Item item;
    public bool isFront;
    public PositionRotationScale originPRS;

    private void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isFront)
            CardManager.instance.CardMouseOver(this);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isFront)
            CardManager.instance.CardMouseExit(this);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isFront)
            CardManager.instance.CardMouseDown(this);
    }
    public void OnDrag(PointerEventData eventData)
    {        
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (isFront)
            CardManager.instance.CardMouseUp();
    }
    public void SetDragPosition(Vector3 pos)
    {
        cardVisual.DOKill();
        cardVisual.localPosition = pos;
        cardVisual.rotation = Utils.QI;
        cardVisual.localScale = Vector3.one;
    }
    public void Setup(Item item, bool isFrount)
    {
        this.item = item;
        this.isFront = isFrount;

        if(this.isFront)
        {
            character.sprite = this.item.sprite;
            nameTMP.text = this.item.name;
            attackTMP.text = this.item.attack.ToString();
            healthTMP.text = this.item.health.ToString();
            cost.text = this.item.cardCost.ToString();
            ability.text = this.item.cardInfo;
            character.gameObject.SetActive(true);

            cardFrontGroup.gameObject.SetActive(true);
            cardBackGround.gameObject.SetActive(false);
        }
        else
        {
            card.sprite = cardBack;
            nameTMP.text = "";
            attackTMP.text = "";
            healthTMP.text = "";
            cost.text = "";
            ability.text = "";
            character.gameObject.SetActive(false);

            cardFrontGroup.gameObject.SetActive(false);
            cardBackGround.gameObject.SetActive(true);
        }
    }
    public void MoveTransform(PositionRotationScale prs, bool isUseDoTween, float doTweenTime = 0.0f)
    {
        if(isUseDoTween)
        {
            transform.DOMove(prs.pos, doTweenTime);
            transform.DORotateQuaternion(prs.rot, doTweenTime);
            transform.DOScale(prs.scale, doTweenTime);
        }
        else
        {
            transform.position = prs.pos; 
            transform.rotation = prs.rot;
            transform.localScale = prs.scale;
        }
    }
    public void MoveVisualTransform(PositionRotationScale prs, bool isEnlarge, float doTweenTime = 0.0f)
    {
        if(isEnlarge)
        {
            cardVisual.transform.DOMove(prs.pos, doTweenTime);
            cardVisual.transform.DORotateQuaternion(prs.rot, doTweenTime);
            cardVisual.transform.DOScale(prs.scale, doTweenTime);
        }
        else
        {
            cardVisual.transform.position = prs.pos;
            cardVisual.transform.rotation = prs.rot;
            cardVisual.transform.localScale = prs.scale;
        }
    }
}
