using DG.Tweening;
using TMPro;
using UnityEngine;

public class Damage : MonoBehaviour
{

    [SerializeField] TMP_Text damageTMP;

    public void Damaged(int damage)
    {
        if (damage <= 0)
            return;

        damageTMP.text = $"-{damage}";

        Sequence sequence = DOTween.Sequence()
            .Append(transform.DOScale(Vector3.one * 2.9f, 0.5f).SetEase(Ease.InOutBack))
            .AppendInterval(1.2f)
            .Append(transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutBack))
            .OnComplete(() => Destroy(gameObject));
    }
}
