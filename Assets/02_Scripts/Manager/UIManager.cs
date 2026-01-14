using UnityEngine;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{    
    [SerializeField] TurnChangePanel turnChangePanel;
    [SerializeField] ResultPanel resultPanel;
    [SerializeField] GameObject endTurnBtn;

    [Header("ManaUI")]
    [SerializeField] TextMeshProUGUI myManaText;
    [SerializeField] TextMeshProUGUI otherManaText;
    [SerializeField] GameObject[] myManaCost;
    [SerializeField] GameObject[] myManaCostBG;
    [SerializeField] GameObject[] otherManaCost;
    [SerializeField] GameObject[] otherMAnaCostBG;

    void Start()
    {
        UpdateManaUI(true, 0, 0);
        UpdateManaUI(false, 0, 0);

        TurnManager.Instance.UnsubscribeOnTurnStarted(TurnStarted);
        TurnManager.Instance.UnsubscribeOnGameResult(GameResult);
        TurnManager.Instance.UnsubscribeOnManaChange(UpdateManaUI);

        TurnManager.Instance.SubscribeOnTurnStarted(TurnStarted);
        TurnManager.Instance.SubscribeOnGameResult(GameResult);
        TurnManager.Instance.SubscribeOnManaChange(UpdateManaUI);
    }
    void OnDestroy()
    {        
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.UnsubscribeOnTurnStarted(TurnStarted);
            TurnManager.Instance.UnsubscribeOnGameResult(GameResult);
            TurnManager.Instance.UnsubscribeOnManaChange(UpdateManaUI);
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
    public void UpdateManaUI(bool isMine, int currentMana, int maxMana)
    {
        if(isMine)
        {
            if(myManaText != null)
            {
                myManaText.text = $"{currentMana}/{maxMana}";
            }
            if(myManaCost != null && myManaCostBG != null)
            {
                for(int i = 0; i < myManaCost.Length; i++)
                {
                    myManaCost[i].SetActive(i < currentMana);
                    myManaCostBG[i].SetActive(i < maxMana);
                }
            }
        }
        else
        {
            if(otherManaText != null)
            {
                otherManaText.text = $"{currentMana}/{maxMana}";
            }
            if (otherManaCost != null && otherMAnaCostBG != null)
            {
                for (int i = 0; i < otherManaCost.Length; i++)
                {
                    otherManaCost[i].SetActive(i < currentMana);
                    otherMAnaCostBG[i].SetActive(i < maxMana);
                }
            }
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