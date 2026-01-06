using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("Develop")]
    [SerializeField][Tooltip("선공, 후공을 정합니다")] ETurnMode eTurnMode;
    [SerializeField][Tooltip("시작 카드 개수를 정합니다")] int startCardCount;

    [Header("Properties")]
    public bool isMyTurn;

    public enum ETurnMode {Random, my, Other }
    WaitForSeconds delay = new WaitForSeconds(0.5f);

    private static Action<bool> onAddCard;

    public void SubscribeOnAddCard(Action<bool> action)
    {
        onAddCard += action;
    }
    public void UnsubscribeOnAddCard(Action<bool> action)
    {
        onAddCard -= action;
    }
    public void TriggerOnAddCard(bool isMine)
    {
        onAddCard?.Invoke(isMine);
    }

    void GameSetup()
    {
        switch(eTurnMode)
        {
            case ETurnMode.Random:
                isMyTurn = UnityEngine.Random.Range(0,2) == 0; break;
            case ETurnMode.my:
                isMyTurn = true; break;
            case ETurnMode.Other:
                isMyTurn = false; break;
        }
    }
    public IEnumerator StartGameCo()
    {
        GameSetup();

        for(int i = 0; i < startCardCount; i++)
        {
            yield return delay;
            onAddCard?.Invoke(true);
            yield return delay;
            onAddCard?.Invoke(false);
        }
    }
}
