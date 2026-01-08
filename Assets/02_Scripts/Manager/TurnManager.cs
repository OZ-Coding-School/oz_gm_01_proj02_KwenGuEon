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
    [SerializeField][Tooltip("카드 드로우가 빨라진다")] bool isFaseMode;
    [SerializeField][Tooltip("시작 카드 개수를 정합니다")] int startCardCount;

    [Header("Properties")]
    public bool isLoading; // 로딩중이면 true로 카드와 엔티티 클릭방지
    public bool isMyTurn;

    public enum ETurnMode {Random, my, Other }
    WaitForSeconds StartGameDelay = new WaitForSeconds(0.5f);
    WaitForSeconds turnCardDelay = new WaitForSeconds(0.5f);
    

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

    private static event Action<bool> onTurnStarted;
    public void SubscribeOnTurnStarted(Action<bool> action)
    {
        onTurnStarted += action;
    }
    public void UnsubscribeOnTurnStarted(Action<bool> action)
    {
        onTurnStarted -= action;
    }
    public void TriggerOnTurnStarted(bool isMine)
    {
        onTurnStarted?.Invoke(isMine);
    }

    void GameSetup()
    {
        if (isFaseMode)
            StartGameDelay = new WaitForSeconds(0.05f);


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
        isLoading = true;

        for(int i = 0; i < startCardCount; i++)
        {
            yield return StartGameDelay;
            onAddCard?.Invoke(true);
            yield return StartGameDelay;
            onAddCard?.Invoke(false);
        }
        StartCoroutine(StartTurnCo());
    }
    IEnumerator StartTurnCo()
    {
        isLoading = true;

        if (isMyTurn)
            GameManager.Instance.TurnChangePanel("나의 턴");

        yield return turnCardDelay;
        onAddCard?.Invoke(isMyTurn);
        yield return turnCardDelay;
        isLoading = false;
        TriggerOnTurnStarted(isMyTurn);

    }
    public void EndTurn()
    {
        isMyTurn = !isMyTurn;
        StartCoroutine(StartTurnCo());
    }
}
