using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] Image card;       //카드 전체 프리팹
    [SerializeField] Image character;  //케릭터 스프라이트
    [SerializeField] TMP_Text nameTMP;          // 이름
    [SerializeField] TMP_Text attackTMP;        //공격력
    [SerializeField] TMP_Text cost;             //마나
    [SerializeField] TMP_Text healthTMP;        //체력
    [SerializeField] TMP_Text ability;          //카드 고유 능력
    [SerializeField] Sprite cargFront;          //카드 앞면
    [SerializeField] Sprite carfBack;           //카드 뒷면

    public Item item;
    bool isFront;
    public PositionRotationScale originPRS;

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
        }
        else
        {
            card.sprite = this.item.backGround;
            nameTMP.text = "";
            attackTMP.text = "";
            healthTMP.text = "";
            cost.text = "";
            ability.text = "";
            character.gameObject.SetActive(false);
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
}
