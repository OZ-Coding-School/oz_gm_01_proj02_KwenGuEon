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

    public int attack;
    public int heath;
    public bool isMine;
    public bool isBossOrEmpty;
    public Vector3 originPos;


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
}
