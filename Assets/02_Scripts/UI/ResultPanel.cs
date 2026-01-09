using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class ResultPanel : MonoBehaviour
{
    [SerializeField] GameObject victoryPanel;
    [SerializeField] GameObject losePanel;

    [SerializeField] TMP_Text resultTMP;
    [SerializeField] PanelDimmer dimmer;

    

    public void ShowVictory()
    {
        losePanel.SetActive(false);
        victoryPanel.SetActive(true);
        transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutQuad);
        dimmer.ShowDimmer();
    }
    public void Showlose()
    {
        victoryPanel.SetActive(false);
        losePanel.SetActive(true);
        transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutQuad);
        dimmer.ShowDimmer();
    }

    public void GoLobby()
    {
        dimmer.HideDimmer();
        SceneManager.LoadScene(0);
    }
    public void Restart()
    {
        dimmer.HideDimmer();
        SceneManager.LoadScene(1);
    }

    private void Start()
    {
        ScaleZero();
        if(dimmer == null)
            dimmer = FindAnyObjectByType<PanelDimmer>();
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
