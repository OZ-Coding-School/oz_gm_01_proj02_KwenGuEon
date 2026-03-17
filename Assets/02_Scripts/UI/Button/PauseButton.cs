using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private Image pauseImage;
    public void OnClickPauseButton()
    {
        SoundManager.instance.PlayOnSFX("ButtonSFX2");
        pauseImage.gameObject.SetActive(true);
    }
}
