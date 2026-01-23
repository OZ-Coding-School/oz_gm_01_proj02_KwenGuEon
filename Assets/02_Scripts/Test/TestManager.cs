using System.Collections;
using UnityEngine;

public class VFXTestManager : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private Camera vfxCamera;
    [SerializeField] private ItemSO itemData;
    [SerializeField] private RectTransform testHero;

    private int currentIndex = 0; // 현재 보고 있는 카드 번호
    private GameObject currentVfxObj; // 현재 떠있는 이펙트

    private void Start()
    {
        ShowVFX(currentIndex);
    }

    private void Update()
    {
        // [오른쪽 화살표]: 다음 이펙트
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex++;
            if (currentIndex >= itemData.items.Length) currentIndex = 0;
            ShowVFX(currentIndex);
        }

        // [왼쪽 화살표]: 이전 이펙트
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = itemData.items.Length - 1;
            ShowVFX(currentIndex);
        }

        // [R 키]: 현재 이펙트 다시 재생 (크기 바꾼거 확인할 때)
        if (Input.GetKeyDown(KeyCode.R))
        {
            ShowVFX(currentIndex);
        }
    }

    void ShowVFX(int index)
    {
        // 기존 거 삭제
        if (currentVfxObj != null) Destroy(currentVfxObj);

        var item = itemData.items[index];
        if (item.VFXPrefab == null)
        {
            Debug.Log($"[{index}] {item.name}: 이펙트 없음 (Pass)");
            return;
        }

        // 콘솔에 현재 인덱스와 이름 출력 (인스펙터에서 찾기 쉽게)
        Debug.Log($"[{index}] {item.name} | Scale: {item.vfxScale}");

        // 생성 (좌표 강제 고정 방식)
        Vector3 spawnPos = vfxCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
        currentVfxObj = Instantiate(item.VFXPrefab, spawnPos, Quaternion.identity);

        // 스케일 안전장치 & 적용
        if (item.vfxScale <= 0.01f) item.vfxScale = 1.0f;
        currentVfxObj.transform.localScale = Vector3.one * item.vfxScale;

        SetLayerRecursively(currentVfxObj, LayerMask.NameToLayer("UI_VFX"));
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}