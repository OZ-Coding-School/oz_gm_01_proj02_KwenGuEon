using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeroSelectPanelButton : MonoBehaviour
{
    public void OnClickGoLobby()
    {
        SoundManager.instance.PlayOnSFX("ButtonSFX2");
        SceneManager.UnloadSceneAsync(3);
    }
    public void OnClickGoHeroSelectedPanel()        
    {
        SoundManager.instance.PlayOnSFX("ButtonSFX2");
        SceneManager.LoadScene(3, LoadSceneMode.Additive);
    }
}
