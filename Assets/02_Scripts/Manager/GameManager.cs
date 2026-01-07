using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] TurnChangePanel turnChangePanel;
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
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            TurnManager.Instance.TriggerOnAddCard(true);
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            TurnManager.Instance.TriggerOnAddCard(false);
        }
        if(Input.GetKeyDown(KeyCode.Keypad3))
        {
            TurnManager.Instance.EndTurn();
        }
    }
    public void StartGame()
    {
        StartCoroutine(TurnManager.Instance.StartGameCo());
    }
    public void TurnChangePanel(string message)
    {
        turnChangePanel.Show(message);
    }
}
