using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class ResultPanel : MonoBehaviour
{
    [SerializeField] GameObject victoryPanel;
    [SerializeField] GameObject losePanel;

    [SerializeField] TMP_Text resultTMP;

    public void ShowVictory()
    {
        losePanel.SetActive(false);
        victoryPanel.SetActive(true);
        transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutQuad);
    }
    public void Showlose()
    {
        victoryPanel.SetActive(false);
        losePanel.SetActive(true);
        transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutQuad);
    }

    public void GoLobby()
    {
        SceneManager.LoadScene(0);
    }
    public void Restart()
    {
        SceneManager.LoadScene(1);
    }

    private void Start()
    {
        ScaleZero();
    }


    [ContextMenu("ScaleOne")]
    void ScaleOne()
    {
        transform.localScale = Vector3.one;
    }
    [ContextMenu("ScaleZero")]
    void ScaleZero()
    {
        transform.localScale = Vector3.zero;
    }
}
