using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    // GameManager에 있던 UI 변수들을 이리로 이사시킵니다.
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
        // 구독 해제 (메모리 누수 방지)
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

    // 게임 결과가 나오면 할 일
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