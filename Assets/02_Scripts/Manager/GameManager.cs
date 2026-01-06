using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        StartGame();
    }

    void Update()
    {
#if UNITY_EDITOR
        InputcheatKey();
#endif
    }
    void InputcheatKey()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            TurnManager.Instance.TriggerOnAddCard(true);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            TurnManager.Instance.TriggerOnAddCard(false);
        }
    }
    public void StartGame()
    {
        StartCoroutine(TurnManager.Instance.StartGameCo());
    }
}
