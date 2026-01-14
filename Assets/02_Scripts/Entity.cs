using DG.Tweening;
using TMPro;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public bool isUI;

    [SerializeField] Item item;
    [SerializeField] SpriteRenderer entity;
    [SerializeField] SpriteRenderer character;
    [SerializeField] TMP_Text attackTMP;
    [SerializeField] TMP_Text heathTMP;
    [SerializeField] GameObject sleepParticle;
    [SerializeField] SpriteRenderer outLineRenderer;
    [SerializeField] SpriteRenderer provocationSpriteOutLIne;
    [SerializeField] SpriteRenderer provocationSprite;

    public int attack;
    public int heath;
    public bool isMine;
    public bool isDead;
    public bool isBossOrEmpty;
    public bool attackAble;
    public Vector3 originPos;
    int liveCount;

    public bool isRush;
    public bool isProvocation;
    public bool isBattleCry;

    private Tween outLineTween;

    private void Start()
    {
        TurnManager.Instance.UnsubscribeOnTurnStarted(OnTurnStarted);
        TurnManager.Instance.SubscribeOnTurnStarted(OnTurnStarted);

        if (isBossOrEmpty || isUI)
        {
            Vector3 screenPos = transform.position;

            screenPos.z = -Camera.main.transform.position.z;

            originPos = Camera.main.ScreenToWorldPoint(screenPos);

            originPos.z = 0;
        }

        if (outLineRenderer == null) return;
        outLineRenderer.gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        TurnManager.Instance.UnsubscribeOnTurnStarted(OnTurnStarted);
    }
    public void TurnOnOffOutLine(bool isAttackable)
    {
        if (outLineRenderer == null) return;
        if (provocationSpriteOutLIne == null) return;

        if (isAttackable)
        {
            if (item.isProvocation)
            {
                outLineRenderer.gameObject.SetActive(false);
                provocationSpriteOutLIne.gameObject.SetActive(true);
                provocationSpriteOutLIne.DOKill();

                provocationSpriteOutLIne.DOFade(0.5f, 1f)
                .SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                outLineRenderer.gameObject.SetActive(true);

                outLineRenderer.DOKill();
                outLineTween = outLineRenderer.DOFade(0.5f, 1f)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }
        else
        {
            outLineRenderer.DOKill();
            outLineRenderer.gameObject.SetActive(false);

            provocationSpriteOutLIne.DOKill();
            provocationSpriteOutLIne.gameObject.SetActive(false);
        }
    }
    void OnTurnStarted(bool myTurn)
    {
        if (isBossOrEmpty)
            return;

        if (isMine == myTurn)
            liveCount++;
        if (!isRush)
        {
            sleepParticle.SetActive(liveCount < 1);
        }
        else
        {
            sleepParticle.SetActive(false);
        }
    }
    public void Setup(Item item)
    {
        attack = item.attack;
        heath = item.health;

        this.item = item;
        character.sprite = this.item.sprite;
        attackTMP.text = attack.ToString();
        heathTMP.text = heath.ToString();

        this.isRush = item.isRush;
        this.isProvocation = item.isProvocation;
        this.isBattleCry = item.isBattleCry;

        if (this.item.isRush)
        {
            this.attackAble = true;
            sleepParticle.SetActive(false);
        }
        else
        {
            sleepParticle.SetActive(true);
        }

        if (this.isProvocation)
        {
            //도발 스프라이트 on
            provocationSprite.gameObject.SetActive(true);
        }
    }
    public void MoveTransform(Vector3 pos, bool useDotween, float dotweenTIme = 0f)
    {
        if (useDotween)
            transform.DOMove(pos, dotweenTIme);
        else
            transform.position = pos;
    }
    public bool TakeDamage(int damage)
    {
        heath -= damage;
        heathTMP.text = heath.ToString();

        if (heath <= 0)
        {
            isDead = true;
            return true;
        }
        return false;
    }
    private void OnMouseDown()
    {
        if (isMine)
            EntityManager.Instance.EntityMouseDown(this);
    }
    private void OnMouseUp()
    {
        if (isMine)
            EntityManager.Instance.EntityMouseUp();
    }
    private void OnMouseDrag()
    {
        if (isMine)
            EntityManager.Instance.EntityMouseDrag();
    }
}
