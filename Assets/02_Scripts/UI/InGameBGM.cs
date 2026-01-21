using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameBGM : MonoBehaviour
{    
    void Start()
    {
        SoundManager.instance.PlayOnBGM("InGameBGM");
    }   
}
