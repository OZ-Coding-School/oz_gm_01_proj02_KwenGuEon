using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{    
    [SerializeField] TurnChangePanel turnChangePanel;
    [SerializeField] ResultPanel resultPanel;
    [SerializeField] GameObject endTurnBtn;

    void Start()
    {
        TurnManager.Instance.UnsubscribeOnTurnStarted(TurnStarted);
        TurnManager.Instance.UnsubscribeOnGameResult(GameResult);

        TurnManager.Instance.SubscribeOnTurnStarted(TurnStarted);
        TurnManager.Instance.SubscribeOnGameResult(GameResult);
    }
    void OnDestroy()
    {        
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.UnsubscribeOnTurnStarted(TurnStarted);
            TurnManager.Instance.UnsubscribeOnGameResult(GameResult);
        }
    }    
    void TurnStarted(bool isMyTurn)
    {
        if (isMyTurn)
        {
            turnChangePanel.Show("나의 턴");
            endTurnBtn.SetActive(true);
        }       
    }

    // 게임 결과 이후 호출
    void GameResult(bool isWin)
    {
        endTurnBtn.SetActive(false);
        StartCoroutine(ShowResultCo(isWin));
    }

    IEnumerator ShowResultCo(bool isWin)
    {
        yield return new WaitForSeconds(2.0f);
        if (isWin) resultPanel.ShowVictory();
        else resultPanel.ShowLose();
    }
}