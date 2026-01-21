using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSelectedButton : MonoBehaviour
{
    [SerializeField] GameObject morganaSelectedPanel;
    [SerializeField] GameObject elinaSelectedPanel;
    [SerializeField] GameObject hawkSelectedPanel;
    private void Start()
    {
        morganaSelectedPanel.SetActive(false);
        elinaSelectedPanel.SetActive(false);
        hawkSelectedPanel.SetActive(false);
    }
    public void OnClickMorganaSelected()   //Èæ¸¶¹ý»ç
    {
        SoundManager.instance.PlayOnSFX("ButtonSFX2");
        OnHeroIndex(0);
        StartCoroutine(SelectedPanelCo(morganaSelectedPanel));
    }
    public void OnClickElinaSelected()     //¸¶¹ý»ç
    {
        SoundManager.instance.PlayOnSFX("ButtonSFX2");
        OnHeroIndex(1);
        StartCoroutine(SelectedPanelCo(elinaSelectedPanel));
    }
    public void OnClickHawkSelected()      //»ç³É²Û
    {
        SoundManager.instance.PlayOnSFX("ButtonSFX2");
        OnHeroIndex(2);
        StartCoroutine(SelectedPanelCo(hawkSelectedPanel));
    }

    public void OnHeroIndex(int index)
    {
        HeroManager.savedHeroIndex = index;
    }
    IEnumerator SelectedPanelCo(GameObject heroPanel)
    {
        heroPanel.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        heroPanel.SetActive(false);
    }
}
