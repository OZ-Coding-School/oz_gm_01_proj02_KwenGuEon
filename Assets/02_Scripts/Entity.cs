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
    [SerializeField] TMP_Text healthTMP;
    [SerializeField] GameObject sleepParticle;
    [SerializeField] SpriteRenderer outLineRenderer;
    [SerializeField] SpriteRenderer provocationSpriteOutLIne;
    [SerializeField] SpriteRenderer provocationSprite;

    public int attack;
    public int health;
    public int maxHealth;
    public int temporaryAttack;
    public bool isMine;
    public bool isDead;
    public bool isBossOrEmpty;
    public bool attackAble;
    public int cantActTurns;
    public bool isCantAct => cantActTurns > 0;
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

        if (isMine != myTurn)
            RemoveTempAttackThisTurn();

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
        health = item.health;
        maxHealth = item.health;

        this.item = item;
        character.sprite = this.item.sprite;
        attackTMP.text = attack.ToString();
        healthTMP.text = health.ToString();

        this.isRush = item.isRush;
        this.isProvocation = item.isProvocation;
        this.isBattleCry = item.isBattleCry;

        if (this.item.isRush)
        {
            attackAble = true;
            sleepParticle.SetActive(false);
            outLineRenderer.gameObject.SetActive(true);
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
        health -= damage;
        UpdateHealthUI();

        if (health <= 0)
        {
            isDead = true;
            return true;
        }
        return false;
    }
    public void Heal(int heal)
    {
        health += heal;

        if (health >= maxHealth)
        {
            health = maxHealth;
        }

        UpdateHealthUI();
    }
    public void AttackUP(int plusAttack)
    {
        attack += plusAttack;
        if (attackTMP != null) attackTMP.text = attack.ToString();
    }
    public void GrantHealth(int amount)
    {
        maxHealth += amount;
        if (health > maxHealth) health = maxHealth;
        UpdateHealthUI();
    }
    /// <summary>
    /// 공격력, 체력을 지정한 숫자로 만든다
    /// SetHealth, SetAttack 
    /// </summary>
    /// <param name="value"></param>
    public void SetHealth(int value)
    {
        health = Mathf.Clamp(value, 0, maxHealth);
        UpdateHealthUI();

        if (health <= 0) isDead = true;
    }
    public void SetAttack(int value)
    {
        attack = Mathf.Max(0, value);
        if (attackTMP != null) attackTMP.text = attack.ToString();
    }
    public void TempAttackThisTurn(int amount)
    {
        if (amount == 0) return;

        temporaryAttack += amount;
        attack += amount;

        if (attackTMP != null)
            attackTMP.text = attack.ToString();
    }
    public void RemoveTempAttackThisTurn()
    {
        if (temporaryAttack == 0) return;

        attack -= temporaryAttack;
        temporaryAttack = 0;

        if (attack < 0) attack = 0;
        if (attackTMP != null)
            attackTMP.text = attack.ToString();
    }
    public void ConsumeCantActOnMyTurnStart()
    {
        if (cantActTurns > 0) cantActTurns--;
    }
    public void UpdateHealthUI()
    {
        if (healthTMP != null)
        {
            healthTMP.text = health.ToString();

            if (health < maxHealth)
            {
                healthTMP.color = Color.red;
            }
            else if (health >= maxHealth)
            {
                healthTMP.color = Color.white;
            }
        }
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