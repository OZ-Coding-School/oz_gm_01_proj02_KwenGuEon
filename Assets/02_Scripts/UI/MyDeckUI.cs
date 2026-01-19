using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyDeckUI : MonoBehaviour
{
    public Transform myDeckParent;
    public GameObject deckCardPrefab;
    public TextMeshProUGUI cardCount;

    private void Start()
    {
        DeckManager.instance.UnsubscribeOnDeckChanged(SelectedCardDeck);
        DeckManager.instance.SubscribeOnDeckChanged(SelectedCardDeck);
        DeckManager.instance.UpdateDeck();
    }

    public void SelectedCardDeck(List<Item> currentDeck)
    {
        foreach(Transform card in myDeckParent)
        {
            Destroy(card.gameObject);
        }

        foreach(Item item in currentDeck)
        {
            GameObject newCard = Instantiate(deckCardPrefab, myDeckParent);
            Button btn = newCard.GetComponent<Button>();
            Card cardData = newCard.GetComponent<Card>();
            if (cardData != null)
            {
                cardData.Setup(item, true, true);
                cardData.enabled = false;
            }
            if (btn != null)
            {
                int targetId = item.cardID;

                btn.onClick.AddListener(() => DeckManager.instance.RemoveCardDeck(targetId));
            }            
        }

        if(cardCount != null)
        {
            cardCount.text = $"{currentDeck.Count} / 30";
        }
    }
}
