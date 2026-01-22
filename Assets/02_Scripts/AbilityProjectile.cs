using DG.Tweening;
using System;
using UnityEngine;

public class AbilityProjectile : MonoBehaviour
{
    
    public void Setup(Entity caster, Entity target, Action onHit)
    {

        transform.localScale = Vector3.one * 1f;        
        transform.SetAsLastSibling();

        Vector3 startScreenPos = ScreenPosition(caster);
        transform.position = startScreenPos;

        Vector3 targetScreenPos = ScreenPosition(target);

        Vector3 dir = targetScreenPos - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 180f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        transform.DOMove(targetScreenPos, 0.5f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {                
                onHit?.Invoke();
                Destroy(gameObject);
            });
    }

    Vector3 ScreenPosition(Entity entity)
    {
        if (entity == null) return transform.position;

        if (entity.GetComponent<RectTransform>() != null)
        {            
            return entity.transform.position;
        }
        else
        {           
            return Camera.main.WorldToScreenPoint(entity.transform.position);
        }
    }
}
