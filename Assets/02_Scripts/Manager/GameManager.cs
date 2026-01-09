using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Multiline(10)]
    [SerializeField] string cheatInfo;       

    private void Awake()
    {
        if (Instance == null)
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
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            TurnManager.Instance.EndTurn();
        }
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            CardManager.instance.TryPutCard(false);
        }
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            EntityManager.Instance.DamageBoss(true, 29);

        }
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            EntityManager.Instance.DamageBoss(false, 29);
        }
    }
    public void StartGame()
    {
        StartCoroutine(TurnManager.Instance.StartGameCo());
    }   
}
