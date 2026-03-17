using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PausePanelButton : MonoBehaviour
{
    [SerializeField] private Image pauseImage;
    private void Start()
    {
        pauseImage.gameObject.SetActive(false);
    }
    public void OnClickGoLobby()
    {
        SoundManager.instance.PlayOnSFX("ButtonSFX2");
        SceneManager.LoadScene(0);
    }
    public void OnClickGameResume()
    {
        SoundManager.instance.PlayOnSFX("ButtonSFX2");
        pauseImage.gameObject.SetActive(false);
    }
    public void OnClickExit()
    {
        SoundManager.instance.PlayOnSFX("ButtonSFX2");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
