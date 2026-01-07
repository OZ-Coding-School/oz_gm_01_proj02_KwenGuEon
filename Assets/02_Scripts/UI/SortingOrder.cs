using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SortingOrder : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    //[SerializeField] string sortingLayerName = "Default";

    int originOrder;

    public void SetOriginOrder(int originOrder)
    {
        this.originOrder = originOrder;

        if(canvas == null)
            canvas = GetComponent<Canvas>();

        canvas.overrideSorting = true;
        canvas.sortingOrder = originOrder;
        //canvas.sortingLayerName = sortingLayerName;
    }
    public void SetMostFrontOrder(bool isMostFront)
    {
        SetOrder(isMostFront ? 100 : originOrder);
    }
    public void SetOrder(int order)
    {
        if(canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = order;
            //canvas.sortingLayerName = sortingLayerName;
        }
    }
    
}
/*public class Order : MonoBehaviour
{
    [SerializeField] Renderer[] backRenderers;
    [SerializeField] Renderer[] middleRenderers;
    [SerializeField] string sortingLayerName;
    int originOrder;

    public void SetOriginOrder(int originOrder)
    {
        this.originOrder = originOrder;
        SetOrder(originOrder);
    }
    public void SetMostFrontOrder(bool isMostFront)
    {
        SetOrder(isMostFront ? 100 : originOrder);
    }

    public void SetOrder(int order)
    {
        int mulOrder = order * 10;

        foreach(var renderer in backRenderers)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder;
        }

        foreach(var renderer in middleRenderers)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder + 1;
        }
    }
}
*/
