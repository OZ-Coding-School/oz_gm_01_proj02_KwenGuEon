using DG.Tweening;
using System;
using UnityEngine;

public class AbilityProjectile : MonoBehaviour
{

    public void Setup(Vector3 startPos, Entity target, Action onHit)
    {
        transform.position = startPos;

        transform.DOMove(target.transform.position, 0.5f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                SoundManager.instance.PlayOnSFX("Arcane_Arrow_Targeted_Build_03");
                onHit?.Invoke();
                Destroy(gameObject);
            });
    }
}
