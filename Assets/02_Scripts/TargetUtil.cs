public static class TargetUtil
{
    public static bool RequiresExternalTarget(Item item)
    {
        if (item == null || item.abilities == null) return false;

        foreach (var a in item.abilities)
        {            
            if (a.targetRule.targetGroup == TargetGroup.Target) return true;
        }
        return false;
    }
}
