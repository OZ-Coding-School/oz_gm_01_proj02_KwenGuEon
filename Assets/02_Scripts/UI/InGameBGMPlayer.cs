using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameBGMPlayer : MonoBehaviour
{    
    void Start()
    {
        SoundManager.instance.PlayOnBGM("InGameBGM");
    }   
}
