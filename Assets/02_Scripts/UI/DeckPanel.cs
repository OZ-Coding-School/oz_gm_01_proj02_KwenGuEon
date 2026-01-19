using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeckPanel : MonoBehaviour
{
    public ItemSO itemSO;
    public List<GameObject> deckPages;    
    int pageIndex = 0;

    public Button frontPageButton;
    public Button backPageButton;

    public GameObject pageParent;
    public GameObject pagePrefab;
    public GameObject cardPrefab;

    void Start()
    {
        InitializeRecipePanel();      

        UpdatePageButton();
    }

    //덱 설정창을 여는 버튼 눌렀을시
    public void OnDeckButtonClick()
    {
        if (pageIndex == 0)
        {
            frontPageButton.gameObject.SetActive(false);
        }
        if (pageIndex + 1 == deckPages.Count)
        {
            backPageButton.gameObject.SetActive(false);
        }

        deckPages[pageIndex].SetActive(true);
        gameObject.SetActive(true);
    }

    //앞(이전)페이지 버튼 누를때
    public void OnFrontPageButtonClick()
    {
        if (deckPages.Count == 0 || pageIndex <= 0) return;

        deckPages[pageIndex].SetActive(false);
        pageIndex--;
        if (pageIndex < 0) pageIndex = 0;

        deckPages[pageIndex].SetActive(true);

        UpdatePageButton();
    }

    //뒷(다음)페이지 버튼 눌렀을때
    public void OnBackPageButtonClick()
    {
        if (deckPages.Count == 0 || pageIndex >= deckPages.Count - 1) return;

        deckPages[pageIndex].gameObject.SetActive(false);
        pageIndex++;
        deckPages[pageIndex].SetActive(true);

        UpdatePageButton();
    }
    void UpdatePageButton()
    {
        if (frontPageButton != null)
            frontPageButton.gameObject.SetActive(pageIndex > 0);

        if (backPageButton != null)
            backPageButton.gameObject.SetActive(pageIndex < deckPages.Count - 1);
    }

    //나가기 버튼 눌렀을때
    public void OnExitButtonClick()
    {
        deckPages[pageIndex].SetActive(false);
        if(DeckManager.instance != null)
        {
            DeckManager.instance.SaveDeck();
            DeckManager.instance.UpdateDeck();
        }

        SceneManager.UnloadSceneAsync(2);
    }

    //패널 초기화
    void InitializeRecipePanel()
    {
        int slotsPerPage = 8;
        GameObject tempPage = null;

        if (itemSO == null || itemSO.items == null) return;

        List<Item> sortList = new List<Item>(itemSO.items);

        sortList.Sort((a, b) =>
        {
            if(a.cardCost != b.cardCost)
            {
                return a.cardCost.CompareTo(b.cardCost);
            }
            return a.cardID.CompareTo(b.cardID);
        });


        for (int i = 0; i < sortList.Count; i++)
        {
            int slotIndex = i % slotsPerPage;

            if (slotIndex == 0)
            {
                tempPage = Instantiate(pagePrefab, pageParent.transform);
                deckPages.Add(tempPage);

                if (deckPages.Count == 1)
                {
                    tempPage.SetActive(true);
                }
                else
                {
                    tempPage.SetActive(false);
                }
            }

            Transform slotTransform = tempPage.transform.GetChild(slotIndex);
            slotTransform.gameObject.SetActive(true);

            GameObject newCard = Instantiate(cardPrefab, slotTransform);

            newCard.transform.localPosition = Vector3.zero;
            newCard.transform.localScale = Vector3.one;
            newCard.transform.localRotation = Utils.QI;

            var cardData = newCard.GetComponent<Card>();

            if (cardData != null)
            {
                cardData.Setup(sortList[i], true, true);
                cardData.enabled = false;
            }
            Button cardBtn = newCard.GetComponent<Button>();

            if (cardBtn != null)
            {
                int currentId = sortList[i].cardID;
                cardBtn.onClick.AddListener(() => { DeckManager.instance.AddCardDeck(currentId); });
            }
        }
    }
}