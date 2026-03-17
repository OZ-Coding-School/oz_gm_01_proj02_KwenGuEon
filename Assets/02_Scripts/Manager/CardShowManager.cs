using System.Collections;
using UnityEngine;

public class CardShowManager : MonoBehaviour
{
    public static CardShowManager Instance;

    [Header("Settings")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform showPosition;

    [SerializeField] private float displayDuration = 1.0f;

    // [수정] 카드 확대 크기 설정 (기본 2.5배, 인스펙터에서 조절 가능)
    [SerializeField] private float targetScale = 2.5f;

    private GameObject currentCardObj;
    private Coroutine currentCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void ShowCard(Item item, bool isMine)
    {
        if (item == null || cardPrefab == null || showPosition == null) return;

        if (currentCardObj == null)
        {
            currentCardObj = Instantiate(cardPrefab, showPosition);

            CanvasGroup cg = currentCardObj.GetComponent<CanvasGroup>();
            if (cg == null) cg = currentCardObj.AddComponent<CanvasGroup>();

            cg.blocksRaycasts = false;
            cg.interactable = false;
        }

        if (currentCoroutine != null) StopCoroutine(currentCoroutine);

        currentCardObj.SetActive(true);
        currentCardObj.transform.localPosition = Vector3.zero;
        currentCardObj.transform.localScale = Vector3.zero;

        Card card = currentCardObj.GetComponent<Card>();
        if (card != null)
        {
            card.Setup(item, true);
        }

        currentCoroutine = StartCoroutine(ShowProcessCo(currentCardObj));
    }

    IEnumerator ShowProcessCo(GameObject obj)
    {
        float timer = 0f;
        float animTime = 0.25f;

        while (timer < animTime)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(0f, targetScale, timer / animTime); // [수정] 1f -> targetScale
            if (obj != null) obj.transform.localScale = Vector3.one * scale;
            yield return null;
        }
        if (obj != null) obj.transform.localScale = Vector3.one * targetScale; // [수정]

        yield return new WaitForSeconds(displayDuration);

        timer = 0f;
        while (timer < animTime)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(targetScale, 0f, timer / animTime); // [수정]
            if (obj != null) obj.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        if (obj != null)
        {
            obj.SetActive(false);
        }
    }
}