using UnityEngine;
public class SortingOrder : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    //[SerializeField] string sortingLayerName = "Default";

    int originOrder;

    public void SetOriginOrder(int originOrder)
    {
        this.originOrder = originOrder;

        if (canvas == null)
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
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = order;
            //canvas.sortingLayerName = sortingLayerName;
        }
    }
}