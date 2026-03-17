using UnityEngine;

public class LobbyBGMPlayer : MonoBehaviour
{
    void Start()
    {
        SoundManager.instance.PlayOnBGM("LobbyBGM");
    }
}
