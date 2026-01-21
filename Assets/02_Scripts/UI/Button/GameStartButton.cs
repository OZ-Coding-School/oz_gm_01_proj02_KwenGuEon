using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartButton : MonoBehaviour
{
    [SerializeField] Image CheckItOutImage;

    private void Start()
    {
        CheckItOutImage.gameObject.SetActive(false);
    }
    public void OnClickGameStartButton()
    {
        SoundManager.instance.PlayOnSFX("ButtonSFX2");
        DeckManager.instance.UpdateDeck();

        if (DeckManager.instance.myPlayCardDeck.Count == 30)
        {
            DeckManager.instance.SaveDeck();
            DeckManager.instance.UpdateDeck();

            SceneManager.LoadScene(1);
        }
        else
        {
            StartCoroutine(CheckOutCo());
        }
    }
    IEnumerator CheckOutCo()
    {
        CheckItOutImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.0f);
        CheckItOutImage.gameObject.SetActive(false);
    }
}
