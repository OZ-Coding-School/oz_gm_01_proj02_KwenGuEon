using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;

public class Entity : MonoBehaviour
{
    public bool isUI;

    [SerializeField] Item item;
    [SerializeField] SpriteRenderer entity;
    [SerializeField] SpriteRenderer character;
    [SerializeField] TMP_Text attackTMP;    
    [SerializeField] TMP_Text heathTMP;
    [SerializeField] GameObject sleepParticle;

    public int attack;
    public int heath;
    public bool isMine;
    public bool isDead;
    public bool isBossOrEmpty;
    public bool attackAble;
    public Vector3 originPos;
    int liveCount;

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
    void OnTurnStarted(bool myTurn)
    {
        if (isBossOrEmpty)
            return;

        if (isMine == myTurn)
            liveCount++;

        sleepParticle.SetActive(liveCount < 1);
    }
    public void Setup(Item item)
    {
        attack = item.attack;
        heath = item.health;

        this.item = item;
        character.sprite = this.item.sprite;
        attackTMP.text = attack.ToString();
        heathTMP.text = heath.ToString();
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

        if(heath <= 0)
        {
            isDead = true;
            return true;
        }
        return false;
    }
    private void OnMouseDown()
    {
        if(isMine)
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
