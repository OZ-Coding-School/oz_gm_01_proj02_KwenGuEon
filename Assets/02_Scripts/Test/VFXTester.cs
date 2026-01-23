using System.Collections;
using UnityEngine;

public class VFXTester : MonoBehaviour
{

    [SerializeField] ItemSO item;
    [SerializeField] EffectManager effectManager;
    [SerializeField] Entity entity;

    [SerializeField] float interval = 2.0f;

    [SerializeField] bool isAutoStart = false;
    void Start()
    {
        if (isAutoStart) StartCoroutine(TestAllVFXCo());
    }
    public void RunTest()
    {
        StartCoroutine(TestAllVFXCo());
    }

    IEnumerator TestAllVFXCo()
    {
        Debug.Log("=== VFX 슬라이드쇼 시작 ===");

        foreach (var item in item.items)
        {
            // 이펙트 없는 카드는 패스
            if (item.VFXPrefab == null) continue;

            Debug.Log($"[테스트 중] 카드명: {item.name} | Scale: {item.vfxScale}");

            //EffectManager의 SpawnVFX를 강제로 호출 (마나, 턴 무시)
            //(아까 만든 함수: Prefab, Pos, Scale, Target 순서)

            //SpawnVFX가 private이라면 public으로 잠시 바꾸거나, 
            //EffectManager에 Test 함수를 하나 뚫어주세요.
            //여기서는 EffectManager 코드를 안 건드리고, ApplyEffect랑 비슷한 방식으로 호출한다고 가정합니다.

            //만약 SpawnVFX가 private이라서 안 보인다면?
            //EffectManager.cs 가서 void SpawnVFX(...) 앞에 'public'만 붙여주세요!

            effectManager.SpawnVFX(item.VFXPrefab, entity.transform.position, item.vfxScale, entity);

            yield return new WaitForSeconds(interval);
        }

        Debug.Log("===  테스트 종료 ===");
    }
}
