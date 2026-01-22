using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween 필수

public class UITrailEffect : MonoBehaviour
{
    [Header("잔상 설정")]
    [SerializeField] private float spawnInterval = 0.02f;    // 잔상 생성 간격
    [SerializeField] private float fadeDuration = 0.3f;      // 잔상이 사라지는 시간
    [SerializeField] private float startAlpha = 0.5f;        // 잔상의 초기 투명도
    [SerializeField] private Color trailColor = Color.white; // 잔상 색상

    private Image myImage;
    private float timeStack = 0;    

    private void Awake()
    {
        myImage = GetComponent<Image>();
    }

    private void Update()
    {        
        if (myImage == null) return;       

        SpawnGhost();
    }

    void SpawnGhost()
    {
        timeStack += Time.deltaTime;
        if (timeStack >= spawnInterval)
        {
            timeStack = 0;
            CreateAfterimage();
        }
    }

    void CreateAfterimage()
    {        
        GameObject ghostObj = new GameObject($"{gameObject.name}_Ghost");
                
        ghostObj.transform.SetParent(transform.parent);
                
        ghostObj.transform.position = transform.position;
        ghostObj.transform.rotation = transform.rotation;
        ghostObj.transform.localScale = transform.localScale;
                
        ghostObj.transform.SetSiblingIndex(transform.GetSiblingIndex());
                
        Image ghostImg = ghostObj.AddComponent<Image>();
        ghostImg.sprite = myImage.sprite;
        ghostImg.color = myImage.color;
        ghostImg.raycastTarget = false;
                
        Color c = trailColor;
        c.a = startAlpha;
        ghostImg.color = c;
        
        ghostImg.DOFade(0f, fadeDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => Destroy(ghostObj));
    }
}