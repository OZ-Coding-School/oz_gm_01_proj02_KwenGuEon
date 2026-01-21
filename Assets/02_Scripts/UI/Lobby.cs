using UnityEngine;

public class Lobby : MonoBehaviour
{
    void Start()
    {
        SoundManager.instance.PlayOnBGM("LobbyBGM");
    }
}
