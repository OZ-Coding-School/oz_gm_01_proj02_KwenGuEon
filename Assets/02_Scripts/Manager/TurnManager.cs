using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    private readonly List<Action> myEndTurnRollback = new List<Action>();
    private readonly List<Action> otherEndTurnRollback = new List<Action>();
    private void Awake()
    {
        if (Instance == null)
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

    [Header("Mana System")]
    public int myMana;
    public int myMaxMana;
    public int otherMana;
    public int otherMaxMana;

    public enum ETurnMode { Random, my, Other }
    WaitForSeconds StartGameDelay = new WaitForSeconds(0.5f);
    WaitForSeconds turnCardDelay = new WaitForSeconds(0.5f);


    private event Action< bool ,int, int> onManaChange;
    #region onManaChange
    public void SubscribeOnManaChange(Action< bool, int, int> action)
    {
        onManaChange += action;
    }
    public void UnsubscribeOnManaChange(Action< bool, int, int> action)
    {
        onManaChange -= action;
    }
    public void TriggerOnManaChange(bool isMine, int mana, int maxMana)
    {
        onManaChange?.Invoke(isMine, mana, maxMana);
    }
    #endregion
    private event Action<bool> onAddCard;
    #region onAddCard
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
    #endregion
    private event Action<bool> onTurnStarted;
    #region onTurnStarted
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
    #endregion
    private event Action<bool> onGameResult;
    #region onGameResult
    public void SubscribeOnGameResult(Action<bool> action)
    {
        onGameResult += action;
    }
    public void UnsubscribeOnGameResult(Action<bool> action)
    {
        onGameResult -= action;
    }
    public void TriggerOnGameResult(bool isWin)
    {
        onGameResult?.Invoke(isWin);
    }
    #endregion    

    void GameSetup()
    {
        myMana = 0; myMaxMana = 0;
        otherMana = 0; otherMaxMana = 0;

        if (isFaseMode)
            StartGameDelay = new WaitForSeconds(0.05f);

        switch (eTurnMode)
        {
            case ETurnMode.Random:
                isMyTurn = UnityEngine.Random.Range(0, 2) == 0; break;
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

        for (int i = 0; i < startCardCount; i++)
        {
            yield return StartGameDelay;
            TriggerOnAddCard(true);
            yield return StartGameDelay;
            TriggerOnAddCard(false);
        }
        StartCoroutine(StartTurnCo());
    }
    IEnumerator StartTurnCo()
    {
        isLoading = true;

        if(isMyTurn)
        {
            if (myMaxMana < 10) myMaxMana++;
            myMana = myMaxMana;
            TriggerOnManaChange(true, myMana, myMaxMana);
        }
        else
        {
            if(otherMaxMana < 10) otherMaxMana++;
            otherMana = otherMaxMana;
            TriggerOnManaChange(false, otherMana, otherMaxMana);
        }

        TriggerOnTurnStarted(isMyTurn);
        yield return turnCardDelay;
        TriggerOnAddCard(isMyTurn);
        yield return turnCardDelay;
        isLoading = false;        
    }
    public bool UseMana(bool isMine, int cost)
    {
        if(isMine)
        {
            if(myMana >= cost)
            {
                myMana -= cost;
                TriggerOnManaChange(true, myMana, myMaxMana);
                return true;
            }            
        }
        else
        {
            if(otherMana >= cost)
            {
                otherMana -= cost;
                TriggerOnManaChange(false, otherMana, otherMaxMana);
                return true;
            }
        }
        return false;
    }
    public void GainTempMana(bool isMine, int amount)
    {
        if (amount <= 0) return;

        if (isMine) myMana += amount;
        else otherMana += amount;

        RegisterEndTurnRollback(isMine, () =>
        {
            if (isMine) myMana -= amount;
            else otherMana -= amount;
        });
    }
    public void RegisterEndTurnRollback(bool isMine, Action rollback)
    {
        if (rollback == null) return;
        if (isMine) myEndTurnRollback.Add(rollback);
        else otherEndTurnRollback.Add(rollback);
    }
    public void GainEmptyMana(bool isMine, int amount)
    {
        if (amount <= 0) return;

        if(isMine)
        {
            myMaxMana += amount;
        }
        else
        {
            otherMaxMana += amount;
        }
    }
    private void ExecuteEndTurnRollbacks(bool isMine)
    {
        var list = isMine ? myEndTurnRollback : otherEndTurnRollback;

        for (int i = 0; i < list.Count; i++)
            list[i]?.Invoke();

        list.Clear();
    }
    public void EndTurn()
    {
        ExecuteEndTurnRollbacks(isMyTurn);

        isMyTurn = !isMyTurn;
        StartCoroutine(StartTurnCo());
    }
}
