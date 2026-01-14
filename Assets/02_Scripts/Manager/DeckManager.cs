using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class DeckData
{
    public List<int> cardIds;
}
public class DeckManager : MonoBehaviour
{
    public static DeckManager instance;

    public ItemSO cardDeckBase;

    public List<int> myCardDeck = new List<int>();
    public List<int> otherCardDeck = new List<int>();

    public List<Item> myPlayCardDeck = new List<Item>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        LoadDeck();
    }

    public void AddCardDeck(int cardId)
    {
        if (myCardDeck.Count >= 30)
        {
            return;
        }

        myCardDeck.Add(cardId);
        SaveDeck();
    }
    public void RemoveCardDeck(int cardId)
    {
        if (myCardDeck.Contains(cardId))
        {
            myCardDeck.Remove(cardId);
            SaveDeck();
        }
    }
    public void LoadDeck()
    {
        string path = Application.persistentDataPath + "/myDeck.json";

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            DeckData data = JsonUtility.FromJson<DeckData>(json);

            myCardDeck = data.cardIds;

            UpdateDeck();
        }
    }
    public void SaveDeck()
    {
        DeckData data = new DeckData();
        data.cardIds = myCardDeck;

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + "/myDeck.json", json);

        Debug.Log("µ¦ ÀúÀå ¿Ï·á");
    }
    public void UpdateDeck()
    {
        myPlayCardDeck.Clear();

        foreach (int id in myCardDeck)
        {
            Item card = cardDeckBase.GetCardID(id);
            if (card != null)
            {
                myPlayCardDeck.Add(card);
            }
        }
    }
}