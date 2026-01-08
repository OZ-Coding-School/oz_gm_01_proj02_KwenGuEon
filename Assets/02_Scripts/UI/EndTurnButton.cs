using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnButton : MonoBehaviour
{
    [SerializeField] Sprite active;
    [SerializeField] Sprite inactive;
    [SerializeField] TextMeshProUGUI btnText;

    void Start()
    {
        Setup(false);
        TurnManager.Instance.UnsubscribeOnTurnStarted(Setup);
        TurnManager.Instance.SubscribeOnTurnStarted(Setup);
    }
    private void OnDestroy()
    {
        TurnManager.Instance.UnsubscribeOnTurnStarted(Setup);        
    }
    public void Setup(bool isActive)
    {
        GetComponent<Image>().sprite = isActive ? active : inactive;
        GetComponent<Button>().interactable = isActive;
        btnText.color = isActive ? new Color32(255, 255, 255, 255) : new Color32(55, 55, 55, 255);
    }
}
