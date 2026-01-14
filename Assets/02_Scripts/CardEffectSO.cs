using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CardEffectSO : ScriptableObject
{
    public bool needTarget;

    public abstract void ActivatedEffect(Entity caster, Entity target);
}
