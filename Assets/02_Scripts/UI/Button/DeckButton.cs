using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeckButton : MonoBehaviour
{
    public void OnClickDeckButton()
    {
        SoundManager.instance.PlayOnSFX("ButtonSFX2");
        SceneManager.LoadScene(2, LoadSceneMode.Additive);
    }
}
